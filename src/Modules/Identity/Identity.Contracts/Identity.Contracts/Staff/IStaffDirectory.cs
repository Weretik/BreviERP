namespace Identity.Contracts.Staff;

/// <summary>Staff accounts for other modules («хто змінив», responsible managers, task assignees).</summary>
public interface IStaffDirectory
{
    Task<StaffAccount?> FindAsync(int userId, CancellationToken cancellationToken);

    Task<StaffAccount?> FindByEmailAsync(string email, CancellationToken cancellationToken);

    /// <summary>Accounts that are not blocked and hold at least one of the roles, by full name then id.</summary>
    Task<IReadOnlyList<StaffAccount>> ListActiveAsync(IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken);
}
