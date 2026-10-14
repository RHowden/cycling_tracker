using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CyclingTracker.Api.Dtos;
using CyclingTracker.Api.Extensions;
using CyclingTracker.Api.Services.AthleteZones;

namespace CyclingTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/athlete/zones")]
public class AthleteZonesController(IAthleteZonesService athleteZonesService) : ControllerBase
{
    [HttpGet]   
    public async Task<IActionResult> GetZones()
    {
        if (!Guid.TryParse(User.GetUserId(), out var userId))
        {
            return BadRequest("Invalid or missing user identifier.");
        }

        var result = await athleteZonesService.GetZonesAsync(userId);

        return result.ToActionResult(this);
    }

    [HttpPut]
      public async Task<IActionResult> UpdateZones([FromBody] UpdateAthleteZonesDto dto)
    {
        if (!Guid.TryParse(User.GetUserId(), out var userId))
        {
            return BadRequest("Invalid or missing user identifier.");
        }

         var result =  await athleteZonesService.UpdateZonesAsync(userId, dto);

        return result.ToActionResult(this);
    }
}