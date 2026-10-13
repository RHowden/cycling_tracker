using CyclingTracker.Api.Data;
using CyclingTracker.Api.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

namespace CyclingTracker.Api.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddAuth(this IServiceCollection services)
    {
        services
            .AddIdentityApiEndpoints<Athlete>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.ConfigureApplicationCookie(options =>
        {
            // Prevent the 404 redirect loop. Force a clean 401 instead so you can see if auth fails.
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };

            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // SameSite=None requires Secure
        });

        services.Configure<CookieAuthenticationOptions>(IdentityConstants.ExternalScheme, options =>
        {
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });

        // 🟢 ADD THIS: Configure the Two-Factor cookie to allow cross-origin deletion
        services.Configure<CookieAuthenticationOptions>(IdentityConstants.TwoFactorUserIdScheme, options =>
        {
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });

        return services;
    }

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGroup("/api/auth").MapIdentityApi<Athlete>();
        return endpoints;
    }
}