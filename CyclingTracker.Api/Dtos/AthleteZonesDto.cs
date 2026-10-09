namespace CyclingTracker.Api.Dtos;

public record ZoneRangeDto(int Zone1Max, int Zone2Max);

public record AthleteZonesDto(ZoneRangeDto HeartRate, ZoneRangeDto Power);