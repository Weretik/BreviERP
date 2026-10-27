using Crm.Application.Customers.Access;
using System.Security.Claims;
namespace Crm.Api.Access;

internal static class CrmActorClaims
{
    public static CrmActor? Read(ClaimsPrincipal user) => int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id > 0
        ? new CrmActor(id, user.IsInRole("Admin")) : null;
}
