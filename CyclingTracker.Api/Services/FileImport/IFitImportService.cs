using CyclingTracker.Api.Dtos;
using CyclingTracker.Api.Shared;

public interface IFitImportService
{
    Task<Result<SessionDto>> ImportAsync(IFormFile fitFile, Guid userId);
}