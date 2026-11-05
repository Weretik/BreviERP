using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Application.Orders;
using Production.Application.Orders.Access;
using Production.Application.Orders.Validation;
using Production.Domain.Work;

namespace Production.Application.Work.RecordWork;

/// <summary>
/// «Додати роботу»: a worker sewed or cut pieces of an item (PRD-D34, D35). Within the plan the item's quantity is the
/// limit per kind; more goes as «понад план» with a reason.
/// </summary>
public sealed record CreateProductionWorkCommand(ProductionActor Actor, int OrderId, int ItemId,
    CreateProductionWorkInput Input) : ICommand<Result<ProductionItemWorkResponse>>;

public sealed class CreateProductionWorkCommandValidator : AbstractValidator<CreateProductionWorkCommand>
{
    public CreateProductionWorkCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.EmployeeId).GreaterThan(0).WithMessage("Оберіть працівника.");
            RuleFor(x => x.Input.Kind).Must(x => Enum.TryParse<ProductionWorkKind>(x, out var k) && Enum.IsDefined(k))
                .WithMessage("Оберіть пошив або крій.");
            RuleFor(x => x.Input.Quantity).InclusiveBetween(1, ProductionWork.MaxQuantity)
                .WithMessage("Кількість — ціле число від 1.");
            RuleFor(x => x.Input.WorkDate).NotEmpty().WithMessage("Вкажіть дату роботи.");
            RuleFor(x => x.Input.Reason).Must(OverPlanReasons.IsKnown).WithMessage(WorkMessages.Reason)
                .When(x => x.Input.OverPlan);
            RuleFor(x => x.Input.Comment).MaximumLength(ProductionWork.CommentMaxLength);
        });
    }
}

public sealed class CreateProductionWorkCommandHandler(IProductionDbContext db, IProductionStaffReader staff,
    WorkCards cards, TimeProvider clock) : ICommandHandler<CreateProductionWorkCommand, Result<ProductionItemWorkResponse>>
{
    public async ValueTask<Result<ProductionItemWorkResponse>> Handle(CreateProductionWorkCommand command,
        CancellationToken cancellationToken)
    {
        var input = command.Input;
        var order = await db.Orders.Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == command.OrderId && x.DeletedAt == null, cancellationToken);
        var item = order?.Items.FirstOrDefault(x => x.Id == command.ItemId);
        if (order is null || item is null) return Result<ProductionItemWorkResponse>.NotFound(OrderMessages.NotFound);
        if (!command.Actor.CanRecordWork(order)) return Result<ProductionItemWorkResponse>.Forbidden(WorkMessages.CannotRecord);
        if (item.SewingRate is not > 0) return Invalid(nameof(input.Kind), WorkMessages.NoRate);
        if (input.WorkDate > WorkshopDay.Today(clock)) return Invalid(nameof(input.WorkDate), WorkMessages.FutureDate);
        var employee = await db.Employees.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == input.EmployeeId && x.IsActive, cancellationToken);
        if (employee is null) return Invalid(nameof(input.EmployeeId), WorkMessages.EmployeeNotFound);
        var kind = Enum.Parse<ProductionWorkKind>(input.Kind);
        if (!input.OverPlan)
        {
            var done = (await cards.DoneAsync([item.Id], cancellationToken))[item.Id];
            var remaining = Math.Max(0, item.Quantity - (kind == ProductionWorkKind.Sewing ? done.Sewn : done.Cut));
            if (input.Quantity > remaining)
                return Result<ProductionItemWorkResponse>.Conflict(
                    WorkMessages.OverPlan(kind == ProductionWorkKind.Sewing ? "пошити" : "покроїти", remaining));
        }

        var actorName = await OrderActors.NameAsync(staff, command.Actor.UserId, cancellationToken);
        if (actorName is null) return Result<ProductionItemWorkResponse>.Forbidden(WorkMessages.CannotRecord);
        db.Work.Add(new ProductionWork(new WorkTarget(order.Id, item.Id, employee.Id, employee.FullName),
            new WorkFacts(kind, input.Quantity, input.WorkDate, input.OverPlan, input.Reason, input.Comment,
                item.SewingRate.Value), actorName, clock.GetUtcNow()));
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(await cards.BuildAsync(order, item, command.Actor, cancellationToken));
    }

    private static Result<ProductionItemWorkResponse> Invalid(string field, string message) =>
        Result<ProductionItemWorkResponse>.Invalid(new ValidationError($"Input.{field}", message));
}

/// <summary>«Сторно» of a record with a reason (PRD-D36); the record stays in the log, crossed out.</summary>
public sealed record VoidProductionWorkCommand(ProductionActor Actor, int WorkId, VoidProductionWorkInput Input)
    : ICommand<Result<ProductionItemWorkResponse>>;

public sealed class VoidProductionWorkCommandValidator : AbstractValidator<VoidProductionWorkCommand>
{
    public VoidProductionWorkCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.WorkId).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.Reason).NotEmpty().WithMessage("Вкажіть причину сторно.")
            .MaximumLength(ProductionWork.ReasonMaxLength).When(x => x.Input is not null);
    }
}

public sealed class VoidProductionWorkCommandHandler(IProductionDbContext db, IProductionStaffReader staff,
    WorkCards cards, TimeProvider clock) : ICommandHandler<VoidProductionWorkCommand, Result<ProductionItemWorkResponse>>
{
    public async ValueTask<Result<ProductionItemWorkResponse>> Handle(VoidProductionWorkCommand command,
        CancellationToken cancellationToken)
    {
        var work = await db.Work.SingleOrDefaultAsync(x => x.Id == command.WorkId, cancellationToken);
        if (work is null) return Result<ProductionItemWorkResponse>.NotFound(WorkMessages.WorkNotFound);
        var order = await db.Orders.Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == work.OrderId && x.DeletedAt == null, cancellationToken);
        if (order is null) return Result<ProductionItemWorkResponse>.NotFound(OrderMessages.NotFound);
        if (!command.Actor.CanRecordWork(order)) return Result<ProductionItemWorkResponse>.Forbidden(WorkMessages.CannotRecord);
        if (work.IsVoided) return Result<ProductionItemWorkResponse>.Conflict(WorkMessages.AlreadyVoided);
        var actorName = await OrderActors.NameAsync(staff, command.Actor.UserId, cancellationToken);
        if (actorName is null) return Result<ProductionItemWorkResponse>.Forbidden(WorkMessages.CannotRecord);
        work.Void(command.Input.Reason, actorName, clock.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        var item = order.Items.First(x => x.Id == work.ItemId);
        return Result.Success(await cards.BuildAsync(order, item, command.Actor, cancellationToken));
    }
}
