using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Orders.Commands.CopyProductionOrder;

public sealed class CopyProductionOrderHandler(IProductionDbContext db, IProductionStaffReader staff,
    OrderCards cards, TimeProvider clock)
    : ICommandHandler<CopyProductionOrderCommand, Result<ProductionOrderResponse>>
{
    public async ValueTask<Result<ProductionOrderResponse>> Handle(CopyProductionOrderCommand command,
        CancellationToken cancellationToken)
    {
        var actor = command.Actor;
        if (!actor.CanCreate) return Result<ProductionOrderResponse>.Forbidden(OrderMessages.CannotCreate);
        var existing = await cards.Orders.AsNoTracking()
            .FirstOrDefaultAsync(x => x.CreateRequestId == command.Input.RequestId, cancellationToken);
        if (existing is not null) return Result.Success(await cards.BuildAsync(existing, actor, cancellationToken));

        var source = await db.Orders.AsNoTracking().Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == command.Id && x.DeletedAt == null, cancellationToken);
        if (source is null) return Result<ProductionOrderResponse>.NotFound(OrderMessages.NotFound);
        var person = await staff.FindAsync(actor.UserId, cancellationToken);
        if (person is null) return Result<ProductionOrderResponse>.Forbidden(OrderMessages.ActorUnknown);

        var now = clock.GetUtcNow();
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        var items = new List<ProductionItem>(source.Items.Count);
        foreach (var item in source.Items.OrderBy(x => x.Position))
        {
            var copied = ProductionItem.Create(item.Position, item.Source, item.Details, item.PlannedFabricMeters);
            if (copied.IsFailure) return copied.Error!.ToResult<ProductionOrderResponse>();
            items.Add(copied.Value);
        }

        var created = ProductionOrder.Create(await OrderNumbers.NextAsync(db, cancellationToken),
            new OrderOrigin(source.QuoteId, source.QuoteNumber, source.CustomerId, person.UserId, person.Name,
                command.Input.RequestId, CopiedFromOrderId: source.Id),
            new OrderHeader(source.ClientName, today, source.Comment), source.PriceTier, items, person.Name, now);
        if (created.IsFailure) return created.Error!.ToResult<ProductionOrderResponse>();
        var copy = created.Value;
        copy.Record("Створено", $"Копія завдання № {source.Number}", person.Name, now);
        db.Orders.Add(copy);
        await db.SaveChangesAsync(cancellationToken);
        db.TelegramDeliveries.Add(new ProductionTelegramDelivery(copy.Id));
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(await cards.BuildAsync(copy, actor, cancellationToken));
    }
}
