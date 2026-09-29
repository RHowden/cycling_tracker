using CyclingTracker.Api.Dtos;
using CyclingTracker.Api.Shared;

public class FitImportService : IFitImportService
{
    public async Task<Result<SessionDto>> ImportAsync(IFormFile fitFile, Guid userId)
    {
        return Result<SessionDto>.Success(new SessionDto(new Guid(), 10));
    }
}