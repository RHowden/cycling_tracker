using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using CyclingTracker.Api.Dtos;
using CyclingTracker.Api.IntegrationTests.Infrastructure;
using Microsoft.Extensions.Options;
using CyclingTracker.Api.Services.AthleteZones;
using CyclingTracker.Api.Shared;

namespace CyclingTracker.Api.IntegrationTests.Features;

public class AthleteZonesEndpointTests : IClassFixture<IntegrationFactory>, IDisposable
{
    private readonly HttpClient _client;
    private readonly Mock<IAthleteZonesService> _zoneMock = new();
    private readonly WebApplicationFactory<Program> _factory;

    public AthleteZonesEndpointTests(IntegrationFactory factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IAthleteZonesService>();
                services.AddScoped(_ => _zoneMock.Object);
            }));

        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _factory.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetZones_AthleteExists_Returns200Ok_WithJsonPayload()
    {
        // Arrange
        Guid expectedUserId = GetUserId();
        var mockZones = Result<AthleteZonesDto>.Success(new AthleteZonesDto(
            new ZoneRangeDto(130, 150),
            new ZoneRangeDto(150, 200)));

        _zoneMock
            .Setup(s => s.GetZonesAsync(expectedUserId))
            .ReturnsAsync(mockZones);

        // Act
        var response = await _client.GetAsync("/api/athlete/zones");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<AthleteZonesDto>();
        Assert.NotNull(content);
        Assert.Equal(mockZones.Value, content);
    }

    [Fact]
    public async Task GetZones_AthleteNotFound_Returns404NotFound()
    {
        // Arrange
        Guid expectedUserId = GetUserId();

        var value = Result<AthleteZonesDto>.Failure(Enums.ServiceErrorType.ResourceNotFound, "_");

        _zoneMock
            .Setup(s => s.GetZonesAsync(expectedUserId))
            .ReturnsAsync(value);

        // Act
        var response = await _client.GetAsync("/api/athlete/zones");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateZones_ValidRequest_Returns200Ok_WithSavedZones()
    {
        // Arrange
        Guid expectedUserId = GetUserId();
        var request = new UpdateAthleteZonesDto(
            new UpdateZoneRangeDto(125, 155),
            new UpdateZoneRangeDto(190, 260));
        var saved = Result<AthleteZonesDto>.Success(new AthleteZonesDto(
            new ZoneRangeDto(125, 155),
            new ZoneRangeDto(190, 260)));

        // Records compare by value, so the deserialised request matches this setup
        _zoneMock
            .Setup(s => s.UpdateZonesAsync(expectedUserId, request))
            .ReturnsAsync(saved);

        // Act
        var response = await _client.PutAsJsonAsync("/api/athlete/zones", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<AthleteZonesDto>();
        Assert.NotNull(content);
        Assert.Equal(saved.Value, content);
    }

    [Fact]
    public async Task UpdateZones_AthleteNotFound_Returns404NotFound()
    {
        // Arrange
        Guid expectedUserId = GetUserId();
        var request = new UpdateAthleteZonesDto(
            new UpdateZoneRangeDto(125, 155),
            new UpdateZoneRangeDto(190, 260));

        var value = Result<AthleteZonesDto>.Failure(Enums.ServiceErrorType.ResourceNotFound, "_");

        _zoneMock
            .Setup(s => s.UpdateZonesAsync(expectedUserId, request))
            .ReturnsAsync(value);

        // Act
        var response = await _client.PutAsJsonAsync("/api/athlete/zones", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData(150, 150)] // equal
    [InlineData(150, 140)] // inverted
    public async Task UpdateZones_Zone2NotAboveZone1_Returns400_AndDoesNotCallService(int zone1Max, int zone2Max)
    {
        // Arrange
        var request = new UpdateAthleteZonesDto(
            new UpdateZoneRangeDto(zone1Max, zone2Max),
            new UpdateZoneRangeDto(190, 260));

        // Act
        var response = await _client.PutAsJsonAsync("/api/athlete/zones", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        _zoneMock.Verify(
            s => s.UpdateZonesAsync(It.IsAny<Guid>(), It.IsAny<UpdateAthleteZonesDto>()),
            Times.Never);
    }

    [Theory]
    [InlineData(0, 150)]    // below range
    [InlineData(130, 2001)] // above range
    public async Task UpdateZones_ValueOutOfRange_Returns400_AndDoesNotCallService(int zone1Max, int zone2Max)
    {
        // Arrange
        var request = new UpdateAthleteZonesDto(
            new UpdateZoneRangeDto(zone1Max, zone2Max),
            new UpdateZoneRangeDto(190, 260));

        // Act
        var response = await _client.PutAsJsonAsync("/api/athlete/zones", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        _zoneMock.Verify(
            s => s.UpdateZonesAsync(It.IsAny<Guid>(), It.IsAny<UpdateAthleteZonesDto>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateZones_MissingPowerZones_Returns400_AndDoesNotCallService()
    {
        // Arrange: body with no "power" property
        var body = new { heartRate = new { zone1Max = 130, zone2Max = 150 } };

        // Act
        var response = await _client.PutAsJsonAsync("/api/athlete/zones", body);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        _zoneMock.Verify(
            s => s.UpdateZonesAsync(It.IsAny<Guid>(), It.IsAny<UpdateAthleteZonesDto>()),
            Times.Never);
    }

    private Guid GetUserId()
    {
        var optionsMonitor = _factory.Services
            .GetRequiredService<IOptionsMonitor<FakeAuthHandlerOptions>>();

        var currentOptions = optionsMonitor.Get(FakeAuthHandler.AuthenticationScheme);
        return Guid.Parse(currentOptions.DefaultUserId);
    }
}