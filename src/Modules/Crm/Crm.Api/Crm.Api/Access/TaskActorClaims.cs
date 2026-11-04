using Crm.Application.Tasks.Access;
using System.Security.Claims;

namespace Crm.Api.Access;

internal static class TaskActorClaims
{
    /// <summary>Director also holds Admin, so «manages tasks» is the Admin role (TSK-001 R-01).</summary>
    public static TaskActor? Read(ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id > 0
            ? new TaskActor(id, user.IsInRole("Admin"))
            : null;
}
