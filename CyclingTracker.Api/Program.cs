using CyclingTracker.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPersistence(builder.Configuration)
    .AddAuth()
    .AddApplicationServices()
    .AddFrontendCors()
    .AddApiDocs()
    .AddErrorHandling();

builder.Services.AddControllers();

var app = builder.Build();

app.UseErrorHandling();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapApiDocs();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors(CorsExtensions.FrontendPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapControllers();

app.Run();