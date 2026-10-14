using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CyclingTracker.Api.Extensions;
using CyclingTracker.Api.Services.FileImport;

namespace CyclingTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ImportController(IFitImportService fitImportsessionService) : ControllerBase
{
    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file is null || file.Length == 0) return BadRequest("No file provided");
        if (!file.FileName.EndsWith(".fit", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only .fit files are supported");

        var result = await fitImportsessionService.ImportAsync(file, User.GetUserId());

        if (!result.IsSuccess)
        {
            return result.ToActionResult(this);
        }

        return CreatedAtAction(
            nameof(SessionsController.GetSession),
            nameof(SessionsController).Replace("Controller", ""), 
            new { id = result.Value }, 
            result.Value);
    }
}
