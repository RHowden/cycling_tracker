using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CyclingTracker.Api.Data;
using CyclingTracker.Api.Extensions;

namespace CyclingTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionsController : ControllerBase
{
    [HttpGet]
    [Authorize]
    public IActionResult GetSessions([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        string currentUserId = User.GetUserId();
        var sessions = SampleData.Sessions.Where(s => s.UserId == currentUserId);

        return Ok(sessions);
    }

    [HttpGet("{id}")]
    [Authorize]
    public IActionResult GetSession(int id)
    {
        string currentUserId = User.GetUserId();
        
        var session = SampleData.Sessions
            .Where(s => s.UserId == currentUserId)
            .FirstOrDefault(s => s.Id == id);
        return Ok(session);
    }
}
