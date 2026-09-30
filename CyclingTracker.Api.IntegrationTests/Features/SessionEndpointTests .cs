using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using CyclingTracker.Api.Dtos;
using CyclingTracker.Api.Services;
using CyclingTracker.Api.IntegrationTests.Infrastructure;
using Microsoft.Extensions.Options;

namespace CyclingTracker.Api.IntegrationTests;

public class SessionEndpointTests : IClassFixture<IntegrationFactory>, IDisposable
{
    private readonly HttpClient _client;
    private readonly Mock<ISessionQueryService> _queryMock = new();
    private readonly WebApplicationFactory<Program> _factory;

    public SessionEndpointTests(IntegrationFactory factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<ISessionQueryService>();
                services.AddScoped(_ => _queryMock.Object);
            }));

        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _factory.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetSessions_ValidUser_Returns200Ok_WithJsonData()
    {
        // Arrange
        var optionsMonitor = _factory.Services
            .GetRequiredService<IOptionsMonitor<FakeAuthHandlerOptions>>();

        // 2. Extract the active options instance tied to your specific scheme name
        var currentOptions = optionsMonitor.Get(FakeAuthHandler.AuthenticationScheme);
        var expectedUserId = Guid.Parse(currentOptions.DefaultUserId);
        var mockResult = new List<SessionIndexDto>
        {
            new(Guid.NewGuid(), DateTime.UtcNow, "Cycling", 3600, 25000, 140, 200)
        };


        _queryMock
            .Setup(s => s.GetUserSessionsAsync(expectedUserId))
            .ReturnsAsync(mockResult);

        // Act
        var response = await _client.GetAsync($"/api/sessions?userId={expectedUserId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<List<SessionIndexDto>>();
        Assert.NotNull(content);
        Assert.Single(content);
    }
}
