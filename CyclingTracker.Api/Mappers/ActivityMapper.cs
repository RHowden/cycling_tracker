using CyclingTracker.Api.Models;
using FitFileParsing;

namespace CyclingTracker.Api.Mappers;

public class ActivityMapper
{
    public Session ToEntity(ActivityData dto, Guid userId) => new()
    {
        UserId = userId,
        StartTimeUtc = ToUtc(dto.StartTime),
        Sport = ToSportType(dto.Type),
        Duration = TimeSpan.FromSeconds(dto.Duration),
        DistanceMetres = dto.Distance,
        AvgHeartRate = dto.AvgHr,
        AvgPowerWatts = dto.AvgPower,
        CreatedAtUtc = DateTime.UtcNow
    };

    private static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc) // Unspecified: assume UTC
    };

    private static SportType ToSportType(string? sport) =>
        sport?.Trim().ToLowerInvariant() switch
        {
            "running" => SportType.Running,
            "cycling" => SportType.Cycling,
            "walking" => SportType.Walking,
            "training" => SportType.Strength,
            _ => SportType.Other
        };
}