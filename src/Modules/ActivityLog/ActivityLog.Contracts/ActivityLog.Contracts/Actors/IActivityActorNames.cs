namespace ActivityLog.Contracts.Actors;

/// <summary>
/// The display name of a staff account when an entry is stored (ACT-001 R-06). Identity implements it, so «Журнал дій»
/// references no module and every module can write to it without a cycle (ADR-0019).
/// </summary>
public interface IActivityActorNames
{
    /// <returns>The full name, else null (then the login is shown).</returns>
    Task<string?> FindAsync(int userId, CancellationToken cancellationToken);
}
