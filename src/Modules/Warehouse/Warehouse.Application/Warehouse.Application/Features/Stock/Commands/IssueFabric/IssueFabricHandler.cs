using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Commands.IssueFabric;

public sealed class IssueFabricHandler(StockChanges changes, StockPairs pairs, StockOperations operations)
    : ICommandHandler<IssueFabricCommand, Result<FabricStockOperationResponse>>
{
    public const string Kind = "issue";

    public async ValueTask<Result<FabricStockOperationResponse>> Handle(IssueFabricCommand command,
        CancellationToken cancellationToken)
    {
        var input = command.Input;
        if (!command.Actor.CanMoveStock) return Result<FabricStockOperationResponse>.Forbidden(StockMessages.CannotMove);
        var hash = StockOperations.Hash(command.Actor.UserId, Kind, input with { ExpectedVersion = null });
        if (await operations.ReplayAsync(input.OperationId, hash, cancellationToken) is { } replay) return replay;
        var actorName = await changes.ActorNameAsync(command.Actor, cancellationToken);
        if (actorName is null) return Result<FabricStockOperationResponse>.Forbidden(StockMessages.CannotMove);

        var pair = await pairs.LoadAsync(input.FabricCode, input.Color, cancellationToken);
        if (pair.Lines.Count == 0) return StockPairs.Refused(StockPairRefusal.NotHeld);
        if (input.ExpectedVersion is { Length: > 0 } version && version != pair.Version)
            return Result<FabricStockOperationResponse>.Conflict(StockMessages.Stale);
        var result = pair.Issue(input.Meters,
            new FabricMovementDetails(input.OperationDate, input.Note, Destination: input.Destination,
                OperationId: input.OperationId), actorName, pairs.Now);
        if (!result.IsDone) return StockPairs.Refused(result.Refusal!.Value);
        return await pairs.SaveAsync(command.Actor, input.OperationId, Kind, hash, pair, result, cancellationToken);
    }
}
