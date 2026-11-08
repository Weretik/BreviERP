using Crm.Application.Quotes.Access;
using System.Security.Claims;

namespace Crm.Api.Access;

internal static class QuoteActorClaims
{
    /// <summary>
    /// Director also holds Admin, so «sees all» is the Admin role (R-02); deleting a КП is the Director's (R-33).
    /// </summary>
    public static QuoteActor? Read(ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id > 0
            ? new QuoteActor(id, user.IsInRole("Admin"), user.IsInRole("Director"))
            : null;
}
