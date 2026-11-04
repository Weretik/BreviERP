using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Application.Orders.Access;
using Production.Application.Orders.Validation;
using Production.Domain.Stock;

namespace Production.Application.Stock.Movements;

internal static class MovementRules
{
    public static IRuleBuilderOptions<T, decimal> PositiveMeters<T>(this IRuleBuilder<T, decimal> rule) =>
        rule.Must(x => x > 0 && FabricStock.IsValidMeters(x)).WithMessage(StockMessages.Meters);

    public static IRuleBuilderOptions<T, string> Reason<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage(StockMessages.Note).MaximumLength(FabricMovement.NoteMaxLength);
}

/// <summary>«Початковий залишок» or «Прихід»; a new fabric and colour opens its line.</summary>
public sealed record CreateFabricMovementCommand(ProductionActor Actor, CreateFabricMovementInput Input)
    : ICommand<Result<FabricStockLineResponse>>;

public sealed class CreateFabricMovementCommandValidator : AbstractValidator<CreateFabricMovementCommand>
{
    public CreateFabricMovementCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.Kind)
                .Must(x => x is nameof(FabricMovementKind.Opening) or nameof(FabricMovementKind.Receipt))
                .WithMessage("Оберіть «Початковий залишок» або «Прихід».");
            RuleFor(x => x.Input.FabricId).GreaterThan(0).WithMessage("Оберіть тканину.");
            RuleFor(x => x.Input.Color).NotEmpty().WithMessage("Вкажіть колір.").MaximumLength(StockColor.MaxLength);
            RuleFor(x => x.Input.Meters).PositiveMeters();
            RuleFor(x => x.Input.OperationDate).NotEmpty().WithMessage("Вкажіть дату.");
            RuleFor(x => x.Input.SupplierId).GreaterThan(0).When(x => x.Input.SupplierId is not null);
            RuleFor(x => x.Input.Document).MaximumLength(FabricMovement.DocumentMaxLength);
            RuleFor(x => x.Input.Note).MaximumLength(FabricMovement.NoteMaxLength);
            RuleFor(x => x.Input.Version).GreaterThan(0).When(x => x.Input.Version is not null);
        });
    }
}

public sealed class CreateFabricMovementCommandHandler(StockChanges changes, IProductionSupplierReader suppliers)
    : ICommandHandler<CreateFabricMovementCommand, Result<FabricStockLineResponse>>
{
    public async ValueTask<Result<FabricStockLineResponse>> Handle(CreateFabricMovementCommand command,
        CancellationToken cancellationToken)
    {
        var input = command.Input;
        if (!command.Actor.CanMoveStock) return Result<FabricStockLineResponse>.Forbidden(StockMessages.CannotMove);
        var actorName = await changes.ActorNameAsync(command.Actor, cancellationToken);
        if (actorName is null) return Result<FabricStockLineResponse>.Forbidden(StockMessages.CannotMove);
        var fabric = await changes.FabricAsync(input.FabricId, cancellationToken);
        if (!fabric.IsSuccess) return fabric.Map(_ => default(FabricStockLineResponse)!);

        ProductionSupplier? supplier = null;
        if (input.SupplierId is { } supplierId)
        {
            supplier = await suppliers.FindAsync(supplierId, cancellationToken);
            if (supplier is null) return Result<FabricStockLineResponse>.NotFound(StockMessages.SupplierNotFound);
        }

        var line = await changes.LineAsync(fabric.Value, input.Color, cancellationToken);
        if (line.Id != 0 && line.Version != input.Version) return Result<FabricStockLineResponse>.Conflict(StockMessages.Stale);
        var kind = Enum.Parse<FabricMovementKind>(input.Kind);
        if (kind == FabricMovementKind.Opening && line.HasMovements)
            return Result<FabricStockLineResponse>.Conflict(StockMessages.OpeningNotFirst);
        if (!FabricStock.IsValidMeters(line.Meters + input.Meters))
            return Result<FabricStockLineResponse>.Invalid(new ValidationError(nameof(input.Meters), StockMessages.TooMuch));

        line.Move(kind, input.Meters, new FabricMovementDetails(input.OperationDate,
            input.Note ?? (kind == FabricMovementKind.Opening ? "Початковий залишок" : null), supplier?.Id,
            supplier?.Name, input.Document), actorName, changes.Now);
        var saved = await changes.SaveAsync(cancellationToken);
        if (!saved.IsSuccess) return Result<FabricStockLineResponse>.Conflict([.. saved.Errors]);
        return Result.Success(await changes.CardAsync(command.Actor, line, cancellationToken));
    }
}

/// <summary>«Коригування»: the counted metres become the balance; the difference is a movement with its reason.</summary>
public sealed record CorrectFabricStockCommand(ProductionActor Actor, int StockId, CorrectFabricStockInput Input)
    : ICommand<Result<FabricStockLineResponse>>;

public sealed class CorrectFabricStockCommandValidator : AbstractValidator<CorrectFabricStockCommand>
{
    public CorrectFabricStockCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.StockId).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.Version).GreaterThan(0);
            RuleFor(x => x.Input.CountedMeters).Must(x => x >= 0 && FabricStock.IsValidMeters(x))
                .WithMessage("Пораховано — число від 0 з двома знаками після коми.");
            RuleFor(x => x.Input.OperationDate).NotEmpty().WithMessage("Вкажіть дату.");
            RuleFor(x => x.Input.Note).Reason();
        });
    }
}

public sealed class CorrectFabricStockCommandHandler(IProductionDbContext db, StockChanges changes)
    : ICommandHandler<CorrectFabricStockCommand, Result<FabricStockLineResponse>>
{
    public async ValueTask<Result<FabricStockLineResponse>> Handle(CorrectFabricStockCommand command,
        CancellationToken cancellationToken)
    {
        var input = command.Input;
        if (!command.Actor.CanMoveStock) return Result<FabricStockLineResponse>.Forbidden(StockMessages.CannotMove);
        var actorName = await changes.ActorNameAsync(command.Actor, cancellationToken);
        if (actorName is null) return Result<FabricStockLineResponse>.Forbidden(StockMessages.CannotMove);
        var line = await db.FabricStocks.SingleOrDefaultAsync(x => x.Id == command.StockId, cancellationToken);
        if (line is null) return Result<FabricStockLineResponse>.NotFound(StockMessages.LineNotFound);
        if (line.Version != input.Version) return Result<FabricStockLineResponse>.Conflict(StockMessages.Stale);
        if (line.Meters == input.CountedMeters)
            return Result<FabricStockLineResponse>.Invalid(new ValidationError(nameof(input.CountedMeters),
                StockMessages.NothingToCorrect));

        line.Correct(input.CountedMeters, new FabricMovementDetails(input.OperationDate, input.Note), actorName,
            changes.Now);
        var saved = await changes.SaveAsync(cancellationToken);
        if (!saved.IsSuccess) return Result<FabricStockLineResponse>.Conflict([.. saved.Errors]);
        return Result.Success(await changes.CardAsync(command.Actor, line, cancellationToken));
    }
}

/// <summary>«Сторно» of an opening, receipt or correction; issues are reversed on the order.</summary>
public sealed record ReverseFabricMovementCommand(ProductionActor Actor, int StockId, int MovementId,
    ReverseFabricMovementInput Input) : ICommand<Result<FabricStockLineResponse>>;

public sealed class ReverseFabricMovementCommandValidator : AbstractValidator<ReverseFabricMovementCommand>
{
    public ReverseFabricMovementCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.StockId).GreaterThan(0);
        RuleFor(x => x.MovementId).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.Version).GreaterThan(0);
            RuleFor(x => x.Input.Note).Reason();
        });
    }
}

public sealed class ReverseFabricMovementCommandHandler(IProductionDbContext db, StockChanges changes)
    : ICommandHandler<ReverseFabricMovementCommand, Result<FabricStockLineResponse>>
{
    public async ValueTask<Result<FabricStockLineResponse>> Handle(ReverseFabricMovementCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.Actor.CanMoveStock) return Result<FabricStockLineResponse>.Forbidden(StockMessages.CannotMove);
        var actorName = await changes.ActorNameAsync(command.Actor, cancellationToken);
        if (actorName is null) return Result<FabricStockLineResponse>.Forbidden(StockMessages.CannotMove);
        var line = await db.FabricStocks.SingleOrDefaultAsync(x => x.Id == command.StockId, cancellationToken);
        if (line is null) return Result<FabricStockLineResponse>.NotFound(StockMessages.LineNotFound);
        var movement = await db.FabricMovements.SingleOrDefaultAsync(
            x => x.Id == command.MovementId && x.StockId == line.Id, cancellationToken);
        if (movement is null) return Result<FabricStockLineResponse>.NotFound(StockMessages.MovementNotFound);
        if (line.Version != command.Input.Version) return Result<FabricStockLineResponse>.Conflict(StockMessages.Stale);
        if (movement.IsReversed) return Result<FabricStockLineResponse>.Conflict(StockMessages.AlreadyReversed);
        if (movement.Kind is not (FabricMovementKind.Opening or FabricMovementKind.Receipt or FabricMovementKind.Adjustment))
            return Result<FabricStockLineResponse>.Conflict(StockMessages.CannotReverse);

        line.Reverse(movement, new FabricMovementDetails(DateOnly.FromDateTime(changes.Now.UtcDateTime),
            command.Input.Note), actorName, changes.Now);
        var saved = await changes.SaveAsync(cancellationToken);
        if (!saved.IsSuccess) return Result<FabricStockLineResponse>.Conflict([.. saved.Errors]);
        return Result.Success(await changes.CardAsync(command.Actor, line, cancellationToken));
    }
}
