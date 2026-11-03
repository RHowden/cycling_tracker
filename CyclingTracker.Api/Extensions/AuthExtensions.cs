using CyclingTracker.Api.Data;
using CyclingTracker.Api.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
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

            ConfigureCookie(options.Cookie);
        });

        foreach (var scheme in new[] { IdentityConstants.ExternalScheme, IdentityConstants.TwoFactorUserIdScheme })
        {
            services.Configure<CookieAuthenticationOptions>(scheme, options =>
                ConfigureCookie(options.Cookie));
        }

        return services;
    }

    /// <summary>
    /// Azure's ingress terminates TLS, so the container only ever sees plain HTTP.
    /// Without this, ASP.NET believes every request is insecure: Secure-policy cookies
    /// lack the Secure flag and generated absolute URLs use http://.
    /// Requires app.UseForwardedHeaders() as the first middleware.
    /// Trusting any sender is safe here because the ingress is the only route into the
    /// container; if it were exposed another way, clients could spoof these headers.
    /// </summary>
    public static IServiceCollection AddAzureForwardHeaders(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(o =>
        {
            o.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
            o.KnownIPNetworks.Clear();
            o.KnownProxies.Clear();
        });

        return services;
    }

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGroup("/api/auth").MapIdentityApi<Athlete>();
        return endpoints;
    }

    private static void ConfigureCookie(CookieBuilder cookie)
    {
        cookie.SameSite = SameSiteMode.Lax;
        cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    }
}

