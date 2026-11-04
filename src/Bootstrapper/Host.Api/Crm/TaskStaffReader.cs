using Crm.Application.Tasks.Staff;
using Identity.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Host.Api.Crm;

/// <summary>Staff of manager tasks: assignees are active users with the Manager role (TSK-001 R-04).</summary>
internal sealed class TaskStaffReader(UserManager<AppUser> users) : ITaskStaffReader
{
    private const string ManagerRole = "Manager";

    public async Task<IReadOnlyList<TaskStaffMember>> ListAssigneesAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        return await users.Users.AsNoTracking()
            .Where(user => (!user.LockoutEnabled || user.LockoutEnd == null || user.LockoutEnd <= now) &&
                user.UserRoles.Any(role => role.Role.Name == ManagerRole))
            .OrderBy(user => user.FullName).ThenBy(user => user.Id)
            .Select(user => new TaskStaffMember(user.Id,
                user.FullName == "" ? user.Email ?? "Користувач #" + user.Id : user.FullName, user.Email))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<TaskStaffMember?> FindAssigneeAsync(int userId, CancellationToken cancellationToken)
    {
        var (user, member) = await FindActiveAsync(userId, cancellationToken);
        return user is not null && await users.IsInRoleAsync(user, ManagerRole) ? member : null;
    }

    public async Task<TaskStaffMember?> FindAsync(int userId, CancellationToken cancellationToken) =>
        (await FindActiveAsync(userId, cancellationToken)).Member;

    private async Task<(AppUser? User, TaskStaffMember? Member)> FindActiveAsync(int userId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await users.FindByIdAsync(userId.ToString(System.Globalization.CultureInfo.InvariantCulture));
        if (user is null || await users.IsLockedOutAsync(user)) return (null, null);
        var name = string.IsNullOrWhiteSpace(user.FullName) ? user.Email ?? $"Користувач #{user.Id}" : user.FullName;
        return (user, new TaskStaffMember(user.Id, name, user.Email));
    }
}
