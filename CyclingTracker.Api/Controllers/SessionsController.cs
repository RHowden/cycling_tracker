using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CyclingTracker.Api.Extensions;
using CyclingTracker.Api.Enums;
using CyclingTracker.Api.Services.SessionQuery;
using CyclingTracker.Api.Services.SessionDeletion;

namespace CyclingTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionsController(ISessionQueryService sessionQueryService,
    ISessionDeletionService sessionDeletionService) : ControllerBase
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

    [HttpDelete("{sessionId}")]
    [Authorize]
    public async Task<IActionResult> DeleteSession(Guid sessionId)
    {

        string currentUserId = User.GetUserId();
        if (!Guid.TryParse(currentUserId, out Guid userId))
        {
            // Returns a clean 401 if the string is empty or isn't a valid Guid format
            return Unauthorized("User ID is missing or invalid in session cookie.");
        }

        var result = await sessionDeletionService.DeleteSessionAsync(sessionId, userId);

        if (!result.IsSuccess)
        {
            return result.ToActionResult(this);
        }
        return NoContent();
    }
}
