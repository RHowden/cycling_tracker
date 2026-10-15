namespace FitFileParsing;

public interface IFitFileParser
{
    FitParseResult Parse(Stream stream);
}