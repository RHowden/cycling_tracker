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
        Samples = ToSamples(dto.Samples),
        CreatedAtUtc = DateTime.UtcNow
    };

    public static SportType ToSportType(string? sport) =>
        sport?.Trim().ToLowerInvariant() switch
        {
            "running" => SportType.Running,
            "cycling" => SportType.Cycling,
            "walking" => SportType.Walking,
            "training" => SportType.Strength,
            _ => SportType.Other
        };

    private static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc) // Unspecified: assume UTC
    };

     private static SampleData? ToSamples(Samples? samples) =>
        samples is null
            ? null
            : new SampleData
            {
                Time = samples.Time,
                HeartRate = samples.HeartRate,
                Power = samples.Power,
                Speed = samples.Speed
            };
}