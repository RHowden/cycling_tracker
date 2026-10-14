# 1. Front end
FROM node:22-alpine AS web
WORKDIR /web
COPY cycling-tracker-web/package*.json ./
RUN npm ci
COPY cycling-tracker-web/ ./
RUN npm run build

# 2. API
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api
WORKDIR /src
COPY CyclingTracker.Api/CyclingTracker.Api.csproj CyclingTracker.Api/
COPY FitFileParsing/FitFileParsing.csproj FitFileParsing/
RUN dotnet restore CyclingTracker.Api/CyclingTracker.Api.csproj
COPY CyclingTracker.Api/ CyclingTracker.Api/
COPY FitFileParsing/ FitFileParsing/
RUN dotnet publish CyclingTracker.Api/CyclingTracker.Api.csproj -c Release -o /app --no-restore

# 3. Runtime: API plus the built React app in wwwroot
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=api /app .
COPY --from=web /web/dist ./wwwroot
ENTRYPOINT ["dotnet", "CyclingTracker.Api.dll"]