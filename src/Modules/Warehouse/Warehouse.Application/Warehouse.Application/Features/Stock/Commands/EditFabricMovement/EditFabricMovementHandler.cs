using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;
using Warehouse.Domain.Stock.Movements;
using Warehouse.Application.Features.Stock.Shared.Operations;

namespace Warehouse.Application.Features.Stock.Commands.EditFabricMovement;

public sealed class EditFabricMovementHandler(StockChanges changes, StockPairs pairs,
    StockOperations operations, MovementRevisions revisions, WarehouseColors colors)
    : ICommandHandler<EditFabricMovementCommand, Result<FabricStockOperationResponse>>
{
    public const string Kind = "movement-edit";

    public async ValueTask<Result<FabricStockOperationResponse>> Handle(EditFabricMovementCommand command,
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
        var from = movement.Stock;

        var delta = Delta(movement.Kind, input.Meters);
        if (delta is null) return Invalid("Meters", StockMessages.WrongSign);
        var target = await TargetAsync(movement, input, cancellationToken);
        if (!target.IsSuccess) return target.Map(_ => default(FabricStockOperationResponse)!);
        var to = target.Value;

        var receipt = movement.Kind == FabricMovementKind.Receipt;
        var issue = movement.Kind == FabricMovementKind.Issue;
        var state = new FabricMovementState(to.Id, input.OperationDate, delta.Value,
            receipt ? input.SupplierName : movement.SupplierName, receipt ? input.Document : movement.Document,
            issue && movement.ItemId is null ? input.Destination : movement.Destination, false);
        if (state with { StockId = 0 } == movement.State with { StockId = 0 } && ReferenceEquals(from, to))
            return Invalid("Meters", "Нічого не змінено.");
        if (!ReferenceEquals(from, to) && await revisions.IssueSourcesAsync(movement, cancellationToken) > 1)
            return Invalid("FabricCode", StockMessages.MultiSourceTarget);
        var left = ReferenceEquals(from, to) ? movement.Delta : 0m;
        if (!FabricStock.IsValidMeters(to.Meters - left + delta.Value)) return Invalid("Meters", StockMessages.OutOfRange);

        var revision = to.Revise(movement, state, FabricMovementRevisionAction.Edit, input.Reason, actorName,
            pairs.Now, input.OperationId);
        if (revision.IsFailure) return revision.Error!.ToResult<FabricStockOperationResponse>();
        return await revisions.SaveAsync(command.Actor, input.OperationId, Kind, hash, revision.Value, from, to,
            cancellationToken);
    }

    /// <summary>The signed change the kind allows for the amount entered, or null.</summary>
    private static decimal? Delta(FabricMovementKind kind, decimal meters) => kind switch
    {
        FabricMovementKind.Opening when meters >= 0 => meters,
        FabricMovementKind.Receipt when meters > 0 => meters,
        FabricMovementKind.Issue when meters > 0 => -meters,
        FabricMovementKind.Adjustment when meters != 0 => meters,
        _ => null
    };

    /// <summary>The line the movement goes to: the same, another offer and colour of a receipt, or a held pair of an issue.</summary>
    private async Task<Result<FabricStock>> TargetAsync(FabricMovement movement, EditFabricMovementInput input,
        CancellationToken cancellationToken)
    {
        var from = movement.Stock;
        switch (movement.Kind)
        {
            case FabricMovementKind.Receipt when input.FabricId is not null || !string.IsNullOrWhiteSpace(input.Color):
            {
                // Only a colour: the same offer in another colour.
                if ((input.FabricId ?? from.FabricId) is not { } fabricId)
                    return Result<FabricStock>.Invalid(new ValidationError("FabricId", StockMessages.FabricNotFound));
                var fabric = await changes.OfferAsync(fabricId, cancellationToken);
                if (!fabric.IsSuccess) return fabric.Map(_ => default(FabricStock)!);
                var color = string.IsNullOrWhiteSpace(input.Color) ? from.Color : input.Color;
                var colorErrors = await colors.CheckAsync([color], cancellationToken);
                if (colorErrors.Count > 0) return Result<FabricStock>.Invalid(colorErrors.ToArray());
                if (FabricAliases.Normalize(fabric.Value.Code) == from.FabricCode &&
                    StockColor.Key(color) == from.ColorKey) return Result.Success(from);
                return Result.Success(await changes.LineAsync(fabric.Value, color, cancellationToken));
            }
            case FabricMovementKind.Issue when !string.IsNullOrWhiteSpace(input.FabricCode):
            {
                var pair = await pairs.LoadAsync(input.FabricCode, input.Color ?? from.Color, cancellationToken);
                if (pair.Lines.Count == 0) return Result<FabricStock>.NotFound(StockMessages.NotHeld);
                return Result.Success(pair.Lines.Contains(from) ? from : pair.Lines[0]);
            }
            case FabricMovementKind.Opening or FabricMovementKind.Adjustment
                when input.FabricId is not null || !string.IsNullOrWhiteSpace(input.FabricCode):
                return Result<FabricStock>.Invalid(new ValidationError("FabricId", StockMessages.NoTargetChange));
            default:
                return Result.Success(from);
        }
    }

    private static Result<FabricStockOperationResponse> Invalid(string field, string message) =>
        Result<FabricStockOperationResponse>.Invalid(new ValidationError(field, message));
}
