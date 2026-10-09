using FitFileParsing;

namespace CyclingTracker.Api.Services.FileImport;

public interface IFitFileParser
{
    ActivityData Parse(Stream stream);
}