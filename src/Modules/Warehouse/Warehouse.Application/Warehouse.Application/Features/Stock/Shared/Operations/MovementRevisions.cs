using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Domain.Stock.Movements;

namespace Warehouse.Application.Features.Stock.Shared.Operations;

/// <summary>
/// Common steps of an edit, a void and a restore of a movement: the movement and its revision guard, the restatement of
/// the lines it left and joined, the operation id and saving.
/// </summary>
public sealed class MovementRevisions(IWarehouseDbContext db, StockOperations operations, StockPairs pairs)
{
    /// <summary>The movement, or why it may not be revised now.</summary>
    public async Task<Result<FabricMovement>> LoadAsync(int movementId, int expectedRevision, bool allowVoided,
        CancellationToken cancellationToken)
    {
        var movement = await db.FabricMovements.Include(x => x.Stock)
            .SingleOrDefaultAsync(x => x.Id == movementId, cancellationToken);
        if (movement is null) return Result<FabricMovement>.NotFound(StockMessages.MovementNotFound);
        if (!movement.CanRevise) return Result<FabricMovement>.Conflict(StockMessages.CannotRevise);
        if (movement.Revision != expectedRevision) return Result<FabricMovement>.Conflict(StockMessages.RevisionStale);
        if (movement.IsVoided && !allowVoided) return Result<FabricMovement>.Conflict(StockMessages.Voided);
        return Result.Success(movement);
    }

    /// <summary>Lines of a «Видача» this movement belongs to follow its new metres, line and void (TZ §8).</summary>
    private async Task SyncIssueLineAsync(FabricMovement movement, CancellationToken cancellationToken)
    {
        if (movement.IssueLineId is not { } lineId) return;
        var issue = await db.FabricIssues.Include(x => x.Lines).ThenInclude(x => x.Movements).ThenInclude(x => x.Stock)
            .SingleAsync(x => x.Lines.Any(l => l.Id == lineId), cancellationToken);
        issue.Lines.Single(x => x.Id == lineId).Sync();
    }

    /// <summary>How many movements the issue line of <paramref name="movement"/> has; 0 when it has none.</summary>
    public Task<int> IssueSourcesAsync(FabricMovement movement, CancellationToken cancellationToken) =>
        movement.IssueLineId is { } lineId
            ? db.FabricMovements.CountAsync(x => x.IssueLineId == lineId, cancellationToken)
            : Task.FromResult(0);

    /// <summary>
    /// Adds the revision, restates every line the movement left or joined, records the operation and saves. A change by
    /// someone else in between is a conflict; a parallel repeat of the same id returns the saved result.
    /// </summary>
    public async Task<Result<FabricStockOperationResponse>> SaveAsync(WarehouseActor actor, Guid operationId,
        string kind, string hash, FabricMovementRevision revision, FabricStock from, FabricStock to,
        CancellationToken cancellationToken)
    {
        db.FabricMovementRevisions.Add(revision);
        var lines = new[] { from, to }.Distinct().ToList();
        var ids = lines.Where(x => x.Id != 0).Select(x => x.Id).ToArray();
        var saved = await db.FabricMovements.Where(x => ids.Contains(x.StockId)).ToListAsync(cancellationToken);
        var all = saved.Append(revision.Movement).Distinct().ToList();
        foreach (var line in lines) line.Restate(all.Where(x => ReferenceEquals(x.Stock, line)));
        await SyncIssueLineAsync(revision.Movement, cancellationToken);

        // Saved lines come back as the tracked, restated instances; a line this change created is not saved yet.
        var pair = await pairs.LoadAsync(to.FabricCode, to.Color, cancellationToken);
        var meters = pair.Lines.Contains(to) ? pair.Meters : pair.Meters + to.Meters;
        db.StockOperations.Add(new StockOperation(operationId, kind, hash, actor.UserId, pair.Code, to.ColorKey,
            to.Color, meters, pairs.Now));
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            if (await operations.ReplayAsync(operationId, hash, cancellationToken) is { } replay) return replay;
            return Result<FabricStockOperationResponse>.Conflict(StockMessages.RevisionStale);
        }

        return Result.Success(new FabricStockOperationResponse(operationId, pair.Code, to.Color, meters, false));
    }
}
