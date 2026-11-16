using ActivityLog.Application.Abstractions;
using Identity.Contracts.Staff;

namespace ActivityLog.Infrastructure.Adapters.Identity;

/// <summary>The staff member's full name from Identity (ADR-0006), else null.</summary>
internal sealed class StaffActorNames(IStaffDirectory staff) : IActorNames
{
    public async Task<string?> FindAsync(int userId, CancellationToken cancellationToken)
    {
        var account = await staff.FindAsync(userId, cancellationToken);
        return string.IsNullOrWhiteSpace(account?.FullName) ? null : account.FullName.Trim();
    }
}
