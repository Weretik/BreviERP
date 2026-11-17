namespace ActivityLog.Application.Features.Entries.Commands.DeleteOldActivityEntries;

/// <summary>
/// ACT-001 R-12 (owner 2026-11-17): the journal keeps one year. One SQL <c>DELETE</c> by the time of the action, on the
/// <c>(OccurredAt, Id)</c> index; a repeat finds nothing to delete, and a stop half-way leaves only whole rows.
/// </summary>
public sealed class DeleteOldActivityEntriesHandler(IActivityLogDbContext db, TimeProvider clock)
    : ICommandHandler<DeleteOldActivityEntriesCommand, Result<int>>
{
    public const int KeepYears = 1;

    public async ValueTask<Result<int>> Handle(DeleteOldActivityEntriesCommand command,
        CancellationToken cancellationToken)
    {
        var before = clock.GetUtcNow().AddYears(-KeepYears);
        var deleted = await db.Entries.Where(x => x.OccurredAt < before).ExecuteDeleteAsync(cancellationToken);
        return Result.Success(deleted);
    }
}
