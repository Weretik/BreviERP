using Warehouse.Application.Access;
using Warehouse.Application.Colors;
using Warehouse.Application.Contracts;
using Warehouse.Domain.Stock;

namespace Warehouse.Application.Stock.Revisions;

/// <summary>
/// Edits one movement: date, metres, supplier and document of a receipt, destination of an issue; a receipt may move to
/// another offer and colour, an issue to another pair the stock holds. The chains of both lines are restated.
/// </summary>
public sealed record EditFabricMovementCommand(WarehouseActor Actor, int MovementId, EditFabricMovementInput Input)
    : ICommand<Result<FabricStockOperationResponse>>;

public sealed class EditFabricMovementCommandValidator : AbstractValidator<EditFabricMovementCommand>
{
    public EditFabricMovementCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.MovementId).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.OperationId).OperationId();
            RuleFor(x => x.Input.ExpectedRevision).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Input.OperationDate).OperationDate();
            RuleFor(x => x.Input.Meters).Must(FabricStock.IsValidMeters).WithMessage(StockMessages.Meters);
            RuleFor(x => x.Input.FabricCode).MaximumLength(FabricStock.CodeMaxLength);
            RuleFor(x => x.Input.Color).MaximumLength(StockColor.MaxLength);
            RuleFor(x => x.Input.SupplierName).FormText();
            RuleFor(x => x.Input.Document).FormText();
            RuleFor(x => x.Input.Destination).FormText();
            RuleFor(x => x.Input.Reason).MaximumLength(FabricMovementRevision.ReasonMaxLength);
        });
    }
}

public sealed class EditFabricMovementCommandHandler(StockChanges changes, StockPairs pairs,
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
            issue ? input.Destination : movement.Destination, false);
        if (state with { StockId = 0 } == movement.State with { StockId = 0 } && ReferenceEquals(from, to))
            return Invalid("Meters", "Нічого не змінено.");
        var left = ReferenceEquals(from, to) ? movement.Delta : 0m;
        if (!FabricStock.IsValidMeters(to.Meters - left + delta.Value)) return Invalid("Meters", StockMessages.OutOfRange);

        var revision = to.Revise(movement, state, FabricMovementRevisionAction.Edit, input.Reason, actorName,
            pairs.Now, input.OperationId);
        return await revisions.SaveAsync(command.Actor, input.OperationId, Kind, hash, revision, from, to,
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
            case FabricMovementKind.Receipt when input.FabricId is { } fabricId:
            {
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
