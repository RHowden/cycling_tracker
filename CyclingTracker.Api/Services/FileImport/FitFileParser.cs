using FitFileParsing;

namespace CyclingTracker.Api.Services.FileImport;

public class FitFileParser : IFitFileParser
{
    public ActivityData Parse(Stream fitStream) =>
        new FitFileParsing.FitFileParser().Parse(fitStream);
}
