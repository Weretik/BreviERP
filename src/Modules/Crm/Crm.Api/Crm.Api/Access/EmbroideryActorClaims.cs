using System.Security.Claims;
using Crm.Application.Features.Embroidery.Shared.Access;
namespace Crm.Api.Access;

internal static class EmbroideryActorClaims
{
    /// <summary>
    /// «Sees the cost» (EMB-001 R-02) is «Ціна вишивки: собівартість і налаштування: Перегляд» on «Права доступу»
    /// (ACC-002 R-02); the director always has it.
    /// </summary>
    public static async Task<EmbroideryActor?> ReadAsync(ClaimsPrincipal user, IAuthorizationService authorization) =>
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id > 0
            ? new EmbroideryActor(id, (await authorization.AuthorizeAsync(user, CrmAccess.EmbroideryCostView)).Succeeded)
            : null;
}
