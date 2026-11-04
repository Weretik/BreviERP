namespace Crm.Application.Tasks.Staff;

/// <summary>Staff of manager tasks over Identity: who can receive a task and the names of who changed it.</summary>
public interface ITaskStaffReader
{
    /// <summary>Active (not blocked) users with the Manager role, by name (R-04).</summary>
    Task<IReadOnlyList<TaskStaffMember>> ListAssigneesAsync(CancellationToken cancellationToken);

    /// <summary>Null when the user does not exist, is blocked or is not a Manager.</summary>
    Task<TaskStaffMember?> FindAssigneeAsync(int userId, CancellationToken cancellationToken);

    /// <summary>The current name of any active user, for the author and history rows.</summary>
    Task<TaskStaffMember?> FindAsync(int userId, CancellationToken cancellationToken);
}

public sealed record TaskStaffMember(int UserId, string Name, string? Email);
