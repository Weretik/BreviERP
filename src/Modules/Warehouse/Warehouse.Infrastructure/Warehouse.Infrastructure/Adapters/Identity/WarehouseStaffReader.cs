using Identity.Contracts.Staff;

using Warehouse.Application.Abstractions;

namespace Warehouse.Infrastructure.Adapters.Identity;

/// <summary>Staff accounts (Identity) for «хто змінив» on «Склад тканин».</summary>
internal sealed class WarehouseStaffReader(IStaffDirectory staff) : IWarehouseStaffReader
{
    public async Task<WarehouseStaffMember?> FindAsync(int userId, CancellationToken cancellationToken) =>
        Member(await staff.FindAsync(userId, cancellationToken));

    public async Task<WarehouseStaffMember?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
        Member(await staff.FindByEmailAsync(email, cancellationToken));

    private static WarehouseStaffMember? Member(StaffAccount? account) => account is null
        ? null
        : new WarehouseStaffMember(account.Id, account.DisplayName.Trim(), account.Email);
}
