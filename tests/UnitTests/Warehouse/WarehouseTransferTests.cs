using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Commands.TransferStock;
using Warehouse.Application.Features.Stock.Shared;
using Production.Application.Features.Orders.Commands.ImportProductionOrders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Warehouse.Domain.Stock.Movements;
using Warehouse.Domain.Stock.Plans;
using Production.Application.Features.Orders.Commands.ImportProductionOrders.Rows;
using Production.Application.Features.Orders.Shared.Validation;
using Warehouse.Application.Features.Stock.Commands.TransferStock.Transfer;
using Warehouse.Application.Features.Stock.Commands.TransferStock.V133.Orders;
using Warehouse.Application.Features.Stock.Commands.TransferStock.V133.Stock;

namespace UnitTests.Warehouse;

/// <summary>WH-006 «Перенесення з v133»: the stock of the old «Цех» replaces the Brevi stock from its D1 export.</summary>
public sealed class WarehouseTransferTests
{
    private static readonly WarehouseActor Admin = new(1, true, false, false);
    private static readonly WarehouseActor Chief = new(4, false, false, true);
    private const string Order = "order-1";
    private const string Item = "item-1";

    [Test]
    public async Task Dry_run_checks_everything_and_apply_brings_the_same_stock_once()
    {
        using var workshop = await Workshop();
        var input = Export();

        var dry = await Transfer(workshop, input, dryRun: true);
        var afterDry = await workshop.Db.FabricStocks.CountAsync();
        var applied = await Transfer(workshop, input);
        var repeat = await Transfer(workshop, input);
        workshop.Db.ChangeTracker.Clear();
        var lines = await workshop.Db.FabricStocks.OrderBy(x => x.Color).ToListAsync();
        var movements = await workshop.Db.FabricMovements.OrderBy(x => x.Id).ToListAsync();
        var revisions = await workshop.Db.FabricMovementRevisions.OrderBy(x => x.Id).ToListAsync();
        var issue = await workshop.Db.FabricIssues.Include(x => x.Lines).SingleAsync();
        var plan = await workshop.Db.FabricItemPlans.SingleAsync();

        Assert.Multiple(() =>
        {
            Assert.That(dry.Value.Errors, Is.Empty);
            Assert.That(dry.Value.Applied, Is.False);
            Assert.That(dry.Value.Target, Is.EqualTo(new StockTransferTarget(2, 5, 2, 1, 1, 1, 12.5m)));
            Assert.That(afterDry, Is.Zero, "a dry run rolls back");
            Assert.That(applied.Value.Applied, Is.True);
            Assert.That(repeat.Value, Has.Property("Unchanged").True.And.Property("Applied").False);
            Assert.That(lines.Select(x => (x.LegacyId, x.Meters)), Is.EqualTo(new[] { ("s-grey", 12.5m), ("s-red", 0m) }));
            Assert.That(movements.Select(x => (x.LegacyId, x.Stock.LegacyId, x.MetersBefore, x.Delta, x.MetersAfter)),
                Is.EquivalentTo(new[]
                {
                    ("m-open", "s-grey", 0m, 10m, 10m), ("m-receipt", "s-grey", 10m, 5m, 15m),
                    ("m-issue", "s-grey", 15m, -2.5m, 12.5m), ("m-wrong", "s-grey", 12.5m, 0m, 12.5m),
                    ("m-red", "s-red", 0m, 0m, 0m)
                }));
            Assert.That(movements.Single(x => x.LegacyId == "m-wrong"),
                Has.Property("IsVoided").True.And.Property("Kind").EqualTo(FabricMovementKind.Receipt));
            Assert.That(revisions.Select(x => x.Action),
                Is.EqualTo(new[] { FabricMovementRevisionAction.Edit, FabricMovementRevisionAction.Void }));
            Assert.That(revisions[0].BeforeStockId, Is.EqualTo(lines[1].Id), "the receipt was moved from red to grey");
            Assert.That(movements.Single(x => x.LegacyId == "m-issue").CreatedByName, Is.EqualTo("Власниця"));
            Assert.That(issue, Has.Property("IssuedQuantity").EqualTo(4).And.Property("PlanVersion").EqualTo(1));
            Assert.That(issue.Lines.Single(), Has.Property("Purpose").EqualTo(FabricIssuePurpose.Planned)
                .And.Property("Meters").EqualTo(2.5m).And.Property("PlanCoverage").EqualTo(2.4m)
                .And.Property("Layer").EqualTo(FabricLayer.Upper));
            Assert.That(plan.Parts.Select(x => (x.FabricCode, x.ColorKey, x.Meters)),
                Is.EqualTo(new[] { ("FAB-023", "темно-сірий", 6m) }));
            Assert.That(applied.Value.Notes.Select(x => x.Message), Has.Some.Contains("кількість виробів"));
        });
    }

    [Test]
    public async Task An_export_that_does_not_add_up_or_was_cut_short_changes_nothing()
    {
        using var workshop = await Workshop();
        var broken = Export() with
        {
            FabricStock = [Stock("s-grey", 1300), Stock("s-red", 0)],
            FabricIssueLines = [Line() with { Note = "текст [truncated for model]" }]
        };
        var noOrder = Export() with { ProductionOrders = [new V133OrderRow(Order, 7, "new", null), new("order-2", 8, "new", null)] };
        noOrder = noOrder with
        {
            ProductionItems = [new V133ItemRow(Item, Order, "MOD-052", "Куртка «Віол»", 10),
                new V133ItemRow("item-2", "order-2", "MOD-052", "Куртка «Віол»", 4)],
            FabricStockMovements = Export().FabricStockMovements
                .Select(x => x.Id == "m-issue" ? x with { ItemId = "item-2", OrderId = "order-2" } : x).ToList(),
            FabricIssueBatches = [Batch() with { ItemId = "item-2" }],
            FabricIssueLines = [Line() with { ItemId = "item-2" }],
            FabricReservations = []
        };

        var result = await Transfer(workshop, broken);
        var missing = await Transfer(workshop, noOrder);

        Assert.Multiple(() =>
        {
            Assert.That(result.Value.Errors.Select(x => x.Message), Has.Some.Contains("залишок рядка 13.00")
                .And.Some.Contains("[truncated for model]"));
            Assert.That(missing.Value.Errors.Select(x => x.Message), Has.Some.Contains("Завдання №8 немає в Brevi"));
            Assert.That(result.Value.Applied || missing.Value.Applied, Is.False);
            Assert.That(workshop.Db.FabricStocks.Count(), Is.Zero);
        });
    }

    [Test]
    public async Task Records_entered_in_Brevi_are_replaced_only_when_confirmed()
    {
        using var workshop = await Workshop();
        await WarehouseStockHandlerTests.Receive(workshop, Chief, "Opening", 3m, color: "Жовтий");
        workshop.Db.ChangeTracker.Clear();

        var refused = await Transfer(workshop, Export());
        var kept = await workshop.Db.FabricStocks.CountAsync(x => x.LegacyId == null);
        var replaced = await Transfer(workshop, Export(), confirm: true);
        workshop.Db.ChangeTracker.Clear();

        Assert.Multiple(() =>
        {
            Assert.That(refused.Value, Has.Property("Applied").False.And.Property("NeedsConfirmation").True);
            Assert.That(refused.Value.Current, Has.Property("OwnLines").EqualTo(1).And.Property("OwnMovements").EqualTo(1));
            Assert.That(kept, Is.EqualTo(1));
            Assert.That(replaced.Value.Applied, Is.True);
            Assert.That(workshop.Db.FabricStocks.Select(x => x.LegacyId), Is.EquivalentTo(new[] { "s-grey", "s-red" }));
            Assert.That(workshop.Db.StockTransfers.Single().Lines, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task Only_an_admin_transfers()
    {
        using var workshop = await Workshop();

        var result = await Transfer(workshop, Export(), actor: Chief);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.Forbidden));
    }

    /// <summary>
    /// FAB-023: grey opened with 10 m, a receipt of 5 m first put on red by mistake and moved to grey, 2.5 m issued to
    /// the item (4 of 10 products), and a wrong receipt of 1 m voided. Red was opened at zero.
    /// </summary>
    private static StockTransferInput Export() => new("тест",
        [Stock("s-grey", 1250), Stock("s-red", 0)],
        [
            Movement("m-open", "s-grey", "opening", 0, 1000, "2026-10-06T07:00:00Z"),
            Movement("m-red", "s-red", "opening", 0, 0, "2026-10-06T07:01:00Z"),
            Movement("m-receipt", "s-grey", "receipt", 1000, 500, "2026-10-07T07:00:00Z"),
            Movement("m-issue", "s-grey", "issue", 1500, -250, "2026-10-08T07:00:00Z") with
            {
                OrderId = Order, ItemId = Item, OrderNumber = "Ц-00007", ItemQuantity = 4,
                ActorEmail = "owner@brevi.ua"
            },
            Movement("m-wrong", "s-grey", "void", 1250, 0, "2026-10-09T07:00:00Z")
        ],
        [
            Revision("r-1", "m-receipt", "2026-10-07T08:00:00Z", State("s-red", "receipt", 500), State("s-grey", "receipt", 500)),
            Revision("r-2", "m-wrong", "2026-10-09T08:00:00Z", State("s-grey", "receipt", 100, "2026-10-09"),
                State("s-grey", "void", 0, "2026-10-09"))
        ],
        [Batch()],
        [Line()],
        [new V133IssueComponentRow("m-issue", "line-1")],
        [new V133ReservationRow("res-1", Order, Item, "FAB-023", "Темно-сірий", 600)],
        [new V133OrderRow(Order, 7, "new", null)],
        [new V133ItemRow(Item, Order, "MOD-052", "Куртка «Віол»", 10)]);

    private static V133StockRow Stock(string id, long hundredths) =>
        new(id, "FAB-023", "Тканина «Грета»", id == "s-grey" ? "Темно-сірий" : "Червоний", null, hundredths, null, null);

    private static V133MovementRow Movement(string id, string stock, string kind, long before, long delta, string at) =>
        new(id, stock, "FAB-023", null, null, kind, before, delta, before + delta, null, null, null, null, null, null, "",
            "", "", null, at, at[..10]);

    private static string State(string stock, string kind, long delta, string date = "2026-10-07") =>
        $$"""{"movement":{"stockId":"{{stock}}","operationDate":"{{date}}","kind":"{{kind}}","deltaHundredths":{{delta}},"supplier":"","reference":""},"line":null}""";

    private static V133RevisionRow Revision(string id, string movement, string at, string before, string after) =>
        new(id, Guid.NewGuid().ToString(), movement, before, after, "owner@brevi.ua", at);

    private static V133BatchRow Batch() =>
        new("batch-1", Item, "owner@brevi.ua", """{"ok":true,"issuedQuantity":4,"comparison":{"x":"[truncated for model]""",
            "2026-10-08T07:00:00Z");

    private static V133IssueLineRow Line() =>
        new("line-1", "batch-1", Item, "s-grey", "planned", """["FAB-023","темно-сірий"]""", "FAB-023", "Темно-сірий",
            240, "FAB-023", "Темно-сірий", 250, 240, "", "2026-10-08T07:00:00Z",
            """{"version":1,"component":null,"actualRole":"upper"}""", null);

    private static async Task<WarehouseTestHarness> Workshop()
    {
        var workshop = WarehouseStockHandlerTests.Workshop();
        await new ImportProductionOrdersHandler(workshop.Workshop.Db, workshop.Workshop.Staff, workshop.Workshop.Time)
            .Handle(new(new ProductionActor(1, true, false, false),
            [
                new ProductionOrderImportRow(Order, 7, null, "Клієнт", null, "Менеджер", "New", new DateOnly(2026, 10, 6),
                    null, [new ProductionItemImportRow("MOD-052", "Куртка «Віол»", "FAB-023", "Тканина «Грета»", null,
                        new ProductionItemDetailsInput(10, new DateOnly(2026, 11, 30), "Темно-сірий", [], null, null, null, []))])
            ], false), CancellationToken.None);
        return workshop;
    }

    /// <summary>One request: the context forgets what it tracked, as a new request scope would.</summary>
    private static async Task<Result<StockTransferResult>> Transfer(WarehouseTestHarness workshop,
        StockTransferInput input, bool dryRun = false, bool confirm = false, WarehouseActor? actor = null)
    {
        var result = await new TransferStockHandler(workshop.Db, workshop.Reference, workshop.Production,
                workshop.Staff, workshop.Workshop.Time)
            .Handle(new(actor ?? Admin, input, dryRun, confirm), CancellationToken.None);
        workshop.Db.ChangeTracker.Clear();
        return result;
    }
}
