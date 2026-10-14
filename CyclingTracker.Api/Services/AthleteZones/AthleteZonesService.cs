using CyclingTracker.Api.Data;
using CyclingTracker.Api.Dtos;
using CyclingTracker.Api.Models;
using CyclingTracker.Api.Shared;
using Microsoft.EntityFrameworkCore;

namespace CyclingTracker.Api.Services.AthleteZones;

public class AthleteZonesService(AppDbContext db) : IAthleteZonesService
{
    public async Task<Result<AthleteZonesDto>> GetZonesAsync(Guid userId)
    {
        var data = await db.Set<Athlete>()
            .AsNoTracking()
            .Where(a => a.Id == userId)
            .Select(a => new AthleteZonesDto(
                new ZoneRangeDto(a.HeartRateZones.Zone1Max, a.HeartRateZones.Zone2Max),
                new ZoneRangeDto(a.PowerZones.Zone1Max, a.PowerZones.Zone2Max)))
            .SingleOrDefaultAsync();
            return (data is not null) ? 
                Result<AthleteZonesDto>.Success(data) 
                : Result<AthleteZonesDto>
                    .Failure(Enums.ServiceErrorType.ResourceNotFound, "Unable to retrieve zones");
    }

    public async Task<Result<AthleteZonesDto>> UpdateZonesAsync(Guid userId, UpdateAthleteZonesDto dto)
    {
        var athlete = await db.Set<Athlete>().SingleOrDefaultAsync(a => a.Id == userId);
        if (athlete is null)
        {
            return Result<AthleteZonesDto>
                .Failure(Enums.ServiceErrorType.ResourceNotFound, "Athlete not found");
        }

        athlete.HeartRateZones = ToEntity(dto.HeartRate);
        athlete.PowerZones = ToEntity(dto.Power);

        await db.SaveChangesAsync();

        var data = new AthleteZonesDto(
            ToDto(athlete.HeartRateZones),
            ToDto(athlete.PowerZones));

        return Result<AthleteZonesDto>.Success(data);
    }

    private static TrainingZones ToEntity(UpdateZoneRangeDto request) =>
        new(request.Zone1Max, request.Zone2Max);

    private static ZoneRangeDto ToDto(TrainingZones zones) =>
        new(zones.Zone1Max, zones.Zone2Max);
}