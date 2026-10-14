using CyclingTracker.Api.Data;
using CyclingTracker.Api.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPersistence(builder.Configuration)
    .AddAuth()
    .AddApplicationServices()
    .AddApiDocs()
    .AddErrorHandling();

builder.Services.AddControllers();

var app = builder.Build();

if (app.Configuration.GetValue<bool>("RunMigrations"))
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

app.UseErrorHandling();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapApiDocs();
}
else
{
    if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
    {
        app.UseHttpsRedirection();
    }
}

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapControllers();

// SPA fallback: serve index.html for client-side routes (/dashboard, /session/123),
// but not for real files (nonfile) and not for /api/* (so unknown API paths get a real 404).
app.MapFallbackToFile("{*path:nonfile:regex(^(?!api(/|$)).*$)}", "index.html");  

app.Run();