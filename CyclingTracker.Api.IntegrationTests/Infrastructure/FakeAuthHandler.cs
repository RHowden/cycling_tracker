using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CyclingTracker.Api.IntegrationTests.Infrastructure;

public class FakeAuthHandlerOptions : AuthenticationSchemeOptions
{
    public string DefaultUserId { get; set; } = Guid.NewGuid().ToString();
}

public class FakeAuthHandler : AuthenticationHandler<FakeAuthHandlerOptions>
{
    public const string AuthenticationScheme = "FakeTestAuth";

    public FakeAuthHandler(
        IOptionsMonitor<FakeAuthHandlerOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) 
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // 1. Check if the test request specified a custom User ID via an HTTP header
        // If not, fall back to our default test user ID
        var userId = Options.DefaultUserId;
        if (Context.Request.Headers.TryGetValue("X-Test-UserId", out var headerValue))
        {
            userId = headerValue.ToString();
        }

        // 2. Build the fake claims identity exactly matching your cookie structure
        var claims = new[] 
        { 
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, "Test Cyclist")
        };
        
        var identity = new ClaimsIdentity(claims, AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

        // 3. Tell ASP.NET Core that authentication successfully passed!
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
