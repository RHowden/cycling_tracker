namespace CyclingTracker.Api.Dtos;

public record SessionIndexDto(
    Guid Id,
    DateTime StartTimeUtc,
    string Sport,
    double DurationSeconds,
    double DistanceMetres,
    int? AvgHeartRate,
    int? AvgPowerWatts
);
