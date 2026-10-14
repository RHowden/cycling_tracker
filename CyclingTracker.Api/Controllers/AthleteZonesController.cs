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
        var result = await athleteZonesService.GetZonesAsync(User.GetUserId());

        return result.ToActionResult(this);
    }

    [HttpPut]
      public async Task<IActionResult> UpdateZones([FromBody] UpdateAthleteZonesDto dto)
    {
         var result =  await athleteZonesService.UpdateZonesAsync(User.GetUserId(), dto);

        return result.ToActionResult(this);
    }
}