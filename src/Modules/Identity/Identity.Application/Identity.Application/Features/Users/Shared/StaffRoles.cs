using Identity.Domain.Authorization;

namespace Identity.Application.Features.Users.Shared;

/// <summary>Roles of staff accounts, strongest first.</summary>
public static class StaffRoles
{
    public static readonly IReadOnlyList<string> All = [RoleNames.Director, RoleNames.Admin, RoleNames.Accountant, RoleNames.Manager, RoleNames.WorkshopChief];

    public static bool IsStaffRole(string? role) => role is not null && All.Contains(role, StringComparer.Ordinal);

    /// <summary>
    /// Identity roles stored for a staff role. The director also holds Admin, so every Admin-only endpoint lets the director in.
    /// </summary>
    public static IReadOnlyList<string> GrantedRoles(string role) =>
        role == RoleNames.Director ? [RoleNames.Director, RoleNames.Admin] : [role];
}
