using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Commands.OpenFabricStock;

/// <summary>«Початковий залишок»: once per fabric and colour (any code of its group), zero allowed.</summary>
public sealed record OpenFabricStockCommand(WarehouseActor Actor, OpenFabricStockInput Input)
    : ICommand<Result<FabricStockOperationResponse>>;

public sealed class OpenFabricStockValidator : AbstractValidator<OpenFabricStockCommand>
{
    public OpenFabricStockValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.OperationId).OperationId();
            RuleFor(x => x.Input.FabricId).GreaterThan(0).WithMessage("Оберіть тканину.");
            RuleFor(x => x.Input.Color).NotEmpty().WithMessage("Оберіть колір.").MaximumLength(StockColor.MaxLength);
            RuleFor(x => x.Input.Meters).Must(x => x >= 0 && FabricStock.IsValidMeters(x))
                .WithMessage("Початковий залишок — від 0 до 1 000 000 м, до сотих.");
            RuleFor(x => x.Input.OperationDate).OperationDate();
        });
    }
}

public sealed class OpenFabricStockHandler(StockChanges changes, StockPairs pairs, StockOperations operations,
    WarehouseColors colors) : ICommandHandler<OpenFabricStockCommand, Result<FabricStockOperationResponse>>
{
    public const string Kind = "opening";

    public async ValueTask<Result<FabricStockOperationResponse>> Handle(OpenFabricStockCommand command,
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
        var result = pair.Open(fabric.Value, input.Color, input.Meters,
            new FabricMovementDetails(input.OperationDate, "Початковий залишок", OperationId: input.OperationId),
            actorName, pairs.Now);
        if (!result.IsDone) return StockPairs.Refused(result.Refusal!.Value);
        return await pairs.SaveAsync(command.Actor, input.OperationId, Kind, hash, pair, result, cancellationToken);
    }
}
