using CyclingTracker.Api.Dtos;
using CyclingTracker.Api.Shared;

namespace CyclingTracker.Api.Services.AthleteZones;

public interface IAthleteZonesService
{
    Task<Result<AthleteZonesDto>> GetZonesAsync(Guid userId);
    Task<Result<AthleteZonesDto>> UpdateZonesAsync(Guid userId, UpdateAthleteZonesDto dto);
}