using Warehouse.Application.Access;
using Warehouse.Application.Colors;
using Warehouse.Application.Contracts;
using Warehouse.Domain.Stock;

namespace Warehouse.Application.Stock.Receipts;

/// <summary>
/// «Прихід»: the line of the exact offer picked grows (a new line when the pair never held that code). The supplier is
/// the name typed or picked, not a required counterparty.
/// </summary>
public sealed record ReceiveFabricCommand(WarehouseActor Actor, ReceiveFabricInput Input)
    : ICommand<Result<FabricStockOperationResponse>>;

public sealed class ReceiveFabricCommandValidator : AbstractValidator<ReceiveFabricCommand>
{
    public ReceiveFabricCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.OperationId).OperationId();
            RuleFor(x => x.Input.FabricId).GreaterThan(0).WithMessage("Оберіть тканину.");
            RuleFor(x => x.Input.Color).NotEmpty().WithMessage("Оберіть колір.").MaximumLength(StockColor.MaxLength);
            RuleFor(x => x.Input.Meters).PositiveMeters();
            RuleFor(x => x.Input.OperationDate).OperationDate();
            RuleFor(x => x.Input.SupplierName).FormText();
            RuleFor(x => x.Input.Document).FormText();
            RuleFor(x => x.Input.Note).MaximumLength(FabricMovement.NoteMaxLength);
        });
    }
}

public sealed class ReceiveFabricCommandHandler(StockChanges changes, StockPairs pairs, StockOperations operations,
    WarehouseColors colors) : ICommandHandler<ReceiveFabricCommand, Result<FabricStockOperationResponse>>
{
    public const string Kind = "receipt";

    public async ValueTask<Result<FabricStockOperationResponse>> Handle(ReceiveFabricCommand command,
        CancellationToken cancellationToken)
    {
        var input = command.Input;
        if (!command.Actor.CanMoveStock) return Result<FabricStockOperationResponse>.Forbidden(StockMessages.CannotMove);
        var hash = StockOperations.Hash(command.Actor.UserId, Kind, input);
        if (await operations.ReplayAsync(input.OperationId, hash, cancellationToken) is { } replay) return replay;
        var actorName = await changes.ActorNameAsync(command.Actor, cancellationToken);
        if (actorName is null) return Result<FabricStockOperationResponse>.Forbidden(StockMessages.CannotMove);
        var fabric = await changes.OfferAsync(input.FabricId, cancellationToken);
        if (!fabric.IsSuccess) return fabric.Map(_ => default(FabricStockOperationResponse)!);
        var colorErrors = await colors.CheckAsync([input.Color], cancellationToken);
        if (colorErrors.Count > 0) return Result<FabricStockOperationResponse>.Invalid(colorErrors.ToArray());

        var pair = await pairs.LoadAsync(fabric.Value.Code, input.Color, cancellationToken);
        var result = pair.Receive(fabric.Value, input.Color, input.Meters,
            new FabricMovementDetails(input.OperationDate, input.Note, SupplierName: input.SupplierName,
                Document: input.Document, OperationId: input.OperationId), actorName, pairs.Now);
        if (!result.IsDone) return StockPairs.Refused(result.Refusal!.Value);
        return await pairs.SaveAsync(command.Actor, input.OperationId, Kind, hash, pair, result, cancellationToken);
    }
}
