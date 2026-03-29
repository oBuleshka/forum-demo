using System.Security.Claims;
using ForumBL.Core.Exceptions;

namespace ForumAPI.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var rawUserId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(rawUserId, out var userId))
        {
            throw new AppException("Authenticated user id is missing.", StatusCodes.Status401Unauthorized);
        }

        return userId;
    }
}
