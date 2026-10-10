namespace FitFileParsing;

public record ActivityData(DateTime StartTime,
    string Type,
    double Duration,
    int? AvgHr,
    int? AvgPower,
    float Distance,
    SampleData? Samples = null);

public record SampleData(
    int[] Time,
    int?[] HeartRate,
    int?[] Power,
    double?[] Speed);