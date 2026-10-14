using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CyclingTracker.Api.Extensions;
using CyclingTracker.Api.Services.FileImport;
using CyclingTracker.Api.Shared;
using CyclingTracker.Api.Enums;

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
        if (file is null || file.Length == 0)
        {
            return Result.Failure(ServiceErrorType.ValidationError, "No file provided")
                .ToActionResult(this);
        }

        if (!file.FileName.EndsWith(".fit", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure(ServiceErrorType.ValidationError, "Only .fit files are supported")
                .ToActionResult(this);
        }

        var result = await fitImportsessionService.ImportAsync(file, User.GetUserId());

        return result.ToActionResult(this, id => CreatedAtAction(
            nameof(SessionsController.GetSession),
            "Sessions",
            new { id },
            id)
        );
    }
}
