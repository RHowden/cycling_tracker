using CyclingTracker.Api.Dtos;
using CyclingTracker.Api.Models;

namespace CyclingTracker.Api.Mappers;

public class AthleteMapper
{
    public AthleteZonesDto ToZonesDto(Athlete athlete) => new(
        ToDto(athlete.HeartRateZones),
        ToDto(athlete.PowerZones));

    public TrainingZones ToEntity(UpdateZoneRangeDto request) =>
        new(request.Zone1Max, request.Zone2Max);

    private static ZoneRangeDto ToDto(TrainingZones zones) =>
        new(zones.Zone1Max, zones.Zone2Max);
}