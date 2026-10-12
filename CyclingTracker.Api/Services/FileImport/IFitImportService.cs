using CyclingTracker.Api.Dtos;
using CyclingTracker.Api.Shared;

namespace CyclingTracker.Api.Services.FileImport;

public interface  IFitImportService
{
    Task<Result<Guid>> ImportAsync(IFormFile fitFile, Guid userId);
}