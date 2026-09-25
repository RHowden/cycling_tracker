using CyclingTracker.Api.Data;
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
            return Ok(SampleData.Sessions);
        }

        [HttpGet("{id}")]
        public IActionResult GetSession(int id)
        {
            var session =  SampleData.Sessions.FirstOrDefault(s => s.Id == id);
            return Ok(session);
        }
    }
}