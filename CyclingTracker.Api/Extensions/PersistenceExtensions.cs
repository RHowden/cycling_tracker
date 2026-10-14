using CyclingTracker.Api.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CyclingTracker.Api.Extensions;

public static class PersistenceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? BuildLocalConnectionString(configuration);

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }

    // Local dev fallback: the DB from docker-compose.yml, password from POSTGRES_PASSWORD (.env).
    private static string BuildLocalConnectionString(IConfiguration configuration)
    {
        var password = configuration["POSTGRES_PASSWORD"]
            ?? throw new InvalidOperationException(
                "No database connection configured. Set ConnectionStrings:DefaultConnection, " +
                "or POSTGRES_PASSWORD (e.g. via envFile in .vscode/launch.json) to use the local dev DB.");

        return new NpgsqlConnectionStringBuilder
        {
            Host = "localhost",
            Port = 5432,
            Database = "cycling_tracker",
            Username = "postgres",
            Password = password
        }.ConnectionString;
    }
}