using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;
using Warehouse.Domain.Stock.Movements;
using Warehouse.Application.Features.Stock.Shared.Operations;

namespace Warehouse.Application.Features.Stock.Commands.ReceiveFabric;

public sealed class ReceiveFabricHandler(StockChanges changes, StockPairs pairs, StockOperations operations,
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
