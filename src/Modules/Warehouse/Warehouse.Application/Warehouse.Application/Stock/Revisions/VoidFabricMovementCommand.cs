using Warehouse.Application.Access;
using Warehouse.Application.Contracts;
using Warehouse.Domain.Stock;

namespace Warehouse.Application.Stock.Revisions;

/// <summary>«Скасувати»: the movement stays in the history with no effect on the balance; the original is in a revision.</summary>
public sealed record VoidFabricMovementCommand(WarehouseActor Actor, int MovementId, VoidFabricMovementInput Input)
    : ICommand<Result<FabricStockOperationResponse>>;

public sealed class VoidFabricMovementCommandValidator : AbstractValidator<VoidFabricMovementCommand>
{
    public VoidFabricMovementCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.MovementId).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.OperationId).OperationId();
            RuleFor(x => x.Input.ExpectedRevision).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Input.Reason).RevisionReason();
        });
    }
}

public sealed class VoidFabricMovementCommandHandler(StockChanges changes, StockPairs pairs,
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
        return await revisions.SaveAsync(command.Actor, input.OperationId, Kind, hash, revision, line, line,
            cancellationToken);
    }
}
