namespace FitFileParsing;

public record ActivityData(DateTime StartTime,
    string Type,
    double Duration,
    int? AvgHr,
    int? AvgPower,
    float Distance);