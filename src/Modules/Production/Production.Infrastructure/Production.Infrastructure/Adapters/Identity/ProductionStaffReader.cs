using Identity.Contracts.Staff;

using Production.Application.Abstractions;

namespace Production.Infrastructure.Adapters.Identity;

/// <summary>Staff accounts (Identity) for «хто змінив» and the manager of a moved order.</summary>
internal sealed class ProductionStaffReader(IStaffDirectory staff) : IProductionStaffReader
{
    public async Task<ProductionStaffMember?> FindAsync(int userId, CancellationToken cancellationToken) =>
        Member(await staff.FindAsync(userId, cancellationToken));

    public async Task<ProductionStaffMember?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
        Member(await staff.FindByEmailAsync(email, cancellationToken));

    private static ProductionStaffMember? Member(StaffAccount? account) => account is null
        ? null
        : new ProductionStaffMember(account.Id, account.DisplayName.Trim(), account.Email);
}
