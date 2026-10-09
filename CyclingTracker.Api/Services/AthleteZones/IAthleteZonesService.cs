using CyclingTracker.Api.Dtos;

namespace CyclingTracker.Api.Services.AthleteZones;

public interface IAthleteZonesService
{
    Task<AthleteZonesDto?> GetZonesAsync(Guid userId);
    Task<AthleteZonesDto?> UpdateZonesAsync(Guid userId, UpdateAthleteZonesDto dto);
}