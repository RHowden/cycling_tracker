namespace CyclingTracker.Api.Data;

public class SampleData
{
    public static readonly dynamic[] Sessions = BuildSessions();

    private static dynamic[] BuildSessions()
    {
        var rnd = new Random();
        var currentDate = DateTime.Today;
        var sessions = new List<dynamic>();

        for (int i = 1; i <= 10; i++)
        {
            currentDate = currentDate.AddDays(rnd.Next(1, 5)); 
            sessions.Add(new
                {
                    Id = i,
                    Date = currentDate.ToString("yyyy-MM-dd"), // Increments the date
                    Type = "Run",
                    DistanceKm = rnd.Next(50),
                    DurationMin = rnd.Next(180),
                    AvgHr = 140 + rnd.Next(1, 30),
                    AvgPower = 150 + rnd.Next(1, 50),
                    Distance = rnd.Next(1, 50)
                }
            );
        }
        return sessions.ToArray();
    }
}