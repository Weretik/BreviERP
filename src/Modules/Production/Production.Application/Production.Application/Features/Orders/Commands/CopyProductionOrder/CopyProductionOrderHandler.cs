using BuildingBlocks.Application.Idempotency;
using BuildingBlocks.Domain.Identifiers.Orders;
using BuildingBlocks.Domain.Identifiers.People;
using Production.Domain.Orders.ValueObjects;
using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Orders.Shared.Responses;
using Production.Application.Features.Orders.Shared.Rules;
using BuildingBlocks.Application;

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
        var request = RequestKeys.Of(command.Input.RequestId, command with { Input = command.Input with { RequestId = Guid.Empty } })!.Value;
        var existing = await cards.Orders.AsNoTracking()
            .FirstOrDefaultAsync(x => x.CreateRequestId == request.Id, cancellationToken);
        if (existing is not null)
            return request.Matches(existing.CreateRequestHash)
                ? Result.Success(await cards.BuildAsync(existing, actor, cancellationToken))
                : RequestKeys.Conflict<ProductionOrderResponse>();

        var source = await db.Orders.AsNoTracking().Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == new ProductionOrderId(command.Id) && x.DeletedAt == null, cancellationToken);
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
            new OrderOrigin(source.QuoteId, source.QuoteNumber, source.CustomerId, new UserId(person.UserId), person.Name,
                request, CopiedFromOrderId: source.Id),
            new OrderHeader(source.ClientName, today, source.Comment), source.PriceTier, items, person.Name, now);
        if (created.IsFailure) return created.Error!.ToResult<ProductionOrderResponse>();
        var copy = created.Value;
        copy.Record("Створено", $"Копія завдання № {source.Number}", person.Name, now);
        // No delivery row yet: a missing row reads as «Не надіслано», and «Надіслати в цех» creates it.
        var saved = await OrderNumbers.SaveNewAsync(db, copy, cancellationToken);
        return Result.Success(await cards.BuildAsync(saved, actor, cancellationToken));
    }
}
