using CyclingTracker.Api.Dtos;
using CyclingTracker.Api.Models;
using CyclingTracker.Api.Services.AthleteZones;
using Microsoft.EntityFrameworkCore;

namespace CyclingTracker.Api.IntegrationTest.Services;

[Collection("Postgres")]
public class AthleteZonesServiceTests(PostgresFixture pg) : DbTestBase(pg)
{
    [Fact]
    public async Task GetZones_AthleteExists_ReturnsTheirHrAndPowerZones()
    {
        // Arrange
        var userId = await SeedAthleteAsync(
            hr: new TrainingZones(120, 145),
            power: new TrainingZones(180, 240));

        var sut = new AthleteZonesService(pg.CreateDb());

        // Act
        var result = await sut.GetZonesAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new ZoneRangeDto(120, 145), result.Value!.HeartRate);
        Assert.Equal(new ZoneRangeDto(180, 240), result.Value!.Power);
    }

    [Fact]
    public async Task GetZones_NewAthleteWithDefaults_ReturnsDefaultZones()
    {
        // Arrange
        var userId = await SeedAthleteAsync();
        var sut = new AthleteZonesService(pg.CreateDb());

        // Act
        var result = await sut.GetZonesAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new ZoneRangeDto(130, 150), result.Value!.HeartRate);
        Assert.Equal(new ZoneRangeDto(150, 200), result.Value!.Power);
    }

    [Fact]
    public async Task GetZones_AthleteDoesNotExist_ReturnsNull()
    {
        // Arrange
        var sut = new AthleteZonesService(pg.CreateDb());

        // Act
        var result = await sut.GetZonesAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetZones_MultipleAthletes_ReturnsOnlyRequestedAthletesZones()
    {
        // Arrange
        var targetUser = await SeedAthleteAsync(hr: new TrainingZones(110, 140));
        await SeedAthleteAsync(hr: new TrainingZones(160, 175));

        var sut = new AthleteZonesService(pg.CreateDb());

        // Act
        var result = await sut.GetZonesAsync(targetUser);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new ZoneRangeDto(110, 140), result.Value!.HeartRate);
    }

    [Fact]
    public async Task UpdateZones_AthleteExists_PersistsNewZonesAndReturnsThem()
    {
        // Arrange
        var userId = await SeedAthleteAsync();
        var sut = new AthleteZonesService(pg.CreateDb());
        var request = new UpdateAthleteZonesDto(
            new UpdateZoneRangeDto(125, 155),
            new UpdateZoneRangeDto(190, 260));

        // Act
        var result = await sut.UpdateZonesAsync(userId, request);

        // Assert: returned DTO
        Assert.NotNull(result);
        Assert.Equal(new ZoneRangeDto(125, 155), result.Value!.HeartRate);
        Assert.Equal(new ZoneRangeDto(190, 260), result.Value!.Power);

        // Assert: actually persisted (read back through a fresh context)
        var persisted = await LoadAthleteAsync(userId);
        Assert.Equal(125, persisted.HeartRateZones.Zone1Max);
        Assert.Equal(155, persisted.HeartRateZones.Zone2Max);
        Assert.Equal(190, persisted.PowerZones.Zone1Max);
        Assert.Equal(260, persisted.PowerZones.Zone2Max);
    }

    [Fact]
    public async Task UpdateZones_AthleteDoesNotExist_ReturnsNull()
    {
        // Arrange
        var sut = new AthleteZonesService(pg.CreateDb());
        var request = new UpdateAthleteZonesDto(
            new UpdateZoneRangeDto(125, 155),
            new UpdateZoneRangeDto(190, 260));

        // Act
        var result = await sut.UpdateZonesAsync(Guid.NewGuid(), request);

        // Assert
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task UpdateZones_MultipleAthletes_DoesNotChangeOtherAthletesZones()
    {
        // Arrange
        var targetUser = await SeedAthleteAsync();
        var otherUser = await SeedAthleteAsync(
            hr: new TrainingZones(115, 135),
            power: new TrainingZones(170, 220));

        var sut = new AthleteZonesService(pg.CreateDb());
        var request = new UpdateAthleteZonesDto(
            new UpdateZoneRangeDto(125, 155),
            new UpdateZoneRangeDto(190, 260));

        // Act
        await sut.UpdateZonesAsync(targetUser, request);

        // Assert
        var other = await LoadAthleteAsync(otherUser);
        Assert.Equal(115, other.HeartRateZones.Zone1Max);
        Assert.Equal(135, other.HeartRateZones.Zone2Max);
        Assert.Equal(170, other.PowerZones.Zone1Max);
        Assert.Equal(220, other.PowerZones.Zone2Max);
    }

    [Theory]
    [InlineData(150, 150)] // equal
    [InlineData(150, 140)] // inverted
    public async Task UpdateZones_Zone2NotAboveZone1_ThrowsAndLeavesZonesUnchanged(int zone1Max, int zone2Max)
    {
        // Arrange
        var userId = await SeedAthleteAsync(hr: new TrainingZones(120, 145));
        var sut = new AthleteZonesService(pg.CreateDb());
        var request = new UpdateAthleteZonesDto(
            new UpdateZoneRangeDto(zone1Max, zone2Max),
            new UpdateZoneRangeDto(190, 260));

        // Act & Assert: the entity guard is the backstop behind DTO validation
        await Assert.ThrowsAsync<ArgumentException>(() => sut.UpdateZonesAsync(userId, request));

        var persisted = await LoadAthleteAsync(userId);
        Assert.Equal(120, persisted.HeartRateZones.Zone1Max);
        Assert.Equal(145, persisted.HeartRateZones.Zone2Max);
    }

    private async Task<Guid> SeedAthleteAsync(TrainingZones? hr = null, TrainingZones? power = null)
    {
        await using var db = pg.CreateDb();
        var athlete = new Athlete { Id = Guid.NewGuid() };
        if (hr is not null) athlete.HeartRateZones = hr;
        if (power is not null) athlete.PowerZones = power;
        db.Add(athlete);
        await db.SaveChangesAsync();
        return athlete.Id;
    }

    private async Task<Athlete> LoadAthleteAsync(Guid userId)
    {
        await using var db = pg.CreateDb();
        return await db.Set<Athlete>().AsNoTracking().SingleAsync(a => a.Id == userId);
    }
}