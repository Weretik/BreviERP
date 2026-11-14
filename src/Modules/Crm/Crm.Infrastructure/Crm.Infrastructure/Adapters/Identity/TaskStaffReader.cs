using Crm.Application.Tasks.Staff;

using Identity.Contracts.Staff;

namespace Crm.Infrastructure.Adapters.Identity;

/// <summary>Staff of manager tasks: assignees are active users with the Manager role (TSK-001 R-04).</summary>
internal sealed class TaskStaffReader(IStaffDirectory staff) : ITaskStaffReader
{
    public async Task<IReadOnlyList<TaskStaffMember>> ListAssigneesAsync(CancellationToken cancellationToken) =>
        (await staff.ListActiveAsync([StaffRoles.Manager], cancellationToken)).Select(Member).ToArray();

    public async Task<TaskStaffMember?> FindAssigneeAsync(int userId, CancellationToken cancellationToken)
    {
        var account = await ActiveAsync(userId, cancellationToken);
        return account is not null && account.IsInAnyRole(StaffRoles.Manager) ? Member(account) : null;
    }

    public async Task<TaskStaffMember?> FindAsync(int userId, CancellationToken cancellationToken) =>
        await ActiveAsync(userId, cancellationToken) is { } account ? Member(account) : null;

    private async Task<StaffAccount?> ActiveAsync(int userId, CancellationToken cancellationToken) =>
        await staff.FindAsync(userId, cancellationToken) is { IsLockedOut: false } account ? account : null;

    private static TaskStaffMember Member(StaffAccount account) => new(account.Id, account.DisplayName, account.Email);
}
