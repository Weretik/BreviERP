namespace ActivityLog.Application.Abstractions;

/// <summary>The display name of a staff account at the moment the entry is stored (ACT-001 R-06).</summary>
public interface IActorNames
{
    /// <returns>The full name, else null (then the login is shown).</returns>
    Task<string?> FindAsync(int userId, CancellationToken cancellationToken);
}
