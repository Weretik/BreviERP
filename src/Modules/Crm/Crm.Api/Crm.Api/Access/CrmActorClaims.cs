using System.Security.Claims;
using Crm.Application.Abstractions.Customers;
namespace Crm.Api.Access;

internal static class CrmActorClaims
{
    public static CrmActor? Read(ClaimsPrincipal user) => int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id > 0
        ? new CrmActor(id, user.IsInRole("Admin")) : null;
}
