using Microsoft.EntityFrameworkCore;
using Production.Application.Abstractions;
using UnitTests.Production;
using Warehouse.Application.Abstractions;
using static UnitTests.Warehouse.WarehouseStockHandlerTests;
using Warehouse.Application.Features.Stock.Commands.IssueFabric;
using Warehouse.Application.Features.Stock.Queries.GetFabricBooking;
using Warehouse.Application.Features.Stock.Queries.GetFabricConsumption;
using Warehouse.Application.Features.Stock.Queries.GetFabricMovements;
using Warehouse.Application.Features.Stock.Queries.GetFabricPlanFact;
using Warehouse.Application.Features.Stock.Queries.GetFabricValuation;
using Warehouse.Application.Features.Stock.Queries.GetHardwareOrder;
using Warehouse.Application.Features.Stock.Shared;
using Production.Application.Abstractions.Sales;
using Warehouse.Application.Features.Stock.Queries.GetFabricMovements.Results;
using Warehouse.Application.Features.Stock.Queries.GetFabricPlanFact.Responses;
using Warehouse.Application.Features.Stock.Queries.GetFabricValuation.Results;
using Warehouse.Application.Features.Stock.Queries.GetHardwareOrder.Results;

namespace UnitTests.Warehouse;

/// <summary>The four reports of «Склад тканин» and «На 1 виріб» (WH-005).</summary>
public sealed class WarehouseReportTests
{
    private static readonly WarehouseActor Chief = new(4, false, false, true);
    private static readonly WarehouseActor Owner = new(1, true, false, false);
    private static readonly global::Production.Application.Features.Orders.Shared.Access.ProductionActor OrderManager = new(2, false, true, false);

    [Test]
    public async Task Plan_and_fact_compares_main_fabric_of_items_issued_for_all_products()
    {
        using var workshop = Workshop();
        await Receive(workshop, Chief, "Opening", 20m);
        await Receive(workshop, Chief, "Opening", 5m, "Червоний");
        var first = await Order(workshop);
        await IssueCut(workshop, Chief, first.OrderId, first.ItemId, first.Version, 3,
            [L("темно-сірий", "Темно-сірий", 6m), L("червоний", "Червоний", 0.9m)]);

        var report = await PlanFact(workshop);
        var movements = await Movements(workshop, Chief, FabricMovementFilter.Issue);

        var model = report.Models.Single();
        var item = model.Items.Single();
        Assert.Multiple(() =>
        {
            Assert.That((model.ModelName, model.ModelCode), Is.EqualTo(("Куртка «Віол»", "MOD-052")));
            Assert.That((item.Plan, item.Fact, item.Difference, item.Result), Is.EqualTo((7.5m, 6.9m, -0.6m, "Economy")));
            Assert.That(model.Summary.Orders, Is.EqualTo(1));
            Assert.That(model.Summary.Products, Is.EqualTo(3), "an item counts once, not once per colour");
            Assert.That((model.Summary.PlanPerProduct, model.Summary.FactPerProduct, model.Summary.DifferencePerProduct),
                Is.EqualTo((2.5m, 2.3m, -0.2m)));
            Assert.That(item.Materials.Select(x => (x.Layer, x.PlanColor, x.Fact)),
                Is.EqualTo(new[] { ("Upper", "Темно-сірий", (decimal?)6m), ("Upper", "Червоний", (decimal?)0.9m) }));
            Assert.That(movements.Rows.Single(x => x.Color == "Темно-сірий").PerProduct, Is.EqualTo(2m),
                "6 m for 3 products");
        });
    }

    [Test]
    public async Task Plan_and_fact_keeps_the_items_whose_last_issue_is_in_the_period()
    {
        using var workshop = Workshop();
        await Receive(workshop, Chief, "Opening", 20m);
        await Receive(workshop, Chief, "Opening", 5m, "Червоний");
        var first = await Order(workshop);
        await IssueCut(workshop, Chief, first.OrderId, first.ItemId, first.Version, 3,
            [L("темно-сірий", "Темно-сірий", 6m), L("червоний", "Червоний", 0.9m)]);

        var issuedOn = (await PlanFact(workshop)).Models.Single().Items.Single().IssuedOn;
        var inside = await PlanFact(workshop, from: new DateOnly(2026, 11, 1), to: new DateOnly(2026, 11, 30));
        var after = await PlanFact(workshop, from: new DateOnly(2026, 11, 5));

        Assert.Multiple(() =>
        {
            Assert.That(issuedOn, Is.EqualTo(new DateOnly(2026, 11, 4)), "WH-010: the day of the last issue");
            Assert.That(inside.Models, Has.Count.EqualTo(1));
            Assert.That(after.Models, Is.Empty);
            Assert.That(after.Total.Orders, Is.Zero);
        });
    }

    [Test]
    public async Task Consumption_splits_what_went_to_tasks_from_one_off_issues()
    {
        using var workshop = Workshop();
        await Receive(workshop, Chief, "Opening", 20m);
        await Receive(workshop, Chief, "Receipt", 10m, supplier: "Тексика");
        await Receive(workshop, Chief, "Opening", 5m, "Червоний");
        var first = await Order(workshop);
        await IssueCut(workshop, Chief, first.OrderId, first.ItemId, first.Version, 3,
            [L("темно-сірий", "Темно-сірий", 6m), L("червоний", "Червоний", 0.9m)]);
        await new IssueFabricHandler(workshop.StockChanges(), workshop.Pairs(), workshop.Operations())
            .Handle(new(Chief, new(Guid.NewGuid(), "FAB-023", "Темно-сірий", null, 1.5m, new DateOnly(2026, 11, 4),
                "Зразок", null)), CancellationToken.None);
        workshop.Db.ChangeTracker.Clear();

        var report = (await new GetFabricConsumptionHandler(workshop.Db, workshop.Reference)
            .Handle(new(Chief, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30)), CancellationToken.None)).Value;

        var dark = report.Rows.Single(x => x.Color == "Темно-сірий");
        Assert.Multiple(() =>
        {
            Assert.That((dark.Received, dark.Issued, dark.IssuedToTasks, dark.OneOff),
                Is.EqualTo((10m, 7.5m, 6m, 1.5m)), "WH-010: «На завдання» and «Разова» apart");
            Assert.That((report.Issued, report.IssuedToTasks, report.OneOff, report.Corrected),
                Is.EqualTo((8.4m, 6.9m, 1.5m, 0m)));
        });
    }

    [Test]
    public async Task One_item_that_cannot_be_compared_makes_the_difference_unknown()
    {
        using var workshop = Workshop();
        await Receive(workshop, Chief, "Opening", 30m);
        await Receive(workshop, Chief, "Opening", 5m, "Червоний");
        var complete = await Order(workshop);
        var unchecked_ = await Order(workshop);
        var partial = await Order(workshop);
        await IssueCut(workshop, Chief, complete.OrderId, complete.ItemId, complete.Version, 3,
            [L("темно-сірий", "Темно-сірий", 6m), L("червоний", "Червоний", 0.9m)]);
        await IssueCut(workshop, Chief, unchecked_.OrderId, unchecked_.ItemId, unchecked_.Version, 3,
            [L("темно-сірий", "Темно-сірий", 6.6m)]);
        await IssueCut(workshop, Chief, partial.OrderId, partial.ItemId, partial.Version, 1,
            [L("темно-сірий", "Темно-сірий", 2m)]);

        var report = await PlanFact(workshop);
        var filtered = await PlanFact(workshop, client: "немає такого");

        var summary = report.Models.Single().Summary;
        var item = report.Models.Single().Items.Single(x => x.ItemId == unchecked_.ItemId);
        Assert.Multiple(() =>
        {
            Assert.That(report.InProgressItems, Is.EqualTo(1), "a partial issue is not counted as economy");
            Assert.That((summary.Orders, summary.Products, summary.UncomparableItems), Is.EqualTo((2, 6, 1)));
            Assert.That((summary.Difference, summary.DifferencePerProduct, summary.Result),
                Is.EqualTo(((decimal?)null, (decimal?)null, "NeedsCheck")));
            Assert.That((item.Result, item.Reason, item.Difference),
                Is.EqualTo(("NeedsCheck", FabricPlanFact.NotIssued, (decimal?)null)));
            Assert.That(item.Materials.Single(x => x.PlanColor == "Червоний").Fact, Is.Null, "«не зафіксовано»");
            Assert.That(report.Total.Result, Is.EqualTo("NeedsCheck"));
            Assert.That(filtered.Models, Is.Empty);
        });
    }

    [Test]
    public void Per_product_figures_are_weighted_by_products()
    {
        FabricPlanFactEntry Entry(int orderId, int products, decimal plan, decimal fact) => new("Модель", null, false,
            new FabricPlanFactItemResponse(orderId, orderId, "Клієнт", orderId, products, plan, fact, fact - plan,
                FabricPlanFact.Outcome(fact - plan), null, []));

        var summary = FabricPlanFact.Summary([Entry(1, 10, 30m, 28m), Entry(2, 20, 80m, 80m)]);

        Assert.Multiple(() =>
        {
            Assert.That(summary.PlanPerProduct, Is.EqualTo(3.667m), "110 / 30, not the mean of 3 and 4");
            Assert.That(summary.FactPerProduct, Is.EqualTo(3.6m));
            Assert.That((summary.Difference, summary.Result), Is.EqualTo(((decimal?)-2m, "Economy")));
        });
    }

    [Test]
    public void Booking_hands_the_stock_to_the_queue_and_orders_the_rest()
    {
        var first = FabricBooking.Allocate(100m, [70m, 50m]);
        var negative = FabricBooking.Allocate(-10m, [20m]);

        Assert.Multiple(() =>
        {
            Assert.That(first.Items.Select(x => x.Reserved), Is.EqualTo(new[] { 70m, 30m }));
            Assert.That((first.Reserved, first.Free, first.ToOrder), Is.EqualTo((100m, 0m, 20m)));
            Assert.That((negative.Reserved, negative.Free, negative.ToOrder, negative.Shortage),
                Is.EqualTo((0m, 0m, 30m, 10m)), "the shortage already there is ordered too");
        });
    }

    [Test]
    public async Task Booking_reads_open_plans_in_order_and_books_nothing()
    {
        using var workshop = Workshop();
        await Receive(workshop, Chief, "Opening", 8m);
        var first = await Order(workshop);
        var second = await Order(workshop);
        await IssueCut(workshop, Chief, first.OrderId, first.ItemId, first.Version, 1,
            [L("темно-сірий", "Темно-сірий", 2m)]);

        var report = (await new GetFabricBookingHandler(workshop.Booking())
            .Handle(new(Chief), CancellationToken.None)).Value;

        var dark = report.Pairs.Single(x => x.Color == "Темно-сірий");
        var red = report.Pairs.Single(x => x.Color == "Червоний");
        Assert.Multiple(() =>
        {
            Assert.That(dark.Items.Select(x => x.OrderId), Is.EqualTo(new[] { first.OrderId, second.OrderId }));
            Assert.That(dark.Items.Select(x => (x.Plan, x.Covered, x.Remaining, x.Reserved, x.ToOrder)),
                Is.EqualTo(new[] { (6.6m, 2.2m, 4.4m, 4.4m, 0m), (6.6m, 0m, 6.6m, 1.6m, 5m) }));
            Assert.That((dark.Stock, dark.Need, dark.Reserved, dark.Free, dark.ToOrder),
                Is.EqualTo((6m, 11m, 6m, 0m, 5m)));
            Assert.That((red.Stock, red.Need, red.ToOrder), Is.EqualTo((0m, 1.8m, 1.8m)));
            Assert.That(report.Pairs[0].ToOrder, Is.EqualTo(5m), "sorted by what to order");
            Assert.That(report.ToOrder, Is.EqualTo(6.8m));
            Assert.That(report.Unknown, Is.Empty);
            Assert.That(dark.Supplier, Is.EqualTo("Тексика"), "WH-009: the supplier of the canonical offer");
            Assert.That(report.CanReceive, Is.True, "the stock keeper may record «Прихід» from a row");
        });
    }

    [Test]
    public async Task Booking_offers_a_receipt_only_to_the_stock_keepers()
    {
        using var workshop = Workshop();
        var manager = new WarehouseActor(2, false, true, false);

        var report = (await new GetFabricBookingHandler(workshop.Booking())
            .Handle(new(manager), CancellationToken.None)).Value;

        Assert.That(report.CanReceive, Is.False);
    }

    [Test]
    public void Need_by_is_the_nearest_date_of_the_items_still_short_or_else_of_those_that_need_it()
    {
        static FabricBooking.Share S(decimal remaining, decimal missing) => new(remaining, remaining - missing, missing);
        var nov20 = new DateOnly(2026, 11, 20);
        var nov14 = new DateOnly(2026, 11, 14);

        Assert.Multiple(() =>
        {
            Assert.That(FabricBooking.NeedBy([(nov14, S(4m, 0m)), (nov20, S(6m, 5m)), (null, S(1m, 1m))]),
                Is.EqualTo(nov20), "the covered item does not set the date");
            Assert.That(FabricBooking.NeedBy([(nov20, S(4m, 0m)), (nov14, S(2m, 0m))]), Is.EqualTo(nov14));
            Assert.That(FabricBooking.NeedBy([(null, S(4m, 4m))]), Is.Null);
        });
    }

    [Test]
    public void Hardware_need_keeps_the_nearest_due_date_of_its_tasks()
    {
        static WarehouseAssemblyItem Item(int id, DateOnly? due) => new(id, id, "Клієнт", DateTimeOffset.UnixEpoch,
            id * 10, "Куртка", "MOD-1", 2, [new("Accessory", "FUR-003", "Кнопка", 3m, "Piece", null)], due);

        var (needs, _) = HardwareNeeds.Calculate([
            Item(1, new DateOnly(2026, 11, 25)), Item(2, new DateOnly(2026, 11, 14)), Item(3, null)
        ]);

        Assert.Multiple(() =>
        {
            Assert.That(needs.Single().DueDate, Is.EqualTo(new DateOnly(2026, 11, 14)));
            Assert.That(needs.Single().Orders.Select(x => x.DueDate),
                Is.EqualTo(new DateOnly?[] { new(2026, 11, 25), new(2026, 11, 14), null }));
        });
    }

    [Test]
    public async Task Stock_shows_what_plans_book_of_each_pair_and_its_latest_movement()
    {
        using var workshop = Workshop();
        await Receive(workshop, Chief, "Opening", 8m);
        var first = await Order(workshop);
        var second = await Order(workshop);
        await IssueCut(workshop, Chief, first.OrderId, first.ItemId, first.Version, 1,
            [L("темно-сірий", "Темно-сірий", 2m)]);

        var list = await List(workshop, Chief);
        var dark = Pair(list, "Темно-сірий");
        var fabric = list.Fabrics.Single(x => x.FabricCode == "FAB-023");

        Assert.Multiple(() =>
        {
            Assert.That((dark.Meters, dark.Booked, dark.Free, dark.ToOrder), Is.EqualTo((6m, 11m, -5m, 5m)),
                "WH-008: free may go below zero, to order as «Що замовити»");
            Assert.That(dark.Bookings!.Select(x => (x.OrderId, x.Need, x.ToOrder)),
                Is.EqualTo(new[] { (first.OrderId, 4.4m, 0m), (second.OrderId, 6.6m, 5m) }));
            Assert.That(dark.LastMovement, Has.Property("Kind").EqualTo("Issue").And.Property("Delta").EqualTo(-2m));
            Assert.That(dark.LastMovement!.OrderNumber, Is.Not.Null);
            Assert.That((fabric.Booked, fabric.Free), Is.EqualTo((11m, -5m)));
            Assert.That(list.Short, Is.EqualTo(1), "a pair without a stock line is not on «Залишки»");
        });
    }

    [Test]
    public async Task Valuation_takes_the_lowest_price_of_the_group_and_lists_shortages_apart()
    {
        using var workshop = Workshop();
        workshop.Reference.Fabrics.Add(new(9, "FAB-009", "Фліс", null, "100% поліестер", "200", "Тексика"));
        await Receive(workshop, Chief, "Opening", 10.5m);
        await Receive(workshop, Chief, "Opening", 3.33m, "Жовтий", fabricId: 5);
        await Receive(workshop, Chief, "Opening", 4m, "Жовтий", fabricId: 9);
        await Receive(workshop, Chief, "Opening", 1m, "Червоний");
        await new IssueFabricHandler(workshop.StockChanges(), workshop.Pairs(), workshop.Operations())
            .Handle(new(Chief, new(Guid.NewGuid(), "FAB-023", "Червоний", null, 3m, new DateOnly(2026, 11, 4), "Цех", null)),
                CancellationToken.None);

        var report = (await new GetFabricValuationHandler(workshop.Db, workshop.Reference, workshop.Workshop.Time)
            .Handle(new(Owner), CancellationToken.None)).Value;

        var greta = report.Fabrics.Single(x => x.FabricCode == "FAB-023");
        Assert.Multiple(() =>
        {
            Assert.That((greta.Price, greta.PriceFabricCode, greta.Value), Is.EqualTo(((decimal?)110m, "FAB-067", 1155m)),
                "FAB-067 is the same fabric and is cheaper");
            Assert.That(report.Fabrics.Single(x => x.FabricCode == "FAB-005").Value, Is.EqualTo(299.7m));
            Assert.That((report.Value, report.PositiveMeters), Is.EqualTo((1454.7m, 17.83m)));
            Assert.That((report.IsPartial, report.UnpricedPairs, report.UnpricedMeters), Is.EqualTo((true, 1, 4m)));
            Assert.That(report.Shortages.Single(), Has.Property("Meters").EqualTo(2m).And.Property("Value").EqualTo(220m));
            Assert.That(report.ShortageValue, Is.EqualTo(220m), "a shortage does not lower the value");
        });
    }

    [Test]
    public void Value_is_kopecks_of_hundredths_times_price()
    {
        Assert.Multiple(() =>
        {
            Assert.That(FabricValuation.Value(0.01m, 0.5m), Is.EqualTo(0.01m), "0,5 kopeck rounds up");
            Assert.That(FabricValuation.Value(12.34m, 105.55m), Is.EqualTo(1302.49m));
        });
    }

    [Test]
    public async Task Hardware_counts_physical_accessories_of_open_orders()
    {
        using var workshop = Workshop();
        workshop.Workshop.Models.Compositions.Add(new ProductionModelComposition(5, "MOD-052",
        [
            new("Fabric", "A-77", "FAB-023", "Грета", 2.2m, "Metre", null, 77),
            new("Accessory", "A-77", "FUR-003", "Кнопка", 2m, "Piece", null, 77),
            new("Accessory", "A-78", "FUR-003", "Кнопка", 5m, "Piece", null, 78),
            new("Accessory", null, "FUR-005", "Блискавка  тракторна", 1m, "Piece", "65 см", null),
            new("Accessory", null, "FUR-007", null, 0.5m, "Metre", null, null),
            new("Accessory", null, "FUR-021", "Стрічка текстильна", 1m, "Metre", null, null),
            new("OtherCost", null, "FUR-032", "Інші витрати", 1m, "Service", null, null),
            new("Accessory", null, "FUR-040", "Нова позиція", 1m, "Piece", null, null),
            new("Accessory", null, "FUR-012", "Люверс", 0m, "Piece", null, null)
        ]));
        await Order(workshop);
        await Order(workshop);

        var report = (await new GetHardwareOrderHandler(workshop.Production, workshop.Workshop.Time)
            .Handle(new(Chief), CancellationToken.None)).Value;

        Assert.Multiple(() =>
        {
            Assert.That((report.Orders, report.Items), Is.EqualTo((2, 2)));
            Assert.That(report.Needs.Select(x => (x.Code, x.Name, x.Quantity)), Is.EqualTo(new[]
            {
                ("FUR-003", "Кнопка", 12m), ("FUR-005", "Блискавка тракторна", 6m), ("FUR-007", "FUR-007", 3m)
            }), "the variant of the item only; textile and the service are not accessories");
            Assert.That(report.Needs.Single(x => x.Code == "FUR-007").NameChecked, Is.False);
            Assert.That(report.Needs.Single(x => x.Code == "FUR-005").Orders.Select(x => (x.Norm, x.Products, x.Quantity)),
                Is.EqualTo(new[] { (1m, 3, 3m), (1m, 3, 3m) }));
            Assert.That(report.Incomplete.Select(x => (x.Code, x.Reason)).Distinct(), Is.EquivalentTo(new[]
            {
                ("FUR-040", HardwareNeeds.UnknownCode), ("FUR-012", HardwareNeeds.NoNorm)
            }));
            Assert.That(HardwareNeeds.IsHardware("FUR-039"), Is.True);
            Assert.That(HardwareNeeds.IsHardware("FUR-026"), Is.False);
            Assert.That(HardwareNeeds.IsHardware("FUR-034"), Is.Null);
        });
    }

    private static async Task<FabricPlanFactResponse> PlanFact(WarehouseTestHarness workshop, string? client = null,
        DateOnly? from = null, DateOnly? to = null)
    {
        workshop.Db.ChangeTracker.Clear();
        return (await new GetFabricPlanFactHandler(workshop.Db, workshop.Production, workshop.Reference,
                workshop.Plans())
            .Handle(new(Chief, null, client, null, from, to), CancellationToken.None)).Value;
    }

    private static async Task<(int OrderId, int ItemId, int Version)> Order(WarehouseTestHarness workshop)
    {
        var order = (await ProductionOrderHandlerTests.Create(workshop.Workshop, OrderManager,
            ProductionOrderHandlerTests.Input())).Value;
        return (order.Id, order.Items[0].Id, order.Version);
    }
}
