using CyclingTracker.Api.Data;
using CyclingTracker.Api.Dtos;
using CyclingTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CyclingTracker.Api.Services.AthleteZones;

public class AthleteZonesService(AppDbContext db) : IAthleteZonesService
{
    public async Task<AthleteZonesDto?> GetZonesAsync(Guid userId)
    {
        return await db.Set<Athlete>()
            .AsNoTracking()
            .Where(a => a.Id == userId)
            .Select(a => new AthleteZonesDto(
                new ZoneRangeDto(a.HeartRateZones.Zone1Max, a.HeartRateZones.Zone2Max),
                new ZoneRangeDto(a.PowerZones.Zone1Max, a.PowerZones.Zone2Max)))
            .SingleOrDefaultAsync();
    }

    public async Task<AthleteZonesDto?> UpdateZonesAsync(Guid userId, UpdateAthleteZonesDto dto)
    {
        var athlete = await db.Set<Athlete>().SingleOrDefaultAsync(a => a.Id == userId);
        if (athlete is null) return null;

        athlete.HeartRateZones = ToEntity(dto.HeartRate);
        athlete.PowerZones = ToEntity(dto.Power);

        await db.SaveChangesAsync();

        return ToZonesDto(athlete);
    }

    private static AthleteZonesDto ToZonesDto(Athlete athlete) => new(
    ToDto(athlete.HeartRateZones),
    ToDto(athlete.PowerZones));

    private static TrainingZones ToEntity(UpdateZoneRangeDto request) =>
        new(request.Zone1Max, request.Zone2Max);

    private static ZoneRangeDto ToDto(TrainingZones zones) =>
        new(zones.Zone1Max, zones.Zone2Max);
}