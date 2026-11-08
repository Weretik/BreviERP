using Crm.Application.Leads.Access;
using System.Security.Claims;

namespace Crm.Api.Access;

internal static class LeadActorClaims
{
    /// <summary>Director also holds Admin, so «may give back anybody's lead» is the Admin role (LEAD-001 R-04).</summary>
    public static LeadActor? Read(ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id > 0
            ? new LeadActor(id, user.IsInRole("Admin"))
            : null;
}
