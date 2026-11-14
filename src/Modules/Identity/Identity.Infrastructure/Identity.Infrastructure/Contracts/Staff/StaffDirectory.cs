using Identity.Contracts.Staff;
using Identity.Infrastructure.Entities;

namespace Identity.Infrastructure.Contracts.Staff;

internal sealed class StaffDirectory(UserManager<AppUser> users) : IStaffDirectory
{
    public async Task<StaffAccount?> FindAsync(int userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await AccountAsync(await users.FindByIdAsync(userId.ToString(CultureInfo.InvariantCulture)));
    }

    public async Task<StaffAccount?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await AccountAsync(await users.FindByEmailAsync(email));
    }

    public async Task<IReadOnlyList<StaffAccount>> ListActiveAsync(IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var names = roles.ToArray();
        var rows = await users.Users.AsNoTracking()
            .Where(user => (!user.LockoutEnabled || user.LockoutEnd == null || user.LockoutEnd <= now) &&
                user.UserRoles.Any(role => names.Contains(role.Role.Name!)))
            .OrderBy(user => user.FullName).ThenBy(user => user.Id)
            .Select(user => new
            {
                user.Id, user.FullName, user.Email, user.PhoneNumber,
                Roles = user.UserRoles.Select(role => role.Role.Name!).ToList()
            })
            .ToListAsync(cancellationToken);
        return rows.Select(x => new StaffAccount(x.Id, x.FullName, x.Email, x.PhoneNumber, false, x.Roles)).ToArray();
    }

    private async Task<StaffAccount?> AccountAsync(AppUser? user) => user is null
        ? null
        : new StaffAccount(user.Id, user.FullName, user.Email, user.PhoneNumber, await users.IsLockedOutAsync(user),
            (await users.GetRolesAsync(user)).ToArray());
}
