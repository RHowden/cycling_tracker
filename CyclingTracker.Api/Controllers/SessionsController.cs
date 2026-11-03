using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CyclingTracker.Api.Extensions;
using CyclingTracker.Api.Services.SessionQuery;
using CyclingTracker.Api.Services.SessionDeletion;
using CyclingTracker.Api.Dtos;
using CyclingTracker.Api.Services.AdHocSession;
using System.Net.Http.Headers;

namespace CyclingTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SessionsController(ISessionQueryService sessionQueryService,
    ISessionDeletionService sessionDeletionService,
    IAdHocSessionService adHocSessionService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSessions([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var result = await sessionQueryService.GetUserSessionsAsync(User.GetUserId());

        return result.ToActionResult(this);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSession(Guid id)
    {
        var result = await sessionQueryService.GetSessionByIdAsync(id, User.GetUserId());
        return result.ToActionResult(this);
    }

    [HttpDelete("{sessionId}")]
    public async Task<IActionResult> DeleteSession(Guid sessionId)
    {
        var result = await sessionDeletionService.DeleteSessionAsync(sessionId, User.GetUserId());

        return result.ToActionResult(this);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAdHocSessionAssync(CreateAdHocSessionRequest sessionData)
    {
        var result = await adHocSessionService.CreateAsync(sessionData, User.GetUserId());

        return result.ToActionResult(this);
    }
}
