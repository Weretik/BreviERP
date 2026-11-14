using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using UnitTests.Production;
using Warehouse.Application.Features.Stock.Commands.CreateFabricIssue;
using Warehouse.Application.Features.Stock.Commands.OpenFabricStock;
using Warehouse.Application.Features.Stock.Commands.ReceiveFabric;
using Warehouse.Application.Features.Stock.Commands.ReverseFabricIssue;
using Warehouse.Application.Features.Stock.Commands.ReverseFabricMovement;
using Warehouse.Application.Features.Stock.Queries.GetFabricMovements;
using Warehouse.Application.Features.Stock.Queries.GetFabricStock;
using Warehouse.Application.Features.Stock.Queries.GetFabricStockChoices;
using Warehouse.Application.Features.Stock.Queries.GetProductionItemFabric;
using Warehouse.Application.Features.Stock.Shared;
using Production.Application.Features.Orders.Shared.Access;

namespace UnitTests.Warehouse;

/// <summary>«Склад тканин» (Warehouse): movements, reservation by colour from the workshop plan, issues and the transfer.</summary>
public sealed class WarehouseStockHandlerTests
{
    private static readonly WarehouseActor Admin = new(1, true, false, false);
    private static readonly WarehouseActor Manager = new(2, false, true, false);
    private static readonly WarehouseActor Chief = new(4, false, false, true);
    private static readonly ProductionActor OrderManager = new(2, false, true, false);
    private static readonly DateOnly Day = new(2026, 11, 4);

    [Test]
    public async Task Receipt_and_reversal_keep_before_change_after()
    {
        using var workshop = Workshop();
        await Receive(workshop, Chief, "Opening", 10m);
        var received = await Receive(workshop, Chief, "Receipt", 25.5m, supplier: "Тексика");
        var receipt = (await Movements(workshop, Chief, FabricMovementFilter.Receipt)).Rows.Single();
        var reversed = await Reverse(workshop, Chief, receipt.Id, "Помилкова накладна");
        var twice = await Reverse(workshop, Chief, receipt.Id, "Ще раз");
        var history = (await Movements(workshop, Chief, FabricMovementFilter.All)).Rows;

        Assert.Multiple(() =>
        {
            Assert.That(received.Value.Meters, Is.EqualTo(35.5m));
            Assert.That(receipt.SupplierName, Is.EqualTo("Тексика"));
            Assert.That(receipt.Document, Is.EqualTo("рах 12137"));
            Assert.That(reversed.Value.Meters, Is.EqualTo(10m));
            Assert.That(history[0], Has.Property("Kind").EqualTo("Reversal").And.Property("MetersAfter").EqualTo(10m));
            Assert.That(history.Single(x => x.Id == receipt.Id).IsReversed, Is.True);
            Assert.That(twice.Status, Is.EqualTo(ResultStatus.Conflict));
        });
    }

    [Test]
    public async Task Managers_see_stock_and_movements_but_cannot_move_it_or_see_prices()
    {
        using var workshop = Workshop();
        await Receive(workshop, Chief, "Opening", 10m);
        var byManager = await Receive(workshop, Manager, "Receipt", 1m);
        var managerList = await List(workshop, Manager);
        var chiefList = await List(workshop, Chief);
        var managerMovements = await Movements(workshop, Manager, FabricMovementFilter.All);
        var managerChoices = await Choices(workshop, Manager);
        var chiefChoices = await Choices(workshop, Chief);

        Assert.Multiple(() =>
        {
            Assert.That(byManager.Status, Is.EqualTo(ResultStatus.Forbidden));
            Assert.That(Pair(managerList, "Темно-сірий").Meters, Is.EqualTo(10m));
            Assert.That(managerList.Permissions.CanMove, Is.False);
            Assert.That(chiefList.Permissions, Is.EqualTo(new FabricStockPermissions(true, true, false)));
            Assert.That(managerMovements.Rows.Single().CanReverse, Is.False);
            Assert.That(managerChoices.Fabrics.Select(x => x.Price), Is.All.Null);
            Assert.That(managerChoices.Suppliers, Is.Empty);
            Assert.That(chiefChoices.Fabrics.Single(x => x.Code == "FAB-023").Price, Is.EqualTo(120m));
        });
    }

    [Test]
    public async Task Plan_is_split_by_colour_and_an_issue_covers_the_plan_of_the_products_cut()
    {
        using var workshop = Workshop();
        await Receive(workshop, Chief, "Opening", 10m);
        await Receive(workshop, Chief, "Opening", 0.2m, "Сірий");
        var order = (await ProductionOrderHandlerTests.Create(workshop.Workshop, OrderManager, ProductionOrderHandlerTests.Input())).Value;
        var itemId = order.Items[0].Id;
        var before = await List(workshop, Chief);
        var tab = await Fabric(workshop, Chief, order.Id, itemId);

        // Two of three products cut: the plan of the cut is 4,4 m of «темно-сірий» and 0,6 m of «червоний».
        var issued = await IssueCut(workshop, Chief, order.Id, itemId, order.Version, 2,
            [L("темно-сірий", "Темно-сірий", 4m), L("червоний", "Сірий", 0.5m), L(null, "Темно-сірий", 1m, layer: "Lining")]);
        var after = await List(workshop, Chief);
        var byManager = await Issue(workshop, Manager, order.Id, itemId, order.Version, L(null, "Темно-сірий", 1m));
        var unknownPart = await Issue(workshop, Chief, order.Id, itemId, order.Version, L("синій", "Темно-сірий", 1m));
        var notHeld = await Issue(workshop, Chief, order.Id, itemId, order.Version, L(null, "Жовтий", 1m));

        Assert.Multiple(() =>
        {
            Assert.That(tab.Plan.Select(x => (x.Color, x.Planned)), Is.EqualTo(new[] { ("Темно-сірий", 6.6m), ("Червоний", 0.9m) }));
            Assert.That(tab.PlanVersion, Is.EqualTo(0), "no plan is saved before the first issue");
            Assert.That(Pair(before, "Темно-сірий").Meters, Is.EqualTo(10m));
            Assert.That(tab.Plan.Single(x => x.Color == "Темно-сірий").StockAvailable, Is.EqualTo(3.4m));
            Assert.That(tab.Plan.Single(x => x.Color == "Червоний").StockId, Is.Null, "a planned colour the stock never held");
            Assert.That(issued.IsSuccess, Is.True, string.Join("; ", issued.ValidationErrors.Select(x => x.ErrorMessage)));
            var card = issued.Value;
            var parts = card.Plan.ToDictionary(x => x.ColorKey);
            Assert.That(card.PlanVersion, Is.EqualTo(1), "the first issue saves the plan");
            Assert.That(parts["темно-сірий"].Covered, Is.EqualTo(4.4m));
            Assert.That(parts["темно-сірий"].Reserved, Is.EqualTo(2.2m));
            Assert.That(parts["червоний"].Covered, Is.EqualTo(0.6m), "a replacement covers its part");
            Assert.That(parts["червоний"].Reserved, Is.EqualTo(0.3m));
            Assert.That(card.Issued, Is.EqualTo(5.5m));
            var issue = card.Issues.Single();
            Assert.That(issue.IssuedQuantity, Is.EqualTo(2));
            Assert.That(issue.Lines.Select(x => x.Purpose), Is.EqualTo(new[] { "Planned", "Replacement", "Additional" }));
            Assert.That(issue.Lines.Single(x => x.Purpose == "Additional"),
                Has.Property("CoveredMeters").EqualTo(0m).And.Property("Layer").EqualTo("Lining"));
            var dark = issue.Comparison.Single(x => x.PlanColorKey == "темно-сірий");
            Assert.That((dark.Plan, dark.Actual, dark.Difference), Is.EqualTo((4.4m, 4m, -0.4m)));
            Assert.That(Pair(after, "Темно-сірий").Meters, Is.EqualTo(5m));
            Assert.That(parts["темно-сірий"].StockAvailable, Is.EqualTo(2.8m));
            Assert.That(Pair(after, "Сірий").Meters, Is.EqualTo(-0.3m), "the balance may go below zero");
            Assert.That(byManager.Status, Is.EqualTo(ResultStatus.Forbidden));
            Assert.That(unknownPart.Status, Is.EqualTo(ResultStatus.Invalid));
            Assert.That(notHeld.Status, Is.EqualTo(ResultStatus.Invalid));
        });
    }

    [Test]
    public async Task Reversing_an_issue_returns_the_fabric_and_the_reservation()
    {
        using var workshop = Workshop();
        await Receive(workshop, Chief, "Opening", 10m);
        var order = (await ProductionOrderHandlerTests.Create(workshop.Workshop, OrderManager, ProductionOrderHandlerTests.Input())).Value;
        var itemId = order.Items[0].Id;
        var issued = await Issue(workshop, Chief, order.Id, itemId, order.Version,
            L("темно-сірий", "Темно-сірий", 6.6m));
        var reversed = await new ReverseFabricIssueHandler(workshop.Production, workshop.Db, workshop.StockChanges(),
                workshop.ItemFabric())
            .Handle(new(Chief, order.Id, issued.Value.Issues[0].Id, new(order.Version, "Видали не ту тканину")),
                CancellationToken.None);
        var stock = await List(workshop, Chief);
        var history = await workshop.Workshop.Db.OrderChanges.Where(x => x.Field == "Тканина").CountAsync();

        Assert.Multiple(() =>
        {
            Assert.That(issued.Value.Plan[0].Reserved, Is.EqualTo(0m));
            Assert.That(reversed.Value.Issues[0].IsReversed, Is.True);
            Assert.That(reversed.Value.Plan[0].Reserved, Is.EqualTo(6.6m));
            Assert.That(Pair(stock, "Темно-сірий").Meters, Is.EqualTo(10m));
            Assert.That(history, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task New_stock_line_takes_a_colour_from_the_list_and_choices_list_it()
    {
        using var workshop = Workshop();
        var rejected = await Receive(workshop, Chief, "Opening", 5m, color: "Бузковий");
        var accepted = await Receive(workshop, Chief, "Opening", 5m, color: "Жовтий");
        var choices = await Choices(workshop, Chief);

        Assert.Multiple(() =>
        {
            Assert.That(rejected.Status, Is.EqualTo(ResultStatus.Invalid));
            Assert.That(accepted.IsSuccess, Is.True,
                string.Join("; ", accepted.ValidationErrors.Select(x => x.ErrorMessage)));
            Assert.That(choices.Colors, Is.EqualTo(workshop.Reference.Colors));
            Assert.That(choices.Suppliers, Is.EqualTo(new[] { "Тексика", "Черкаси ВВ" }));
            Assert.That(choices.Fabrics.Where(x => x.GroupCode == "FAB-023").Select(x => x.Code),
                Is.EqualTo(new[] { "FAB-023", "FAB-067" }));
        });
    }

    [Test]
    public async Task Stock_facts_give_the_report_issues_and_the_balance_at_a_date()
    {
        using var workshop = Workshop();
        await Receive(workshop, Chief, "Opening", 10m);
        var order = (await ProductionOrderHandlerTests.Create(workshop.Workshop, OrderManager,
            ProductionOrderHandlerTests.Input())).Value;
        await Issue(workshop, Chief, order.Id, order.Items[0].Id, order.Version,
            L("темно-сірий", "Темно-сірий", 4m));

        var issues = await workshop.Facts().IssuesAsync(new DateOnly(2026, 11, 1), new DateOnly(2026, 12, 1),
            CancellationToken.None);
        var endOfNovember = await workshop.Facts().BalancesAsync(new DateOnly(2026, 12, 1), CancellationToken.None);
        var beforeNovember = await workshop.Facts().BalancesAsync(new DateOnly(2026, 11, 1), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(issues.Single(), Has.Property("Meters").EqualTo(4m).And.Property("OrderId").EqualTo(order.Id));
            Assert.That(endOfNovember.Single().Meters, Is.EqualTo(6m));
            Assert.That(beforeNovember.Single().Meters, Is.EqualTo(0m), "movements of November are taken off");
        });
    }

    /// <summary>A line of «Видача»: the planned pair it covers (none for an additional one) and the real pair.</summary>
    internal static FabricIssueLineInput L(string? planColor, string color, decimal meters, string code = "FAB-023",
        string? planCode = "FAB-023", string? layer = null) =>
        new(code, color, null, meters, planColor is null ? null : planCode, planColor, planColor is null ? layer : null,
            null);

    internal static WarehouseTestHarness Workshop() => new(ProductionOrderHandlerTests.Workshop());

    /// <summary>«Початковий залишок» («Opening») or «Прихід» («Receipt») of FAB-023 or another offer.</summary>
    internal static Task<Result<FabricStockOperationResponse>> Receive(WarehouseTestHarness workshop,
        WarehouseActor actor, string kind, decimal meters, string color = "Темно-сірий", int fabricId = 23,
        string? supplier = null, DateOnly? date = null, Guid? operationId = null)
    {
        var id = operationId ?? Guid.NewGuid();
        return kind == "Opening"
            ? new OpenFabricStockHandler(workshop.StockChanges(), workshop.Pairs(), workshop.Operations(),
                    workshop.Colors())
                .Handle(new(actor, new(id, fabricId, color, meters, date ?? Day)), CancellationToken.None).AsTask()
            : new ReceiveFabricHandler(workshop.StockChanges(), workshop.Pairs(), workshop.Operations(),
                    workshop.Colors())
                .Handle(new(actor, new(id, fabricId, color, meters, date ?? Day, supplier, "рах 12137", null)),
                    CancellationToken.None).AsTask();
    }

    internal static async Task<FabricStockListResponse> List(WarehouseTestHarness workshop, WarehouseActor actor) =>
        (await new GetFabricStockHandler(workshop.Db, workshop.Reference, workshop.Booking())
            .Handle(new(actor), CancellationToken.None)).Value;

    internal static FabricStockColorResponse Pair(FabricStockListResponse list, string color, string code = "FAB-023") =>
        list.Fabrics.Single(x => x.FabricCode == code).Colors.Single(x => x.Color == color);

    internal static async Task<FabricMovementPageResponse> Movements(WarehouseTestHarness workshop,
        WarehouseActor actor, FabricMovementFilter kind, int page = 1, string? code = null, string? color = null) =>
        (await new GetFabricMovementsHandler(workshop.Db)
            .Handle(new(actor, page, kind, code, color, null), CancellationToken.None)).Value;

    internal static Task<Result<FabricStockOperationResponse>> Reverse(WarehouseTestHarness workshop,
        WarehouseActor actor, int movementId, string reason) =>
        new ReverseFabricMovementHandler(workshop.Db, workshop.StockChanges(), workshop.Pairs())
            .Handle(new(actor, movementId, new(reason)), CancellationToken.None).AsTask();

    private static async Task<FabricStockChoicesResponse> Choices(WarehouseTestHarness workshop, WarehouseActor actor) =>
        (await new GetFabricStockChoicesHandler(workshop.Reference, workshop.Colors())
            .Handle(new(actor), CancellationToken.None)).Value;

    private static async Task<ProductionItemFabricResponse> Fabric(WarehouseTestHarness workshop, WarehouseActor actor,
        int orderId, int itemId) =>
        (await new GetProductionItemFabricHandler(workshop.Production, workshop.ItemFabric())
            .Handle(new(actor, orderId, itemId), CancellationToken.None)).Value;

    /// <summary>«Видача» of all three products of the item, against its saved plan version.</summary>
    internal static Task<Result<ProductionItemFabricResponse>> Issue(WarehouseTestHarness workshop,
        WarehouseActor actor, int orderId, int itemId, int version, params FabricIssueLineInput[] lines) =>
        IssueCut(workshop, actor, orderId, itemId, version, 3, lines);

    internal static async Task<Result<ProductionItemFabricResponse>> IssueCut(WarehouseTestHarness workshop,
        WarehouseActor actor, int orderId, int itemId, int version, int cut, FabricIssueLineInput[] lines,
        Guid? operationId = null, int? planVersion = null)
    {
        var saved = planVersion ?? await workshop.Db.FabricItemPlans.AsNoTracking().Where(x => x.ItemId == itemId)
            .Select(x => x.Version).SingleOrDefaultAsync();
        return await new CreateFabricIssueHandler(workshop.Production, workshop.Db, workshop.StockChanges(),
                workshop.Pairs(), workshop.Operations(), workshop.Plans(), workshop.ItemFabric())
            .Handle(new(actor, orderId, itemId, new(operationId ?? Guid.NewGuid(), version, saved, Day, cut, null, lines)),
                CancellationToken.None);
    }
}
