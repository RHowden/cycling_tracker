using CyclingTracker.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CyclingTracker.Api.IntegrationTests.Infrastructure;

[CollectionDefinition("Api")]
public class ApiCollection : ICollectionFixture<IntegrationFactory> { }

public sealed class IntegrationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public PostgresFixture Postgres { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(o => o.UseNpgsql(Postgres.ConnectionString));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = FakeAuthHandler.AuthenticationScheme;
                options.DefaultChallengeScheme = FakeAuthHandler.AuthenticationScheme;
            })
            .AddScheme<FakeAuthHandlerOptions, FakeAuthHandler>(
                FakeAuthHandler.AuthenticationScheme, _ => { });
        });

    public Task InitializeAsync() => Postgres.InitializeAsync();
    public new Task DisposeAsync() => Postgres.DisposeAsync();
}

