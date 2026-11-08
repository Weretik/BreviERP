using Crm.Application.Quotes.Access;
using System.Security.Claims;

namespace Crm.Api.Access;

internal static class QuoteActorClaims
{
    /// <summary>
    /// Director also holds Admin, so «sees all» is the Admin role (R-02). <paramref name="canDelete"/> is true only on
    /// endpoints guarded by <see cref="CrmAccess.QuotesDelete"/>: «Права доступу» decides who deletes КП (R-33).
    /// </summary>
    public static QuoteActor? Read(ClaimsPrincipal user, bool canDelete = false) =>
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id > 0
            ? new QuoteActor(id, user.IsInRole("Admin"), canDelete)
            : null;
}
