using FitFileParsing;

public class FitFileParser : IFitFileParser
{
    public ActivityData Parse(Stream fitStream) =>
        new FitFileParsing.FitFileParser().Parse(fitStream);
}
