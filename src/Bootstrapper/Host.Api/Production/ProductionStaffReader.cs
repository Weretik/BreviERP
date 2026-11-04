using Identity.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Production.Application.Abstractions;

namespace Host.Api.Production;

/// <summary>Staff accounts (Identity) for «хто змінив» and the manager of a moved order.</summary>
internal sealed class ProductionStaffReader(UserManager<AppUser> users) : IProductionStaffReader
{
    public async Task<ProductionStaffMember?> FindAsync(int userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Member(await users.FindByIdAsync(userId.ToString(System.Globalization.CultureInfo.InvariantCulture)));
    }

    public async Task<ProductionStaffMember?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Member(await users.FindByEmailAsync(email));
    }

    private static ProductionStaffMember? Member(AppUser? user) => user is null
        ? null
        : new ProductionStaffMember(user.Id,
            string.IsNullOrWhiteSpace(user.FullName) ? user.Email ?? $"Користувач #{user.Id}" : user.FullName.Trim(),
            user.Email);
}
