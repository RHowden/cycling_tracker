namespace CyclingTracker.Api.Extensions;

public static class CorsExtensions
{
    public const string FrontendPolicy = "_allowReactApp";

    public static IServiceCollection AddFrontendCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(name: FrontendPolicy, policy =>
            {
                policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173") // <-- Match your exact active React port
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials(); // Required if you use cookies or identity sessions
            });
        });

        return services;
    }
}