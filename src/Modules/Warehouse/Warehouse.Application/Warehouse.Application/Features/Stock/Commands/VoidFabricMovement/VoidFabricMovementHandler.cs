using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;
using Warehouse.Domain.Stock.Movements;
using Warehouse.Application.Features.Stock.Shared.Operations;
using BuildingBlocks.Application;

namespace Warehouse.Application.Features.Stock.Commands.VoidFabricMovement;

public sealed class VoidFabricMovementHandler(StockChanges changes, StockPairs pairs,
    StockOperations operations, MovementRevisions revisions)
    : ICommandHandler<VoidFabricMovementCommand, Result<FabricStockOperationResponse>>
{
    public const string Kind = "movement-void";

    public async ValueTask<Result<FabricStockOperationResponse>> Handle(VoidFabricMovementCommand command,
        CancellationToken cancellationToken)
    {
        var input = command.Input;
        if (!command.Actor.CanMoveStock) return Result<FabricStockOperationResponse>.Forbidden(StockMessages.CannotMove);
        var hash = StockOperations.Hash(command.Actor.UserId, Kind,
            new { command.MovementId, Input = input with { ExpectedRevision = 0 } });
        if (await operations.ReplayAsync(input.OperationId, hash, cancellationToken) is { } replay) return replay;
        var actorName = await changes.ActorNameAsync(command.Actor, cancellationToken);
        if (actorName is null) return Result<FabricStockOperationResponse>.Forbidden(StockMessages.CannotMove);
        var loaded = await revisions.LoadAsync(command.MovementId, input.ExpectedRevision, false, cancellationToken);
        if (!loaded.IsSuccess) return loaded.Map(_ => default(FabricStockOperationResponse)!);
        var movement = loaded.Value;
        var line = movement.Stock;

        var revision = line.Revise(movement, movement.State with { Delta = 0, IsVoided = true },
            FabricMovementRevisionAction.Void, input.Reason, actorName, pairs.Now, input.OperationId);
        if (revision.IsFailure) return revision.Error!.ToResult<FabricStockOperationResponse>();
        return await revisions.SaveAsync(command.Actor, input.OperationId, Kind, hash, revision.Value, line, line,
            cancellationToken);
    }
}
