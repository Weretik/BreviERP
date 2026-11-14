using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Warehouse.Domain.Stock;
using static UnitTests.Warehouse.WarehouseStockHandlerTests;
using Warehouse.Application.Features.Stock.Commands.AdjustFabricStock;
using Warehouse.Application.Features.Stock.Commands.IssueFabric;
using Warehouse.Application.Features.Stock.Queries.GetFabricMovements;
using Warehouse.Application.Features.Stock.Queries.GetFabricStockChoices;
using Warehouse.Application.Features.Stock.Shared;
using Warehouse.Domain.Stock.Movements;
using Warehouse.Application.Features.Stock.Queries.GetFabricMovements.Results;
using Warehouse.Application.Features.Stock.Shared.Operations;

namespace UnitTests.Warehouse;

/// <summary>Ledger rules of «Склад тканин» (WH-002, TZ v133 scenarios A01–A13, A22–A23).</summary>
public sealed class WarehouseLedgerTests
{
    private static readonly WarehouseActor Chief = new(4, false, false, true);
    private static readonly DateOnly Day = new(2026, 11, 4);

    [Test]
    public async Task Opening_is_once_per_pair_under_any_code_of_its_group_and_may_be_zero()
    {
        using var workshop = Workshop();
        var opened = await Receive(workshop, Chief, "Opening", 10.25m);
        var byAlias = await Receive(workshop, Chief, "Opening", 1m, color: "  темно-сірий ", fabricId: 67);
        var zero = await Receive(workshop, Chief, "Opening", 0m, color: "Жовтий");
        var zeroAgain = await Receive(workshop, Chief, "Opening", 0m, color: "Жовтий");
        var openings = await Movements(workshop, Chief, FabricMovementFilter.Opening);

        Assert.Multiple(() =>
        {
            Assert.That(opened.Value.Meters, Is.EqualTo(10.25m));
            Assert.That(byAlias.Status, Is.EqualTo(ResultStatus.Conflict), "A02: FAB-067 is FAB-023");
            Assert.That(zero.Value.Meters, Is.EqualTo(0m));
            Assert.That(zeroAgain.Status, Is.EqualTo(ResultStatus.Conflict), "A03");
            Assert.That(openings.Rows, Has.Count.EqualTo(2));
            Assert.That(openings.Rows.Select(x => x.Kind), Is.All.EqualTo("Opening"));
        });
    }

    [Test]
    public async Task Receipt_keeps_the_offer_picked_and_the_pair_sums_its_lines()
    {
        using var workshop = Workshop();
        await Receive(workshop, Chief, "Opening", 10.25m);
        var received = await Receive(workshop, Chief, "Receipt", 5.5m, fabricId: 67, supplier: "Черкаси ВВ",
            date: new DateOnly(2026, 11, 5));
        var pair = Pair(await List(workshop, Chief), "Темно-сірий");
        var receipt = (await Movements(workshop, Chief, FabricMovementFilter.Receipt)).Rows.Single();

        Assert.Multiple(() =>
        {
            Assert.That(received.Value.Meters, Is.EqualTo(15.75m), "A04");
            Assert.That(received.Value.FabricCode, Is.EqualTo("FAB-023"), "A05");
            Assert.That(pair.Sources.Select(x => (x.FabricCode, x.Meters)),
                Is.EqualTo(new[] { ("FAB-023", 10.25m), ("FAB-067", 5.5m) }), "A12: lines stay separate");
            Assert.That(receipt.FabricCode, Is.EqualTo("FAB-067"));
            Assert.That(receipt.GroupCode, Is.EqualTo("FAB-023"));
            Assert.That((receipt.SupplierName, receipt.Document, receipt.OperationDate),
                Is.EqualTo(("Черкаси ВВ", "рах 12137", new DateOnly(2026, 11, 5))));
        });
    }

    [Test]
    public async Task One_off_issue_spends_lines_in_order_and_may_go_below_zero()
    {
        using var workshop = Workshop();
        await Receive(workshop, Chief, "Opening", 3m);
        await Receive(workshop, Chief, "Receipt", 4m, fabricId: 67);
        var version = Pair(await List(workshop, Chief), "Темно-сірий").Version;

        var split = await Issue(workshop, 5m, version, "Пошиття зразка");
        var stale = await Issue(workshop, 1m, version);
        var shortage = await Issue(workshop, 3m, null);
        var pair = Pair(await List(workshop, Chief), "Темно-сірий");
        var issues = (await Movements(workshop, Chief, FabricMovementFilter.Issue)).Rows;

        Assert.Multiple(() =>
        {
            Assert.That(split.Value.Meters, Is.EqualTo(2m), "A13");
            Assert.That(issues.Where(x => x.Destination == "Пошиття зразка").Select(x => (x.FabricCode, x.Delta)),
                Is.EquivalentTo(new[] { ("FAB-023", -3m), ("FAB-067", -2m) }));
            Assert.That(issues.Select(x => x.OrderId), Is.All.Null, "A06: no order");
            Assert.That(stale.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(shortage.Value.Meters, Is.EqualTo(-1m), "A07: the shortage is visible");
            Assert.That(pair.Sources.Select(x => (x.FabricCode, x.Meters)),
                Is.EqualTo(new[] { ("FAB-023", -1m), ("FAB-067", 0m) }), "the rest leaves the canonical line");
        });
    }

    [Test]
    public async Task Adjustment_sets_the_counted_total_but_never_deepens_a_shortage()
    {
        using var workshop = Workshop();
        await Receive(workshop, Chief, "Opening", 15m);
        var to1225 = await Adjust(workshop, 12.25m, "Інвентаризація");
        var history = (await Movements(workshop, Chief, FabricMovementFilter.All)).Rows;
        await Issue(workshop, 22.25m, null);
        var deeper = await Adjust(workshop, -12m, "Перерахунок");
        var lighter = await Adjust(workshop, -5m, "Перерахунок");
        var same = await Adjust(workshop, -5m, "Перерахунок");
        var shortReason = new AdjustFabricStockValidator().Validate(new AdjustFabricStockCommand(Chief,
            new(Guid.NewGuid(), "FAB-023", "Темно-сірий", "v", 1m, Day, "ok")));

        Assert.Multiple(() =>
        {
            Assert.That(to1225.Value.Meters, Is.EqualTo(12.25m), "A08");
            Assert.That(history[0], Has.Property("Kind").EqualTo("Adjustment").And.Property("Delta").EqualTo(-2.75m)
                .And.Property("Note").EqualTo("Інвентаризація"));
            Assert.That(deeper.Status, Is.EqualTo(ResultStatus.Invalid), "A09: -10 → -12 is refused");
            Assert.That(lighter.Value.Meters, Is.EqualTo(-5m), "A09: -10 → -5 is allowed");
            Assert.That(same.Status, Is.EqualTo(ResultStatus.Invalid));
            Assert.That(shortReason.IsValid, Is.False, "the reason is 3…500 characters");
        });
    }

    [Test]
    public async Task The_same_operation_id_repeats_the_result_and_other_data_is_refused()
    {
        using var workshop = Workshop();
        await Receive(workshop, Chief, "Opening", 10m);
        var id = Guid.NewGuid();
        var first = await Receive(workshop, Chief, "Receipt", 5m, operationId: id);
        var repeat = await Receive(workshop, Chief, "Receipt", 5m, operationId: id);
        var other = await Receive(workshop, Chief, "Receipt", 6m, operationId: id);
        var receipts = await workshop.Db.FabricMovements.CountAsync(x => x.Kind == FabricMovementKind.Receipt);

        Assert.Multiple(() =>
        {
            Assert.That(first.Value.Replayed, Is.False);
            Assert.That(repeat.Value, Has.Property("Replayed").True.And.Property("Meters").EqualTo(15m), "A22");
            Assert.That(other.Status, Is.EqualTo(ResultStatus.Conflict), "A23");
            Assert.That(receipts, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task A_back_dated_movement_restates_the_chain_by_operation_date()
    {
        using var workshop = Workshop();
        await Receive(workshop, Chief, "Opening", 10m, date: new DateOnly(2026, 11, 1));
        await Receive(workshop, Chief, "Receipt", 5m, date: new DateOnly(2026, 11, 10));
        await Receive(workshop, Chief, "Receipt", 2m, date: new DateOnly(2026, 11, 3));
        var rows = (await Movements(workshop, Chief, FabricMovementFilter.All)).Rows;

        Assert.Multiple(() =>
        {
            Assert.That(rows.Select(x => (x.OperationDate.Day, x.MetersAfter)),
                Is.EqualTo(new[] { (10, 17m), (3, 12m), (1, 10m) }), "newest operation date first, chain by date");
            Assert.That(workshop.Db.FabricStocks.AsNoTracking().Single().Meters, Is.EqualTo(17m));
        });
    }

    [Test]
    public async Task Movements_are_paged_by_50_and_filtered_by_fabric_and_colour()
    {
        using var workshop = Workshop();
        await Receive(workshop, Chief, "Opening", 100m);
        await Receive(workshop, Chief, "Opening", 1m, color: "Жовтий");
        for (var i = 0; i < 50; i++) await Receive(workshop, Chief, "Receipt", 1m);

        var first = await Movements(workshop, Chief, FabricMovementFilter.All);
        var second = await Movements(workshop, Chief, FabricMovementFilter.All, page: 2);
        var yellow = await Movements(workshop, Chief, FabricMovementFilter.All, code: "FAB-067", color: " жовтий");

        Assert.Multiple(() =>
        {
            Assert.That((first.Rows.Count, first.HasMore), Is.EqualTo((50, true)));
            Assert.That((second.Rows.Count, second.HasMore), Is.EqualTo((2, false)));
            Assert.That(yellow.Rows.Single().Color, Is.EqualTo("Жовтий"), "A11 and the alias group");
        });
    }

    [Test]
    public async Task A_changed_alias_offer_cannot_be_received_as_the_same_fabric()
    {
        using var workshop = Workshop();
        workshop.Reference.Fabrics[1] = workshop.Reference.Fabrics[1] with { Density = "240" };
        var refused = await Receive(workshop, Chief, "Receipt", 1m, fabricId: 67);
        var choices = await new global::Warehouse.Application.Features.Stock.Queries.GetFabricStockChoices.GetFabricStockChoicesHandler(
            workshop.Reference, workshop.Colors()).Handle(new(Chief), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(refused.Status, Is.EqualTo(ResultStatus.Invalid));
            Assert.That(choices.Value.Fabrics.Select(x => x.Code), Does.Not.Contain("FAB-067"));
        });
    }

    [TestCase("  Білий ", "білий")]
    [TestCase("Темно  синій", "темно синій")]
    [TestCase("Бiлий", "бiлий")]
    public void Colour_key_is_nfc_trimmed_collapsed_and_lower_case(string typed, string key) =>
        Assert.That(StockColor.Key(typed), Is.EqualTo(key));

    [Test]
    public void Aliases_resolve_only_the_reviewed_codes()
    {
        Assert.Multiple(() =>
        {
            Assert.That(FabricAliases.Canonical("fab-067"), Is.EqualTo("FAB-023"));
            Assert.That(FabricAliases.Canonical("FAB-077"), Is.EqualTo("FAB-078"));
            Assert.That(FabricAliases.Canonical("FAB-024"), Is.EqualTo("FAB-024"));
            Assert.That(FabricAliases.Members("FAB-078"), Is.EqualTo(new[] { "FAB-078", "FAB-077" }));
        });
    }

    private static Task<Result<FabricStockOperationResponse>> Issue(WarehouseTestHarness workshop, decimal meters,
        string? version, string? destination = null) =>
        new IssueFabricHandler(workshop.StockChanges(), workshop.Pairs(), workshop.Operations())
            .Handle(new(Chief, new(Guid.NewGuid(), "FAB-023", "Темно-сірий", version, meters, Day, destination, null)),
                CancellationToken.None).AsTask();

    private static async Task<Result<FabricStockOperationResponse>> Adjust(WarehouseTestHarness workshop,
        decimal target, string reason)
    {
        var version = Pair(await List(workshop, Chief), "Темно-сірий").Version;
        return await new AdjustFabricStockHandler(workshop.StockChanges(), workshop.Pairs(), workshop.Operations())
            .Handle(new(Chief, new(Guid.NewGuid(), "FAB-023", "Темно-сірий", version, target, Day, reason)),
                CancellationToken.None);
    }
}
