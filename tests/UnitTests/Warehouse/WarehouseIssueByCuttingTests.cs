using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using UnitTests.Production;
using static UnitTests.Warehouse.WarehouseStockHandlerTests;
using Warehouse.Application.Features.Stock.Commands.CreateFabricIssue;
using Warehouse.Application.Features.Stock.Commands.EditFabricMovement;
using Warehouse.Application.Features.Stock.Commands.RefreshItemFabricPlan;
using Warehouse.Application.Features.Stock.Commands.VoidFabricMovement;
using Warehouse.Application.Features.Stock.Queries.GetFabricMovements;
using Warehouse.Application.Features.Stock.Queries.GetProductionItemFabric;
using Warehouse.Application.Features.Stock.Shared;

namespace UnitTests.Warehouse;

/// <summary>«Видача» by actual cutting: plan of the products cut, saved plan, edits of order issues (WH-004).</summary>
public sealed class WarehouseIssueByCuttingTests
{
    private static readonly WarehouseActor Chief = new(4, false, false, true);
    private static readonly global::Production.Application.Features.Orders.Shared.Access.ProductionActor OrderManager = new(2, false, true, false);

    [Test]
    public async Task Two_lines_of_one_part_are_compared_with_its_plan_once()
    {
        using var workshop = Workshop();
        await Receive(workshop, Chief, "Opening", 10m);
        await Receive(workshop, Chief, "Opening", 5m, "Сірий");
        var (orderId, itemId, version) = await Order(workshop);

        // All three products: the plan of «темно-сірий» is 6,6 m; the cutter used 4 m of it and 2,5 m of another colour.
        var issued = await IssueCut(workshop, Chief, orderId, itemId, version, 3,
            [L("темно-сірий", "Темно-сірий", 4m), L("темно-сірий", "Сірий", 2.5m)]);

        Assert.That(issued.IsSuccess, Is.True, string.Join("; ", issued.ValidationErrors.Select(x => x.ErrorMessage)));
        var issue = issued.Value.Issues.Single();
        var dark = issue.Comparison.Single(x => x.PlanColorKey == "темно-сірий");
        Assert.Multiple(() =>
        {
            Assert.That(issue.Lines.Select(x => x.Purpose), Is.EqualTo(new[] { "Planned", "Replacement" }));
            Assert.That(issue.Lines.Select(x => x.CoveredMeters), Is.EqualTo(new[] { 4m, 2.6m }),
                "the plan goes to the lines in order, the rest to the last one");
            Assert.That((dark.Plan, dark.Actual, dark.Difference), Is.EqualTo((6.6m, 6.5m, -0.1m)));
            Assert.That(dark.PerProduct, Is.EqualTo(2.167m));
            var part = issued.Value.Plan.Single(x => x.ColorKey == "темно-сірий");
            Assert.That((part.Covered, part.Missing, part.Reserved), Is.EqualTo((6.6m, 0m, 0m)));
        });
    }

    [Test]
    public async Task The_plan_is_saved_at_the_first_issue_and_changes_only_by_refresh()
    {
        using var workshop = Workshop();
        await Receive(workshop, Chief, "Opening", 20m);
        var (orderId, itemId, version) = await Order(workshop);
        var first = await IssueCut(workshop, Chief, orderId, itemId, version, 1, [L("темно-сірий", "Темно-сірий", 2m)]);
        await SetNorm(workshop, 3m);

        var card = await Card(workshop, orderId, itemId);
        var stale = await IssueCut(workshop, Chief, orderId, itemId, version, 1, [L("темно-сірий", "Темно-сірий", 2m)],
            planVersion: 0);
        var refreshed = await Refresh(workshop, orderId, itemId, version, 1);
        var again = await Refresh(workshop, orderId, itemId, version, 2);
        var history = await workshop.Workshop.Db.OrderChanges.Where(x => x.Field == "Тканина")
            .Select(x => x.After).ToListAsync();

        Assert.Multiple(() =>
        {
            Assert.That(first.Value.Issues.Single().Comparison.Single().Plan, Is.EqualTo(2.2m), "6,6 × 1 / 3");
            Assert.That((card.PlanVersion, card.Plan.Single(x => x.ColorKey == "темно-сірий").Planned),
                Is.EqualTo((1, 6.6m)), "a norm change does not touch the saved plan");
            Assert.That(stale.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(refreshed.IsSuccess, Is.True, string.Join("; ", refreshed.Errors));
            Assert.That((refreshed.Value.PlanVersion, refreshed.Value.Plan.Single(x => x.ColorKey == "темно-сірий").Planned),
                Is.EqualTo((2, 9m)));
            Assert.That(refreshed.Value.Plan.Single(x => x.ColorKey == "темно-сірий").Missing, Is.EqualTo(6.8m));
            Assert.That(again.Status, Is.EqualTo(ResultStatus.Invalid), "the norm did not change since");
            Assert.That(history, Has.Some.StartsWith("План тканини оновлено"));
        });
    }

    [Test]
    public async Task A_repeat_moves_nothing_and_more_products_than_the_item_has_are_refused()
    {
        using var workshop = Workshop();
        await Receive(workshop, Chief, "Opening", 10m);
        var (orderId, itemId, version) = await Order(workshop);
        var operationId = Guid.NewGuid();
        FabricIssueLineInput[] lines = [L("темно-сірий", "Темно-сірий", 3m)];

        var first = await IssueCut(workshop, Chief, orderId, itemId, version, 1, lines, operationId);
        var repeat = await IssueCut(workshop, Chief, orderId, itemId, version, 1, lines, operationId, planVersion: 0);
        var otherData = await IssueCut(workshop, Chief, orderId, itemId, version, 2, lines, operationId);
        var tooMany = await IssueCut(workshop, Chief, orderId, itemId, version, 4, lines);
        var stock = await List(workshop, Chief);

        Assert.Multiple(() =>
        {
            Assert.That(first.IsSuccess, Is.True);
            Assert.That(repeat.IsSuccess, Is.True, "the same form after a lost answer");
            Assert.That(repeat.Value.Issues, Has.Count.EqualTo(1));
            Assert.That(otherData.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(tooMany.Status, Is.EqualTo(ResultStatus.Invalid));
            Assert.That(Pair(stock, "Темно-сірий").Meters, Is.EqualTo(7m));
        });
    }

    [Test]
    public async Task Editing_or_voiding_an_order_issue_restates_its_line()
    {
        using var workshop = Workshop();
        await Receive(workshop, Chief, "Opening", 10m);
        var (orderId, itemId, version) = await Order(workshop);
        await IssueCut(workshop, Chief, orderId, itemId, version, 3, [L("темно-сірий", "Темно-сірий", 6m)]);
        var row = await IssueRow(workshop, itemId);

        var edited = await Edit(workshop, row, 6.5m);
        var afterEdit = await Card(workshop, orderId, itemId);
        var voided = await new VoidFabricMovementHandler(workshop.StockChanges(), workshop.Pairs(),
                workshop.Operations(), Revisions(workshop))
            .Handle(new(Chief, row.Id, new(Guid.NewGuid(), 1, "Видали не на те завдання")), CancellationToken.None);
        var afterVoid = await Card(workshop, orderId, itemId);
        var stock = await List(workshop, Chief);

        Assert.Multiple(() =>
        {
            Assert.That(row.CanEdit, Is.True);
            Assert.That(row.CanReverse, Is.False, "an order issue is reversed as a whole package");
            Assert.That(edited.IsSuccess, Is.True, string.Join("; ", edited.Errors.Concat(edited.ValidationErrors.Select(x => x.ErrorMessage))));
            Assert.That(afterEdit.Issued, Is.EqualTo(6.5m));
            Assert.That(afterEdit.Issues.Single().Comparison.Single().Actual, Is.EqualTo(6.5m));
            Assert.That(voided.IsSuccess, Is.True);
            var line = afterVoid.Issues.Single().Lines.Single();
            Assert.That((line.IsVoided, line.CoveredMeters, line.Meters), Is.EqualTo((true, 0m, 0m)));
            Assert.That(afterVoid.Plan.Single(x => x.ColorKey == "темно-сірий").Reserved, Is.EqualTo(6.6m),
                "the reservation comes back");
            Assert.That(Pair(stock, "Темно-сірий").Meters, Is.EqualTo(10m));
        });
    }

    [Test]
    public async Task A_line_spent_over_two_offers_keeps_its_pair()
    {
        using var workshop = Workshop();
        await Receive(workshop, Chief, "Opening", 2m);
        await Receive(workshop, Chief, "Receipt", 5m, fabricId: 67, supplier: "Черкаси ВВ");
        await Receive(workshop, Chief, "Opening", 1m, "Сірий");
        var (orderId, itemId, version) = await Order(workshop);
        var issued = await IssueCut(workshop, Chief, orderId, itemId, version, 3, [L("темно-сірий", "Темно-сірий", 4m)]);
        var row = await IssueRow(workshop, itemId);

        var moved = await Edit(workshop, row, row.Delta * -1, fabricCode: "FAB-023", color: "Сірий");
        var dated = await Edit(workshop, row, row.Delta * -1, date: row.OperationDate.AddDays(-1));

        Assert.Multiple(() =>
        {
            Assert.That(issued.Value.Issues.Single().Lines.Single().Meters, Is.EqualTo(4m), "one line, two movements");
            Assert.That(moved.Status, Is.EqualTo(ResultStatus.Invalid));
            Assert.That(dated.IsSuccess, Is.True, "the date and metres of each movement may still change");
        });
    }

    [Test]
    public void The_plan_of_a_cut_is_rounded_half_up_to_hundredths()
    {
        Assert.Multiple(() =>
        {
            Assert.That(global::Warehouse.Domain.Stock.FabricItemPlan.ComparisonPlan(30m, 4, 10), Is.EqualTo(12m), "A16");
            Assert.That(global::Warehouse.Domain.Stock.FabricItemPlan.ComparisonPlan(6.6m, 1, 3), Is.EqualTo(2.2m));
            Assert.That(global::Warehouse.Domain.Stock.FabricItemPlan.ComparisonPlan(0.25m, 1, 2), Is.EqualTo(0.13m));
        });
    }

    private static async Task<(int OrderId, int ItemId, int Version)> Order(WarehouseTestHarness workshop)
    {
        var order = (await ProductionOrderHandlerTests.Create(workshop.Workshop, OrderManager,
            ProductionOrderHandlerTests.Input())).Value;
        return (order.Id, order.Items[0].Id, order.Version);
    }

    private static async Task SetNorm(WarehouseTestHarness workshop, decimal main)
    {
        var norm = await workshop.Workshop.Db.FabricNorms.SingleAsync();
        norm.Set(norm.Values with { TwoColorsMain = main }, 1, workshop.Workshop.Time.GetUtcNow());
        await workshop.Workshop.Db.SaveChangesAsync();
    }

    private static async Task<ProductionItemFabricResponse> Card(WarehouseTestHarness workshop, int orderId, int itemId)
    {
        workshop.Db.ChangeTracker.Clear();
        return (await new GetProductionItemFabricHandler(workshop.Production, workshop.ItemFabric())
            .Handle(new(Chief, orderId, itemId), CancellationToken.None)).Value;
    }

    private static async Task<Result<ProductionItemFabricResponse>> Refresh(WarehouseTestHarness workshop, int orderId,
        int itemId, int version, int planVersion)
    {
        workshop.Db.ChangeTracker.Clear();
        return await new RefreshItemFabricPlanHandler(workshop.Production, workshop.Db, workshop.StockChanges(),
                workshop.Plans(), workshop.ItemFabric())
            .Handle(new(Chief, orderId, itemId, new(version, planVersion)), CancellationToken.None);
    }

    private static async Task<FabricMovementRowResponse> IssueRow(WarehouseTestHarness workshop, int itemId)
    {
        workshop.Db.ChangeTracker.Clear();
        return (await new GetFabricMovementsHandler(workshop.Db)
                .Handle(new(Chief, 1, FabricMovementFilter.Issue, null, null, itemId), CancellationToken.None)).Value
            .Rows.OrderBy(x => x.Id).First();
    }

    private static MovementRevisions Revisions(WarehouseTestHarness workshop) =>
        new(workshop.Db, workshop.Operations(), workshop.Pairs());

    private static async Task<Result<FabricStockOperationResponse>> Edit(WarehouseTestHarness workshop,
        FabricMovementRowResponse row, decimal meters, string? fabricCode = null, string? color = null,
        DateOnly? date = null)
    {
        workshop.Db.ChangeTracker.Clear();
        return await new EditFabricMovementHandler(workshop.StockChanges(), workshop.Pairs(),
                workshop.Operations(), Revisions(workshop), workshop.Colors())
            .Handle(new(Chief, row.Id, new(Guid.NewGuid(), row.Revision, date ?? row.OperationDate, meters, null,
                fabricCode, color, null, null, null, "Перевірили розкрій")), CancellationToken.None);
    }
}
