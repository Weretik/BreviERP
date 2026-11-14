using System.Security.Claims;
using Crm.Application.Features.Embroidery.Shared.Access;
namespace Crm.Api.Access;

internal static class EmbroideryActorClaims
{
    public static EmbroideryActor? Read(ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id > 0
            ? new EmbroideryActor(id, user.IsInRole("Admin"))
            : null;
}
