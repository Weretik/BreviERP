using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Application.Orders.Access;
using Production.Application.Stock.GetStock;
using Production.Application.Stock.ImportStock;
using Production.Application.Stock.Issues;
using Production.Application.Stock.Movements;

namespace UnitTests.Production;

/// <summary>«Склад тканин»: movements, reservation by colour, issues and the transfer (PRD-004).</summary>
public sealed class ProductionStockHandlerTests
{
    private static readonly ProductionActor Admin = new(1, true, false, false);
    private static readonly ProductionActor Manager = new(2, false, true, false);
    private static readonly ProductionActor Chief = new(4, false, false, true);
    private static readonly DateOnly Day = new(2026, 11, 4);

    [Test]
    public async Task Receipt_correction_and_reversal_keep_before_change_after()
    {
        using var workshop = Workshop();
        var opened = await Receive(workshop, Chief, "Opening", 10m, null);
        var received = await Receive(workshop, Chief, "Receipt", 25.5m, opened.Value.Line.Version, supplierId: 7);
        var stale = await Receive(workshop, Chief, "Receipt", 1m, opened.Value.Line.Version);
        var line = received.Value.Line;
        var corrected = await new CorrectFabricStockCommandHandler(workshop.Db, workshop.StockChanges())
            .Handle(new(Chief, line.Id!.Value, new(line.Version, 30m, Day, "Інвентаризація")), CancellationToken.None);
        var receipt = corrected.Value.Movements.Single(x => x.Kind == "Receipt");
        var reversed = await new ReverseFabricMovementCommandHandler(workshop.Db, workshop.StockChanges())
            .Handle(new(Chief, line.Id.Value, receipt.Id, new(corrected.Value.Line.Version, "Помилкова накладна")),
                CancellationToken.None);
        var twice = await new ReverseFabricMovementCommandHandler(workshop.Db, workshop.StockChanges())
            .Handle(new(Chief, line.Id.Value, receipt.Id, new(reversed.Value.Line.Version, "Ще раз")), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(received.Value.Line.Meters, Is.EqualTo(35.5m));
            Assert.That(received.Value.Movements[0].SupplierName, Is.EqualTo("Тексика"));
            Assert.That(stale.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(corrected.Value.Movements[0].Delta, Is.EqualTo(-5.5m));
            Assert.That(reversed.Value.Line.Meters, Is.EqualTo(4.5m));
            Assert.That(reversed.Value.Movements[0], Has.Property("MetersBefore").EqualTo(30m)
                .And.Property("MetersAfter").EqualTo(4.5m));
            Assert.That(reversed.Value.Movements.Single(x => x.Id == receipt.Id).IsReversed, Is.True);
            Assert.That(twice.Status, Is.EqualTo(ResultStatus.Conflict));
        });
    }

    [Test]
    public async Task Managers_see_stock_without_value_and_cannot_move_it()
    {
        using var workshop = Workshop();
        await Receive(workshop, Chief, "Opening", 10m, null);
        var byManager = await Receive(workshop, Manager, "Receipt", 1m, 2);
        var managerList = await List(workshop, Manager);
        var chiefList = await List(workshop, Chief);

        Assert.Multiple(() =>
        {
            Assert.That(byManager.Status, Is.EqualTo(ResultStatus.Forbidden));
            Assert.That(managerList.Rows.Single().Value, Is.Null);
            Assert.That(managerList.Permissions.CanMove, Is.False);
            Assert.That(chiefList.Rows.Single().Value, Is.EqualTo(1200m));
        });
    }

    [Test]
    public async Task Plan_is_split_by_colour_and_an_issue_takes_it_off_the_reservation()
    {
        using var workshop = Workshop();
        await Receive(workshop, Chief, "Opening", 10m, null);
        var order = (await ProductionOrderHandlerTests.Create(workshop, Manager, ProductionOrderHandlerTests.Input())).Value;
        var itemId = order.Items[0].Id;
        var before = await List(workshop, Chief);
        var tab = await Fabric(workshop, Chief, order.Id, itemId);

        var issued = await Issue(workshop, Chief, order.Id, itemId, order.Version,
            L("Planned", "темно-сірий", 23, "Темно-сірий", 4m),
            L("Replacement", "червоний", 23, "Бордовий", 0.5m),
            L("Additional", null, 23, "Темно-сірий", 1m));
        var after = await List(workshop, Chief);
        var byManager = await Issue(workshop, Manager, order.Id, itemId, order.Version,
            L("Additional", null, 23, "Темно-сірий", 1m));
        var wrongPlanned = await Issue(workshop, Chief, order.Id, itemId, order.Version,
            L("Planned", "темно-сірий", 23, "Червоний", 1m));

        Assert.Multiple(() =>
        {
            Assert.That(tab.Plan.Select(x => (x.Color, x.Planned)), Is.EqualTo(new[] { ("Темно-сірий", 6.6m), ("Червоний", 0.9m) }));
            Assert.That(before.Rows.Single(x => x.Color == "Темно-сірий").Available, Is.EqualTo(3.4m));
            Assert.That(before.Rows.Single(x => x.Color == "Червоний").Id, Is.Null, "a planned colour the stock never held");
            var parts = issued.Value.Plan.ToDictionary(x => x.ColorKey);
            Assert.That(parts["темно-сірий"].Covered, Is.EqualTo(4m));
            Assert.That(parts["темно-сірий"].Reserved, Is.EqualTo(2.6m));
            Assert.That(parts["червоний"].Covered, Is.EqualTo(0.9m), "a replacement closes its part");
            Assert.That(parts["червоний"].Reserved, Is.EqualTo(0m));
            Assert.That(issued.Value.Issued, Is.EqualTo(5.5m));
            Assert.That(issued.Value.Issues.Single().Lines.Single(x => x.Purpose == "Additional").CoveredMeters, Is.EqualTo(0m));
            var grey = after.Rows.Single(x => x.Color == "Темно-сірий");
            Assert.That((grey.Meters, grey.Reserved, grey.Available), Is.EqualTo((5m, 2.6m, 2.4m)));
            Assert.That(after.Rows.Single(x => x.Color == "Бордовий").Meters, Is.EqualTo(-0.5m), "the balance may go below zero");
            Assert.That(byManager.Status, Is.EqualTo(ResultStatus.Forbidden));
            Assert.That(wrongPlanned.Status, Is.EqualTo(ResultStatus.Invalid));
        });
    }

    [Test]
    public async Task Reversing_an_issue_returns_the_fabric_and_the_reservation()
    {
        using var workshop = Workshop();
        await Receive(workshop, Chief, "Opening", 10m, null);
        var order = (await ProductionOrderHandlerTests.Create(workshop, Manager, ProductionOrderHandlerTests.Input())).Value;
        var itemId = order.Items[0].Id;
        var issued = await Issue(workshop, Chief, order.Id, itemId, order.Version,
            L("Planned", "темно-сірий", 23, "Темно-сірий", 6.6m));
        var reversed = await new ReverseFabricIssueCommandHandler(workshop.Db, workshop.StockChanges(), workshop.ItemFabric())
            .Handle(new(Chief, order.Id, issued.Value.Issues[0].Id, new(order.Version, "Видали не ту тканину")),
                CancellationToken.None);
        var stock = await List(workshop, Chief);
        var history = await workshop.Db.OrderChanges.Where(x => x.Field == "Тканина").CountAsync();

        Assert.Multiple(() =>
        {
            Assert.That(issued.Value.Plan[0].Reserved, Is.EqualTo(0m));
            Assert.That(reversed.Value.Issues[0].IsReversed, Is.True);
            Assert.That(reversed.Value.Plan[0].Reserved, Is.EqualTo(6.6m));
            Assert.That(stock.Rows.Single(x => x.Color == "Темно-сірий").Meters, Is.EqualTo(10m));
            Assert.That(history, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task Import_checks_every_line_and_a_repeat_changes_nothing()
    {
        using var workshop = Workshop();
        var good = Line("s1", 12.5m, Movement("m1", "Opening", 0m, 10m), Movement("m2", "Receipt", 10m, 2.5m));
        var broken = Line("s2", 5m, Movement("m3", "Opening", 0m, 4m));

        var withError = await Import(workshop, false, good, broken);
        var preview = await Import(workshop, true, good);
        var saved = await Import(workshop, false, good);
        var repeat = await Import(workshop, false, good);
        var line = await workshop.Db.FabricStocks.Include(x => x.Movements).SingleAsync();

        Assert.Multiple(() =>
        {
            Assert.That(withError.Value.Errors, Is.EqualTo(1));
            Assert.That(withError.Value.Lines[1].Error, Does.Contain("залишок 5"));
            Assert.That(preview.Value.DryRun, Is.True);
            Assert.That(saved.Value, Has.Property("Created").EqualTo(1).And.Property("DryRun").EqualTo(false));
            Assert.That(repeat.Value, Has.Property("Created").EqualTo(0).And.Property("Unchanged").EqualTo(1));
            Assert.That(line.Meters, Is.EqualTo(12.5m));
            Assert.That(line.Movements.Select(x => x.CreatedByName), Is.All.EqualTo("Власниця"));
        });
    }

    private static FabricIssueLineInput L(string purpose, string? plan, int fabricId, string color, decimal meters) =>
        new(purpose, plan, fabricId, color, meters);

    private static ProductionTestHarness Workshop()
    {
        var workshop = ProductionOrderHandlerTests.Workshop();
        workshop.Fabrics.Fabrics.Clear();
        workshop.Fabrics.Fabrics.Add(new(23, "FAB-023", "Тканина «Грета»", 120m));
        workshop.Suppliers.Suppliers.Add(new(7, "Тексика"));
        return workshop;
    }

    private static Task<Result<FabricStockLineResponse>> Receive(ProductionTestHarness workshop, ProductionActor actor,
        string kind, decimal meters, int? version, int? supplierId = null) =>
        new CreateFabricMovementCommandHandler(workshop.StockChanges(), workshop.Suppliers, workshop.Colors())
            .Handle(new(actor, new(kind, 23, "Темно-сірий", meters, Day, supplierId, "рах 12137", null, version)),
                CancellationToken.None).AsTask();

    private static async Task<FabricStockListResponse> List(ProductionTestHarness workshop, ProductionActor actor) =>
        (await new GetFabricStockQueryHandler(workshop.Db, workshop.Reservations(), workshop.StockRows())
            .Handle(new(actor, null), CancellationToken.None)).Value;

    private static async Task<ProductionItemFabricResponse> Fabric(ProductionTestHarness workshop, ProductionActor actor,
        int orderId, int itemId) =>
        (await new GetProductionItemFabricQueryHandler(workshop.Db, workshop.ItemFabric())
            .Handle(new(actor, orderId, itemId), CancellationToken.None)).Value;

    private static Task<Result<ProductionItemFabricResponse>> Issue(ProductionTestHarness workshop,
        ProductionActor actor, int orderId, int itemId, int version, params FabricIssueLineInput[] lines) =>
        new CreateFabricIssueCommandHandler(workshop.Db, workshop.StockChanges(), workshop.Reservations(),
                workshop.ItemFabric())
            .Handle(new(actor, orderId, itemId, new(version, Day, null, lines)), CancellationToken.None).AsTask();

    private static FabricStockImportRow Line(string id, decimal meters, params FabricMovementImportRow[] movements) =>
        new(id, "FAB-023", id == "s1" ? "Темно-сірий" : "Жовтий", meters, movements);

    private static FabricMovementImportRow Movement(string id, string kind, decimal before, decimal delta) =>
        new(id, kind, before, delta, before + delta, new DateOnly(2026, 10, 6),
            new DateTimeOffset(2026, 10, 6, 9, 0, 0, TimeSpan.Zero), null, null, null, "owner@brevi.ua", null, null);

    private static Task<Result<FabricStockImportResult>> Import(ProductionTestHarness workshop, bool dryRun,
        params FabricStockImportRow[] rows) =>
        new ImportFabricStockCommandHandler(workshop.Db, workshop.Fabrics, workshop.Staff, workshop.Reservations())
            .Handle(new(Admin, new(rows, [], []), dryRun), CancellationToken.None).AsTask();
}
