using System.ComponentModel.DataAnnotations;
using CyclingTracker.Api.Models;

namespace CyclingTracker.Api.Dtos;

public record CreateAdHocSessionRequest(
    [EnumDataType(typeof(SportType))] SportType Sport,
    DateTimeOffset StartTime,
    [Range(1, 86_400)] int DurationSeconds,
    [Range(30, 250)] int? AvgHeartRate,
    [Range(0, 2_500)] int? AvgPowerWatts,
    [Range(0, 1_000_000)] double? DistanceMetres);