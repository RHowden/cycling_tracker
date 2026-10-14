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
using CyclingTracker.Api.Shared;
using CyclingTracker.Api.Enums;
using Microsoft.AspNetCore.Mvc;
using CyclingTracker.Api.Services.SessionQuery;
using CyclingTracker.Api.Services.SessionDeletion;

namespace CyclingTracker.Api.IntegrationTests.Features;

public class SessionEndpointTests : IClassFixture<IntegrationFactory>, IDisposable
{
    private readonly HttpClient _client;
    private readonly Mock<ISessionQueryService> _queryMock = new();
    private readonly Mock<ISessionDeletionService> _deleteMock = new();
    private readonly WebApplicationFactory<Program> _factory;

    public SessionEndpointTests(IntegrationFactory factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<ISessionQueryService>();
                services.AddScoped(_ => _queryMock.Object);
                services.AddScoped(_ => _deleteMock.Object);
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
        Guid expectedUserId = GetUserId();
        IReadOnlyList<SessionIndexDto> sessions = new List<SessionIndexDto>
    {
        new(Guid.NewGuid(), DateTime.UtcNow, "Cycling", 3600, 25000, 140, 200)
    };

        _queryMock
            .Setup(s => s.GetUserSessionsAsync(expectedUserId))
            .ReturnsAsync(Result<IReadOnlyList<SessionIndexDto>>.Success(sessions));

        // Act
        var response = await _client.GetAsync($"/api/sessions?userId={expectedUserId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<IReadOnlyList<SessionIndexDto>>();
        Assert.NotNull(content);
        Assert.Single(content);
    }

    [Fact]
    public async Task GetSessionById_SessionExists_Returns200Ok_WithJsonPayload()
    {
        // Arrange
        Guid expectedUserId = GetUserId();

        var targetSessionId = Guid.NewGuid();
        var mockSession = Result<SessionDto>.Success(new SessionDto(targetSessionId,
            DateTime.UtcNow, "Cycling", 3600, 25000, 140, 200, null));

        // Strict mock setup matches the targeted session ID and the authenticated user ID
        _queryMock
            .Setup(s => s.GetSessionByIdAsync(targetSessionId, expectedUserId))
            .ReturnsAsync(mockSession);

        // Act
        var response = await _client.GetAsync($"/api/sessions/{targetSessionId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<SessionIndexDto>();
        Assert.NotNull(content);
        Assert.Equal(targetSessionId, content.Id);
    }

    [Fact]
    public async Task DeleteSession_ValidUserAndSession_Returns202NoContent()
    {
        // Arrange
        Guid expectedUserId = GetUserId();

        var sessionId = Guid.NewGuid();

        _deleteMock
            .Setup(s => s.DeleteSessionAsync(sessionId, expectedUserId))
             .ReturnsAsync(Result.Success());

        // Act
        var response = await _client.DeleteAsync($"/api/sessions/{sessionId}?userId={expectedUserId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteSession_SessionNotFound_Returns404NotFound()
    {
        // Arrange
        Guid expectedUserId = GetUserId();

        var sessionId = Guid.NewGuid();

        _deleteMock
            .Setup(s => s.DeleteSessionAsync(sessionId, expectedUserId))
             .ReturnsAsync(Result.Failure(ServiceErrorType.ResourceNotFound, "_"));

        // Act
        var response = await _client.DeleteAsync($"/api/sessions/{sessionId}?userId={expectedUserId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteSession_Error_Returns500_WithErrorDetails()
    {
        // Arrange
        Guid expectedUserId = GetUserId();

        var sessionId = Guid.NewGuid();

        var expectedError = "error occured";

        _deleteMock
            .Setup(s => s.DeleteSessionAsync(sessionId, expectedUserId))
             .ReturnsAsync(Result.Failure(ServiceErrorType.Failure, expectedError));

        // Act
        var response = await _client.DeleteAsync($"/api/sessions/{sessionId}?userId={expectedUserId}");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(500, problem.Status);
        Assert.Equal(expectedError, problem.Detail);
    }

    private Guid GetUserId()
    {
        var optionsMonitor = _factory.Services
            .GetRequiredService<IOptionsMonitor<FakeAuthHandlerOptions>>();

        // 2. Extract the active options instance tied to your specific scheme name
        var currentOptions = optionsMonitor.Get(FakeAuthHandler.AuthenticationScheme);
        var expectedUserId = Guid.Parse(currentOptions.DefaultUserId);
        return expectedUserId;
    }
}
