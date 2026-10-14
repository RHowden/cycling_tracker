using System.ComponentModel.DataAnnotations;

namespace CyclingTracker.Api.Dtos;

public record UpdateZoneRangeDto(
    [Range(1, 2000)] int Zone1Max,
    [Range(1, 2000)] int Zone2Max) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Zone2Max <= Zone1Max)
            yield return new ValidationResult(
                "Zone 2 max must be greater than Zone 1 max.",
                [nameof(Zone2Max)]);
    }
}

public record UpdateAthleteZonesDto(
    [Required] UpdateZoneRangeDto HeartRate,
    [Required] UpdateZoneRangeDto Power);