using Production.Application.Abstractions;
using Production.Domain.Work;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Payroll.Shared;
using Production.Application.Features.Work.Shared;
using BuildingBlocks.Application;

namespace Production.Application.Features.Work.Commands.VoidProductionWork;

public sealed class VoidProductionWorkHandler(IProductionDbContext db, IProductionStaffReader staff,
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
        if (await PayrollMonths.IsClosedAsync(db, work.WorkDate, cancellationToken))
            return Result<ProductionItemWorkResponse>.Conflict(PayrollMessages.Closed);
        var actorName = await OrderActors.NameAsync(staff, command.Actor.UserId, cancellationToken);
        if (actorName is null) return Result<ProductionItemWorkResponse>.Forbidden(WorkMessages.CannotRecord);
        var voided = work.Void(command.Input.Reason, actorName, clock.GetUtcNow());
        if (voided.IsFailure) return voided.Error!.ToResult<ProductionItemWorkResponse>();
        await db.SaveChangesAsync(cancellationToken);
        var item = order.Items.First(x => x.Id == work.ItemId);
        return Result.Success(await cards.BuildAsync(order, item, command.Actor, cancellationToken));
    }
}
