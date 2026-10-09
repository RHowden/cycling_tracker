namespace CyclingTracker.Api.Models;

public class TrainingZones
{
    public int Zone1Max { get; private set; }
    public int Zone2Max { get; private set; }

    private TrainingZones() { } // for EF

    public TrainingZones(int zone1Max, int zone2Max)
    {
        if (zone1Max <= 0)
            throw new ArgumentOutOfRangeException(nameof(zone1Max), "Zone 1 max must be positive.");
        if (zone2Max <= zone1Max)
            throw new ArgumentException("Zone 2 max must be greater than Zone 1 max.", nameof(zone2Max));

        Zone1Max = zone1Max;
        Zone2Max = zone2Max;
    }

    /// <summary>Returns 1, 2 or 3 for the given HR or power value.</summary>
    public int GetZone(int value) =>
        value <= Zone1Max ? 1 :
        value <= Zone2Max ? 2 : 3;
}