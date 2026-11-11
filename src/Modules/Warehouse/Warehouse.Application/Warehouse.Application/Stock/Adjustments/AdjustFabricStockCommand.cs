using Warehouse.Application.Access;
using Warehouse.Application.Contracts;
using Warehouse.Domain.Stock;

namespace Warehouse.Application.Stock.Adjustments;

/// <summary>
/// «Уточнити»: the counted total of a pair replaces its balance through one adjustment with a reason. It may not end
/// below min(0, current total); a real shortage is recorded as an issue instead.
/// </summary>
public sealed record AdjustFabricStockCommand(WarehouseActor Actor, AdjustFabricStockInput Input)
    : ICommand<Result<FabricStockOperationResponse>>;

public sealed class AdjustFabricStockCommandValidator : AbstractValidator<AdjustFabricStockCommand>
{
    public const int ReasonMinLength = 3;
    public const int ReasonMaxLength = 500;

    public AdjustFabricStockCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.OperationId).OperationId();
            RuleFor(x => x.Input.FabricCode).NotEmpty().MaximumLength(FabricStock.CodeMaxLength);
            RuleFor(x => x.Input.Color).NotEmpty().MaximumLength(StockColor.MaxLength);
            RuleFor(x => x.Input.ExpectedVersion).NotEmpty().WithMessage(StockMessages.Stale).MaximumLength(64);
            RuleFor(x => x.Input.TargetMeters).Must(FabricStock.IsValidMeters)
                .WithMessage("Фактичний залишок — число до 1 000 000 м, до сотих.");
            RuleFor(x => x.Input.OperationDate).OperationDate();
            RuleFor(x => x.Input.Reason).Must(x => x?.Trim().Length is >= ReasonMinLength and <= ReasonMaxLength)
                .WithMessage(StockMessages.Reason);
        });
    }
}

public sealed class AdjustFabricStockCommandHandler(StockChanges changes, StockPairs pairs, StockOperations operations)
    : ICommandHandler<AdjustFabricStockCommand, Result<FabricStockOperationResponse>>
{
    public const string Kind = "adjustment";

    public async ValueTask<Result<FabricStockOperationResponse>> Handle(AdjustFabricStockCommand command,
        CancellationToken cancellationToken)
    {
        var input = command.Input;
        if (!command.Actor.CanMoveStock) return Result<FabricStockOperationResponse>.Forbidden(StockMessages.CannotMove);
        var hash = StockOperations.Hash(command.Actor.UserId, Kind, input with { ExpectedVersion = string.Empty });
        if (await operations.ReplayAsync(input.OperationId, hash, cancellationToken) is { } replay) return replay;
        var actorName = await changes.ActorNameAsync(command.Actor, cancellationToken);
        if (actorName is null) return Result<FabricStockOperationResponse>.Forbidden(StockMessages.CannotMove);

        var pair = await pairs.LoadAsync(input.FabricCode, input.Color, cancellationToken);
        if (pair.Lines.Count == 0) return StockPairs.Refused(StockPairRefusal.NotHeld);
        if (input.ExpectedVersion != pair.Version) return Result<FabricStockOperationResponse>.Conflict(StockMessages.Stale);
        var result = pair.Adjust(input.TargetMeters,
            new FabricMovementDetails(input.OperationDate, input.Reason.Trim(), OperationId: input.OperationId),
            actorName, pairs.Now);
        if (!result.IsDone) return StockPairs.Refused(result.Refusal!.Value);
        return await pairs.SaveAsync(command.Actor, input.OperationId, Kind, hash, pair, result, cancellationToken);
    }
}
