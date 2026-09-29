namespace FitFileParsing;

public record ActivityData(DateTime StartTime,
    string Type,
    double DurationMin,
    int? AvgHr,
    int? AvgPower,
    float Distance);