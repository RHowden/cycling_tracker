namespace CyclingTracker.Api.Models;

public class Session
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime StartTimeUtc { get; set; }
    public SportType Sport { get; set; }
    public TimeSpan Duration { get; set; }
    public double DistanceMetres { get; set; }
    public int? AvgHeartRate { get; set; }
    public int? AvgPowerWatts { get; set; }
    public DateTime CreatedAtUtc { get; set; }

     public SampleData? Samples { get; set; }
}