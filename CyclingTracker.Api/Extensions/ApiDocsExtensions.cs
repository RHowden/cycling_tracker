using Scalar.AspNetCore;

namespace CyclingTracker.Api.Extensions;

public static class ApiDocsExtensions
{
    public static IServiceCollection AddApiDocs(this IServiceCollection services)
    {
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        services.AddOpenApi();
        return services;
    }

    public static IEndpointRouteBuilder MapApiDocs(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapOpenApi();
        endpoints.MapScalarApiReference();
        return endpoints;
    }
}