using CyclingTracker.Api.Data;
using CyclingTracker.Api.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace CyclingTracker.Api.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddAuth(this IServiceCollection services)
    {
        services
            .AddIdentityApiEndpoints<Athlete>()
            .AddEntityFrameworkStores<AppDbContext>();

        // Re-check the user and security stamp on every request, so deleted or
        // password-changed accounts lose access immediately (default: every 30 minutes).
        services.Configure<SecurityStampValidatorOptions>(options =>
            options.ValidationInterval = TimeSpan.Zero);

        services.ConfigureApplicationCookie(options =>
        {
            // API clients get a 401, not a redirect to a login page.
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };

            UseCrossSiteCookie(options.Cookie);
        });

        foreach (var scheme in new[] { IdentityConstants.ExternalScheme, IdentityConstants.TwoFactorUserIdScheme })
        {
            services.Configure<CookieAuthenticationOptions>(scheme, options =>
                UseCrossSiteCookie(options.Cookie));
        }

        return services;
    }

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGroup("/api/auth").MapIdentityApi<Athlete>();
        return endpoints;
    }

    // Needed while the front end and API are on different origins.
    // Revisit when moving to a single origin (see the CSRF item in the review).
    private static void UseCrossSiteCookie(CookieBuilder cookie)
    {
        cookie.SameSite = SameSiteMode.None;
        cookie.SecurePolicy = CookieSecurePolicy.Always; // SameSite=None requires Secure
    }
}