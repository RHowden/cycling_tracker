using CyclingTracker.Api.Services.AthleteZones;
using CyclingTracker.Api.Services.FileImport;
using CyclingTracker.Api.Services.SessionDeletion;
using CyclingTracker.Api.Services.SessionQuery;
using FitFileParsing;

namespace CyclingTracker.Api.Extensions;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IFitImportService, FitImportService>();
        services.AddScoped<IFitFileParser, FitFileParser>();
        services.AddScoped<ISessionQueryService, SessionQueryService>();
        services.AddScoped<ISessionDeletionService, SessionDeletionService>();
        services.AddScoped<IAthleteZonesService, AthleteZonesService>();

        return services;
    }
}