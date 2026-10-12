namespace CyclingTracker.Api.Dtos;

public record SessionDto(
    Guid Id,
    DateTime StartTimeUtc,
    string Sport,
    double DurationSeconds,
    double DistanceMetres,
    int? AvgHeartRate,
    int? AvgPowerWatts,
    SessionSamplesDto? Samples
);

public record SessionSamplesDto(
    int[] Time,
    int?[] HeartRate,
    int?[] Power,
    double?[] Speed
);