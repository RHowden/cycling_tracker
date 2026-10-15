using System.Diagnostics.CodeAnalysis;

namespace FitFileParsing;

public sealed class FitParseResult
{
    public ActivityData? ActivityData { get; }
    public FitParseError? Error { get; }

    [MemberNotNullWhen(true, nameof(ActivityData))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool Succeeded => ActivityData is not null;

    private FitParseResult(ActivityData? activityData, FitParseError? error) =>
        (ActivityData, Error) = (activityData, error);

    public static FitParseResult Success(ActivityData fit) => new(fit, null);
    public static FitParseResult Failure(FitParseError error) => new(null, error);
}