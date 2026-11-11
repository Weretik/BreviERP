using Warehouse.Application.Access;
using Warehouse.Application.Contracts;
using Warehouse.Domain.Stock;

namespace Warehouse.Application.Stock.OneOff;

/// <summary>
/// «Разова витрата / без замовлення»: the fabric really left the shelf, so the balance may go below zero; it covers no
/// order plan.
/// </summary>
public sealed record IssueFabricCommand(WarehouseActor Actor, IssueFabricInput Input)
    : ICommand<Result<FabricStockOperationResponse>>;

public sealed class IssueFabricCommandValidator : AbstractValidator<IssueFabricCommand>
{
    public IssueFabricCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.OperationId).OperationId();
            RuleFor(x => x.Input.FabricCode).NotEmpty().WithMessage("Оберіть тканину зі складу.")
                .MaximumLength(FabricStock.CodeMaxLength);
            RuleFor(x => x.Input.Color).NotEmpty().WithMessage("Оберіть колір зі складу.")
                .MaximumLength(StockColor.MaxLength);
            RuleFor(x => x.Input.ExpectedVersion).MaximumLength(64);
            RuleFor(x => x.Input.Meters).PositiveMeters();
            RuleFor(x => x.Input.OperationDate).OperationDate();
            RuleFor(x => x.Input.Destination).FormText();
            RuleFor(x => x.Input.Note).MaximumLength(FabricMovement.NoteMaxLength);
        });
    }
}

public sealed class IssueFabricCommandHandler(StockChanges changes, StockPairs pairs, StockOperations operations)
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
