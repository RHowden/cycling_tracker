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


    [HttpPost("import")]             // POST /api/sessions/import (multipart, FIT file)
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file is null || file.Length == 0) return BadRequest("No file provided");
        if (!file.FileName.EndsWith(".fit", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only .fit files are supported");

        await using var stream = file.OpenReadStream();
        //var session = await _fitIngestService.IngestAsync(stream, User);
        var session = SampleData.Sessions[0];
        return Ok(session);
    }
}
