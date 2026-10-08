using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CyclingTracker.Api.Extensions;
using CyclingTracker.Api.Services;

namespace CyclingTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImportController(IFitImportService fitImportsessionService) : ControllerBase
{
    [HttpPost("import")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file is null || file.Length == 0) return BadRequest("No file provided");
        if (!file.FileName.EndsWith(".fit", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only .fit files are supported");

        string currentUserId = User.GetUserId();
        if (!Guid.TryParse(currentUserId, out Guid userId))
        {
            // Returns a clean 401 if the string is empty or isn't a valid Guid format
            return Unauthorized("User ID is missing or invalid in session cookie.");
        }

        var result = await fitImportsessionService.ImportAsync(file, userId);

        if (!result.IsSuccess)
        {
            return UnprocessableEntity(new { error = result.ErrorMessage });
        }

        return CreatedAtAction(
            nameof(SessionsController.GetSession),
            nameof(SessionsController).Replace("Controller", ""), 
            new { id = result.Value!.Id }, 
            result.Value);
    }
}
