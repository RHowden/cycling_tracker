using FitFileParsing;

public interface IFitFileParser
{
    ActivityData Parse(Stream stream);
}