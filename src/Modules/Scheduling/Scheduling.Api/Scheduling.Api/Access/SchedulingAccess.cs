namespace Scheduling.Api.Access;

/// <summary>
/// «Адміністрування › Фонові задачі» on «Права доступу» (SCH-001 R-11). The action comes from the HTTP method:
/// GET = View sees the page, POST = Create is «Запустити зараз», PUT = Edit changes the schedule. Admin by default.
/// </summary>
internal static class SchedulingAccess
{
    public const string Page = "access:background-tasks";

    /// <summary>Who did it, for the run history: the name, else the e-mail.</summary>
    public static string Actor(ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.Name) ?? user.FindFirstValue(ClaimTypes.Email) ?? "невідомий користувач";
}
