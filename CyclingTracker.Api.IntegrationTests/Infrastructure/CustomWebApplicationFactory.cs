using CyclingTracker.Api.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CyclingTracker.Api.IntegrationTests;

// EntryPoint points to your main Program or Startup class
public class CustomWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint> where TEntryPoint : class
{
    public Mock<IFitImportService> FitImportServiceMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real service registration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IFitImportService));
            if (descriptor != null) services.Remove(descriptor);

            // Inject our mock factory instance so we can control it inside our tests
            services.AddScoped(_ => FitImportServiceMock.Object);

             services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = FakeAuthHandler.AuthenticationScheme;
                options.DefaultChallengeScheme = FakeAuthHandler.AuthenticationScheme;
            })
            .AddScheme<FakeAuthHandlerOptions, FakeAuthHandler>(
                FakeAuthHandler.AuthenticationScheme, _ => { });
        });
    }
}
