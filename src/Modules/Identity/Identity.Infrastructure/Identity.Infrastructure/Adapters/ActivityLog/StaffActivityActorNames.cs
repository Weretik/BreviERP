using ActivityLog.Contracts.Actors;
using Identity.Contracts.Staff;

namespace Identity.Infrastructure.Adapters.ActivityLog;

/// <summary>The staff member's full name for «Журнал дій», else null (ADR-0019).</summary>
internal sealed class StaffActivityActorNames(IStaffDirectory staff) : IActivityActorNames
{
    public async Task<string?> FindAsync(int userId, CancellationToken cancellationToken)
    {
        var account = await staff.FindAsync(userId, cancellationToken);
        return string.IsNullOrWhiteSpace(account?.FullName) ? null : account.FullName.Trim();
    }
}
