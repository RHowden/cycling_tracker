using Microsoft.AspNetCore.Identity;

namespace CyclingTracker.Api.Models;

public class Athlete : IdentityUser<Guid>
{
    public TrainingZones HeartRateZones { get; set; } = new(130, 150);
    public TrainingZones PowerZones { get; set; } = new(150, 200);
}
