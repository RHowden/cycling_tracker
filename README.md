# Cycling Tracker

Upload a FIT file from a ride and get a dashboard of your training plus a detailed view of each session. React front end, .NET API, Postgres.

I built it as a portfolio project while moving from React into C#, so most of the interesting parts are on the API side.

## Features

- Register and log in
- Ingest `.fit` files into sessions
- Dashboard of training over a chosen period
- Session detail with heart rate, and power charts

## Stack

- **Front end:** React, TypeScript, Vite
- **API:** ASP.NET Core, EF Core
- **Database:** PostgreSQL
- **FIT parsing:** standalone `FitFileParsing` library
- **Tests:** xUnit (API and parser), front end tests
- **Infra:** Docker (multi-stage build), GitHub Actions

## Layout

```
CyclingTracker.Api                    Web API
CyclingTracker.Api.UnitTests          Unit tests
CyclingTracker.Api.IntegrationTests   Integration tests
FitFileParsing                        FIT parsing library
FitFileParsing.Tests                  Parser tests
cycling-tracker-web                   React front end
```

## Design decisions

- **Cookie auth with `SameSite=Lax`.** The Vite dev proxy puts the front end and API on one origin, so there's no CORS configuration to maintain. The client handles 401s in one place.
- **One error contract.** Service results map to HTTP responses through a single `ToActionResult()` (for example, 409 for a duplicate upload).
- **Auth is enforced by a test.** `ControllerStructureTests` fails the build if a controller is missing `[Authorize]`.
- **Dropouts are nulls, not zeros.** Sensor gaps show up as gaps in the data instead of false readings.
- **Deployable as is.** Multi-stage Dockerfile, a full compose file with a Postgres healthcheck, and migrations on startup.

## Running it

You'll need the .NET SDK, Node and Docker.

**Everything in Docker**

```bash
cp .env.example .env
docker compose -f docker-compose.full.yml up --build
```

**Local development**

```bash
cp .env.example .env
docker compose up -d                      # Postgres
dotnet run --project CyclingTracker.Api   # API
cd cycling-tracker-web
npm install
npm run dev                               # front end, proxies to the API
```

## Tests

```bash
dotnet test
cd cycling-tracker-web && npm test
```

## Next

- Apply the `from` / `to` date filters on the sessions endpoint
- Real HTTP-to-database integration tests
- More front end coverage

## Licence

[MIT / whichever you pick]