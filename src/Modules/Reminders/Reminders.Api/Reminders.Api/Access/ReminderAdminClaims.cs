using System.Security.Claims;
using Reminders.Application.Access;

namespace Reminders.Api.Access;

internal static class ReminderAdminClaims
{
    public static ReminderAdmin Read(ClaimsPrincipal user) => new(
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id > 0 ? id : null,
        user.FindFirstValue(ClaimTypes.Name) ?? user.FindFirstValue(ClaimTypes.Email));
}
