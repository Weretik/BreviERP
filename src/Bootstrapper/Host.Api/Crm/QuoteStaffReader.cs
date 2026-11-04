using Crm.Application.Quotes.Staff;
using Identity.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Host.Api.Crm;

/// <summary>Staff who may own a КП: not blocked, role Manager or Admin (the Director also holds Admin).</summary>
internal sealed class QuoteStaffReader(UserManager<AppUser> users) : IQuoteStaffReader
{
    public async Task<IReadOnlyList<QuoteStaffMember>> ListAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        return await users.Users.AsNoTracking()
            .Where(user => (!user.LockoutEnabled || user.LockoutEnd == null || user.LockoutEnd <= now) &&
                user.UserRoles.Any(role => role.Role.Name == "Manager" || role.Role.Name == "Admin"))
            .OrderBy(user => user.FullName).ThenBy(user => user.Id)
            .Select(user => new QuoteStaffMember(user.Id, user.FullName == "" ? user.Email ?? "Користувач #" + user.Id : user.FullName, user.Email, user.PhoneNumber))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<QuoteStaffMember?> FindAsync(int userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await users.FindByIdAsync(userId.ToString(System.Globalization.CultureInfo.InvariantCulture));
        if (user is null || await users.IsLockedOutAsync(user)) return null;
        if (!await users.IsInRoleAsync(user, "Manager") && !await users.IsInRoleAsync(user, "Admin")) return null;
        var name = string.IsNullOrWhiteSpace(user.FullName) ? user.Email ?? $"Користувач #{user.Id}" : user.FullName;
        return new QuoteStaffMember(user.Id, name, user.Email, user.PhoneNumber);
    }
}
