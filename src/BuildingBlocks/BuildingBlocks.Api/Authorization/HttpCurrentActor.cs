using System.Globalization;
using System.Security.Claims;
using BuildingBlocks.Application.Actors;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Api.Authorization;

/// <summary>The signed-in user of the current request, read from the cookie session's claims.</summary>
internal sealed class HttpCurrentActor(IHttpContextAccessor accessor) : ICurrentActor
{
    public Actor? Current
    {
        get
        {
            var user = accessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true) return null;
            if (!int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), NumberStyles.None,
                    CultureInfo.InvariantCulture, out var id)) return null;
            var roles = user.FindAll(ClaimTypes.Role).Select(claim => claim.Value).Distinct().ToArray();
            return new Actor(id, user.ActorName(), roles);
        }
    }
}
