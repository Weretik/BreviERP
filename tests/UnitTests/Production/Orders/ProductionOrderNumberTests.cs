using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Ardalis.Result;
using Production.Application.Features.Orders.Commands.CopyProductionOrder;
using Production.Application.Features.Orders.Commands.CreateProductionOrder;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Responses;
using Production.Infrastructure.Persistence;
using static UnitTests.Production.Orders.ProductionOrderHandlerTests;

namespace UnitTests.Production.Orders;

/// <summary>Order numbers are «MAX + 1» guarded by the unique index: a create that loses the race takes the next one.</summary>
public sealed class ProductionOrderNumberTests
{
    private static readonly ProductionActor Manager = new(2, false, true, false);

    [Test]
    public async Task Two_orders_created_at_once_get_two_numbers()
    {
        using var workshop = ProductionOrderHandlerTests.Workshop();
        var other = new CreateFirst(workshop, Input());
        await using var press = workshop.OtherRequest(other);

        var result = await CreateIn(workshop, press, Input());

        Assert.That(result.IsSuccess, Is.True, string.Join("; ", result.Errors));
        Assert.Multiple(() =>
        {
            Assert.That(other.Created, Is.True);
            Assert.That(workshop.Db.Orders.AsNoTracking().OrderBy(x => x.Number).Select(x => x.Number),
                Is.EqualTo(new[] { 1, 2 }));
            Assert.That(result.Value.Number, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task The_same_press_twice_at_once_is_one_order()
    {
        using var workshop = ProductionOrderHandlerTests.Workshop();
        var input = Input();
        var other = new CreateFirst(workshop, input);
        await using var press = workshop.OtherRequest(other);

        var result = await CreateIn(workshop, press, input);

        Assert.That(result.IsSuccess, Is.True, string.Join("; ", result.Errors));
        var saved = await workshop.Db.Orders.AsNoTracking().SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(result.Value.Id, Is.EqualTo(saved.Id.Value));
            Assert.That(result.Value.Items, Has.Count.EqualTo(1), "the card of the saved order");
        });
    }

    [Test]
    public async Task A_copy_made_while_another_order_is_created_takes_the_next_number()
    {
        using var workshop = ProductionOrderHandlerTests.Workshop();
        var source = (await Create(workshop, Manager, Input())).Value;
        var other = new CreateFirst(workshop, Input());
        await using var press = workshop.OtherRequest(other);

        var copy = await new CopyProductionOrderHandler(press, workshop.Staff, workshop.Cards(), workshop.Time)
            .Handle(new(Manager, source.Id, new(Guid.NewGuid())), CancellationToken.None);

        Assert.That(copy.IsSuccess, Is.True, string.Join("; ", copy.Errors));
        Assert.That(copy.Value.Number, Is.EqualTo(3));
    }

    private static Task<Result<ProductionOrderResponse>> CreateIn(ProductionTestHarness workshop,
        ProductionDbContext db, CreateProductionOrderInput input) =>
        new CreateProductionOrderHandler(db, workshop.Quotes, workshop.Staff, workshop.Resolver(), workshop.Cards(),
            workshop.EmbroideryLinks(), workshop.Colors(), workshop.Time).Handle(new(Manager, input), CancellationToken.None)
            .AsTask();

    /// <summary>Another manager's create saves first, between the press's number and its save.</summary>
    private sealed class CreateFirst(ProductionTestHarness workshop, CreateProductionOrderInput input) : SaveChangesInterceptor
    {
        public bool Created { get; private set; }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (Created) return result;
            Created = true;
            Assert.That((await Create(workshop, Manager, input)).IsSuccess, Is.True);
            return result;
        }
    }
}
