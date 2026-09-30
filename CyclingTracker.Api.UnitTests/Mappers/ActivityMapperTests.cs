using CyclingTracker.Api.Mappers;
using CyclingTracker.Api.Models;
using FitFileParsing;

namespace CyclingTracker.Api.UnitTests.services;

public class ActivityMapperTests
{
    private readonly ActivityMapper _mapper = new();
    private readonly Guid _userId = Guid.NewGuid();

    private static ActivityData MakeDto(
        DateTime? start = null,
        string sport = "running",
        double durationSeconds = 2730,
        int? avgHr = 150,
        int? avgPower = null,
        float distance = 10000f) =>
        new(start ?? new DateTime(2026, 9, 1, 7, 0, 0, DateTimeKind.Utc),
            sport, durationSeconds, avgHr, avgPower, distance);

    [Fact]
    public void Maps_happy_path()
    {
        var result = _mapper.ToEntity(MakeDto(), _userId);

        Assert.Equal(_userId, result.UserId);
        Assert.Equal(TimeSpan.FromSeconds(2730), result.Duration);
        Assert.Equal(10000, result.DistanceMetres);
        Assert.Equal(SportType.Running, result.Sport);
        Assert.Equal(150, result.AvgHeartRate);
        Assert.Null(result.AvgPowerWatts);
    }

    [Theory]
    [InlineData(DateTimeKind.Utc)]
    [InlineData(DateTimeKind.Unspecified)]
    [InlineData(DateTimeKind.Local)]
    public void StartTime_is_always_utc(DateTimeKind kind)
    {
        var start = new DateTime(2026, 9, 1, 7, 0, 0, kind);

        var result = _mapper.ToEntity(MakeDto(start: start), _userId);

        Assert.Equal(DateTimeKind.Utc, result.StartTimeUtc.Kind);
    }

    [Theory]
    [InlineData("Running", SportType.Running)]
    [InlineData("cycling", SportType.Cycling)]
    [InlineData("underwater_basket_weaving", SportType.Other)]
    [InlineData("", SportType.Other)]
    [InlineData(null, SportType.Other)]
    public void Sport_is_normalised(string? input, SportType expected)
    {
        var result = _mapper.ToEntity(MakeDto(sport: input!), _userId);

        Assert.Equal(expected, result.Sport);
    }
}