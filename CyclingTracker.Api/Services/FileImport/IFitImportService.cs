using CyclingTracker.Api.Dtos;
using CyclingTracker.Api.Shared;

namespace CyclingTracker.Api.Services;

public interface  IFitImportService
{
    Task<Result<SessionDto>> ImportAsync(IFormFile fitFile, Guid userId);
}