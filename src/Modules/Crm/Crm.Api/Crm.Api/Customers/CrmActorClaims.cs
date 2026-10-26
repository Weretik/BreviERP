using System.Security.Claims;
using Crm.Application.Customers.Models;
namespace Crm.Api.Customers;

internal static class CrmActorClaims
{
    public static CrmActor? Read(ClaimsPrincipal user) => int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id > 0
        ? new CrmActor(id, user.IsInRole("Admin")) : null;
}
