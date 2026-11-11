using Identity.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Warehouse.Application.Abstractions;

namespace Host.Api.Warehouse;

/// <summary>Staff accounts (Identity) for «хто змінив» on «Склад тканин».</summary>
internal sealed class WarehouseStaffReader(UserManager<AppUser> users) : IWarehouseStaffReader
{
    public async Task<WarehouseStaffMember?> FindAsync(int userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Member(await users.FindByIdAsync(userId.ToString(System.Globalization.CultureInfo.InvariantCulture)));
    }

    public async Task<WarehouseStaffMember?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Member(await users.FindByEmailAsync(email));
    }

    private static WarehouseStaffMember? Member(AppUser? user) => user is null
        ? null
        : new WarehouseStaffMember(user.Id,
            string.IsNullOrWhiteSpace(user.FullName) ? user.Email ?? $"Користувач #{user.Id}" : user.FullName.Trim(),
            user.Email);
}
