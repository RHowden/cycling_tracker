using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using CyclingTracker.Api.Models;


namespace CyclingTracker.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<Athlete> _signInManager;

        // Dependency injection gives us access to Identity's session handlers
        public AuthController(SignInManager<Athlete> signInManager)
        {
            _signInManager = signInManager;
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // Invalidates the current user cookie session on the server
            await _signInManager.SignOutAsync();
            
            return Ok(); // Returns a clean 200 HTTP response
        }
    }
}
