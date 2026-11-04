using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Application.Orders.Access;
using Production.Domain.Orders;

namespace Production.Application.Orders.CopyOrder;

/// <summary>
/// «Копіювати»: a new order with the same client, КП and items (with their sewing rate and КП price). It starts as
/// «Нове» today; the person who copies is the manager. A repeated request returns the same copy.
/// </summary>
public sealed record CopyProductionOrderCommand(ProductionActor Actor, int Id, CopyProductionOrderInput Input)
    : ICommand<Result<ProductionOrderResponse>>;

public sealed class CopyProductionOrderCommandValidator : AbstractValidator<CopyProductionOrderCommand>
{
    public CopyProductionOrderCommandValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.RequestId).NotEmpty().When(x => x.Input is not null);
    }
}

public sealed class CopyProductionOrderCommandHandler(IProductionDbContext db, IProductionStaffReader staff,
    TimeProvider clock)
    : ICommandHandler<CopyProductionOrderCommand, Result<ProductionOrderResponse>>
{
    public async ValueTask<Result<ProductionOrderResponse>> Handle(CopyProductionOrderCommand command,
        CancellationToken cancellationToken)
    {
        var actor = command.Actor;
        if (!actor.CanCreate) return Result<ProductionOrderResponse>.Forbidden(OrderMessages.CannotCreate);
        var existing = await db.Orders.AsNoTracking().Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.CreateRequestId == command.Input.RequestId, cancellationToken);
        if (existing is not null) return Result.Success(existing.ToResponse(actor));

        var source = await db.Orders.AsNoTracking().Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == command.Id && x.DeletedAt == null, cancellationToken);
        if (source is null) return Result<ProductionOrderResponse>.NotFound(OrderMessages.NotFound);
        var person = await staff.FindAsync(actor.UserId, cancellationToken);
        if (person is null) return Result<ProductionOrderResponse>.Forbidden(OrderMessages.ActorUnknown);

        var now = clock.GetUtcNow();
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        var items = source.Items.OrderBy(x => x.Position)
            .Select(x => new ProductionItem(x.Position, x.Source, x.Details, x.PlannedFabricMeters));
        var copy = new ProductionOrder(await OrderNumbers.NextAsync(db, cancellationToken),
            new OrderOrigin(source.QuoteId, source.QuoteNumber, source.CustomerId, person.UserId, person.Name,
                command.Input.RequestId, CopiedFromOrderId: source.Id),
            new OrderHeader(source.ClientName, today, source.Comment), source.PriceTier, items, person.Name, now);
        copy.Record("Створено", $"Копія завдання № {source.Number}", person.Name, now);
        db.Orders.Add(copy);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(copy.ToResponse(actor));
    }
}
