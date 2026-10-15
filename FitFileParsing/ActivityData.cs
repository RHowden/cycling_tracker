namespace FitFileParsing;

public record ActivityData(DateTime StartTime,
    string Type,
    double Duration,
    int? AvgHr,
    int? AvgPower,
    float Distance,
    Samples? Samples = null);

public record Samples(
    int[] Time,
    int?[] HeartRate,
    int?[] Power,
    double?[] Speed);