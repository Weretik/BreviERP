using Warehouse.Application.Abstractions;
using Warehouse.Application.Access;
using Warehouse.Application.Contracts;
using Warehouse.Domain.Stock;

namespace Warehouse.Application.Stock.Revisions;

/// <summary>«Історія правок» of a movement: every edit, void and restore with the values before and after.</summary>
public sealed record GetFabricMovementHistoryQuery(WarehouseActor Actor, int MovementId)
    : IQuery<Result<FabricMovementHistoryResponse>>;

public sealed class GetFabricMovementHistoryQueryHandler(IWarehouseDbContext db)
    : IQueryHandler<GetFabricMovementHistoryQuery, Result<FabricMovementHistoryResponse>>
{
    public async ValueTask<Result<FabricMovementHistoryResponse>> Handle(GetFabricMovementHistoryQuery query,
        CancellationToken cancellationToken)
    {
        var movement = await db.FabricMovements.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == query.MovementId, cancellationToken);
        if (movement is null) return Result<FabricMovementHistoryResponse>.NotFound(StockMessages.MovementNotFound);
        var revisions = await db.FabricMovementRevisions.AsNoTracking().Where(x => x.MovementId == movement.Id)
            .ToListAsync(cancellationToken);
        var ids = revisions.SelectMany(x => new[] { x.BeforeStockId, x.AfterStockId }).Distinct().ToArray();
        var lines = await db.FabricStocks.AsNoTracking().Where(x => ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        FabricMovementStateResponse State(FabricMovementState x)
        {
            var line = lines.GetValueOrDefault(x.StockId);
            return new FabricMovementStateResponse(line?.FabricCode ?? "—", line?.Color ?? "—", x.OperationDate, x.Delta,
                x.SupplierName, x.Document, x.Destination, x.IsVoided);
        }

        var last = revisions.Where(x => !x.IsUndone && x.Action != FabricMovementRevisionAction.Restore)
            .MaxBy(x => x.Number);
        var canRestore = query.Actor.CanMoveStock && movement.CanRevise && last is not null &&
                         last.After == movement.State;
        var rows = revisions.OrderByDescending(x => x.Number).Select(x => new FabricMovementRevisionResponse(x.Id,
            x.Number, x.Action.ToString(), State(x.Before), State(x.After), x.Reason, x.CreatedByName, x.CreatedAt,
            x.IsUndone)).ToList();
        return Result.Success(new FabricMovementHistoryResponse(movement.Id, movement.Revision, canRestore, rows));
    }
}
