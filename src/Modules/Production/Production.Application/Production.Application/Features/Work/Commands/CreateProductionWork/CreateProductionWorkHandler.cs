using Production.Application.Abstractions;
using Production.Domain.Work;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Payroll.Shared;
using Production.Application.Features.Work.Shared;
using BuildingBlocks.Application;

namespace Production.Application.Features.Work.Commands.CreateProductionWork;

public sealed class CreateProductionWorkHandler(IProductionDbContext db, IProductionStaffReader staff,
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
        if (await PayrollMonths.IsClosedAsync(db, input.WorkDate, cancellationToken))
            return Result<ProductionItemWorkResponse>.Conflict(PayrollMessages.Closed);
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
        var work = ProductionWork.Create(new WorkTarget(order.Id, item.Id, employee.Id, employee.FullName),
            new WorkFacts(kind, input.Quantity, input.WorkDate, input.OverPlan, input.Reason, input.Comment,
                item.SewingRate.Value), actorName, clock.GetUtcNow());
        if (work.IsFailure) return work.Error!.ToResult<ProductionItemWorkResponse>();
        db.Work.Add(work.Value);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(await cards.BuildAsync(order, item, command.Actor, cancellationToken));
    }

    private static Result<ProductionItemWorkResponse> Invalid(string field, string message) =>
        Result<ProductionItemWorkResponse>.Invalid(new ValidationError($"Input.{field}", message));
}
