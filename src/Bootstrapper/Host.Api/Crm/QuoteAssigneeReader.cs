using Crm.Application.Quotes.Ownership;
using Identity.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Ardalis.Result;

namespace Host.Api.Crm;

public sealed class QuoteAssigneeReader(UserManager<AppUser> users) : IQuoteAssigneeReader
{
    public async Task<bool> IsEligibleAsync(int userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await users.FindByIdAsync(userId.ToString(System.Globalization.CultureInfo.InvariantCulture));
        return user is not null && !await users.IsLockedOutAsync(user) &&
            (await users.IsInRoleAsync(user, "Manager") || await users.IsInRoleAsync(user, "Admin"));
    }

    public async Task<PagedResult<IReadOnlyList<QuoteAssignee>>> SearchAsync(
        string? search, int page, int pageSize, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var query = users.Users.AsNoTracking().Where(user =>
            (!user.LockoutEnabled || user.LockoutEnd == null || user.LockoutEnd <= now) &&
            user.UserRoles.Any(role => role.Role.Name == "Manager" || role.Role.Name == "Admin"));
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToUpperInvariant();
            query = query.Where(user => user.FullName.ToUpper().Contains(term));
        }
        var count = await query.CountAsync(cancellationToken);
        var rows = await query.OrderBy(user => user.FullName).ThenBy(user => user.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(user => new QuoteAssignee(user.Id, user.FullName == "" ? "Користувач #" + user.Id : user.FullName))
            .ToArrayAsync(cancellationToken);
        return new PagedResult<IReadOnlyList<QuoteAssignee>>(new PagedInfo(page, pageSize,
            (long)Math.Ceiling(count / (double)pageSize), count), rows);
    }
}
