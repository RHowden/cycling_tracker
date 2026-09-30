using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CyclingTracker.Api.Data;
using CyclingTracker.Api.Extensions;
using CyclingTracker.Api.Services;

namespace CyclingTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionsController(ISessionQueryService sessionQueryService) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetSessions([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {     
        if (!Guid.TryParse(User.GetUserId(), out var userId))
        {
            return BadRequest("Invalid or missing user identifier.");
        }

        var sessions = await sessionQueryService.GetUserSessionsAsync(userId);

        return Ok(sessions);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetSession(Guid id)
    {
        if (!Guid.TryParse(User.GetUserId(), out var userId))
        {
            return BadRequest("Invalid or missing user identifier.");
        }

        var session = await sessionQueryService.GetSessionByIdAsync(id, userId);

        if (session == null)
        {
            return NotFound();
        }

        return Ok(session);
    }
}
