using Microsoft.AspNetCore.Mvc;

namespace CyclingTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetSessions([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var rnd = new Random();
            var currentDate =  DateTime.Parse("026-09-15");

            var sessions = Enumerable.Range(1, 10)            
                .Select(i =>
                {
                    currentDate = currentDate.AddDays(rnd.Next(1, 5)); 
                
                    return new { 

                        Id = i, // Uses the loop index (1, 2, 3...)
                        Date = currentDate.ToString("yyyy-MM-dd"), // Increments the date
                        Type = "Run", 
                        DistanceKm = rnd.Next(50), 
                        DurationMin = rnd.Next(180), 
                        AvgHr = 140 + rnd.Next(1, 30),
                        AvgPower = 150 + rnd.Next(1, 50),
                        Distance = rnd.Next(1, 50)
                    };                    
                }
            ).ToArray();

            return Ok(sessions);
        }

        [HttpGet("{id}")]
        public IActionResult GetSession(int id)
        {
            var session = new { Id = id, Date = "2026-09-15", Type = "Run", DistanceKm = 8.2, DurationMin = 42, AvgHr = 152 };
            return Ok(session);
        }
    }
}