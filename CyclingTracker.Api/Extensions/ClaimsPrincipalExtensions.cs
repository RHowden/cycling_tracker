using System.Security.Claims;

namespace CyclingTracker.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(value, out var id) && id != Guid.Empty
            ? id
            : throw new UnauthorizedAccessException("User ID claim is missing or invalid.");
    }
}



