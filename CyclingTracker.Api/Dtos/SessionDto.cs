using CyclingTracker.Api.Models;

namespace CyclingTracker.Api.Dtos;

public record SessionDto(
    Guid Id,
    DateTime StartTimeUtc,
    string Sport,
    double DurationSeconds,
    double DistanceMetres,
    int? AvgHeartRate,
    int? AvgPowerWatts,
    SessionSamplesDto? Samples)
{
    public static SessionDto From(Session session) => new(
        session.Id,
        session.StartTimeUtc,
        session.Sport.ToString(),
        session.Duration.TotalSeconds,
        session.DistanceMetres,
        session.AvgHeartRate,
        session.AvgPowerWatts,
        session.Samples is null
            ? null
            : new SessionSamplesDto(
                session.Samples.Time,
                session.Samples.HeartRate,
                session.Samples.Power,
                session.Samples.Speed));
};

public record SessionSamplesDto(
    int[] Time,
    int?[] HeartRate,
    int?[] Power,
    double?[] Speed
);