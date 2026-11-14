
using Identity.Contracts.Staff;
using Crm.Application.Abstractions.Quotes;

namespace Crm.Infrastructure.Adapters.Identity;

/// <summary>Staff who may own a КП: not blocked, role Manager or Admin (the Director also holds Admin).</summary>
internal sealed class QuoteStaffReader(IStaffDirectory staff) : IQuoteStaffReader
{
    private static readonly string[] Roles = [StaffRoles.Manager, StaffRoles.Admin];

    public async Task<IReadOnlyList<QuoteStaffMember>> ListAsync(CancellationToken cancellationToken) =>
        (await staff.ListActiveAsync(Roles, cancellationToken)).Select(Member).ToArray();

    public async Task<QuoteStaffMember?> FindAsync(int userId, CancellationToken cancellationToken)
    {
        var account = await staff.FindAsync(userId, cancellationToken);
        return account is null || account.IsLockedOut || !account.IsInAnyRole(Roles) ? null : Member(account);
    }

    private static QuoteStaffMember Member(StaffAccount account) =>
        new(account.Id, account.DisplayName, account.Email, account.PhoneNumber);
}
