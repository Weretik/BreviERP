using Ardalis.Result;
using static UnitTests.Warehouse.WarehouseStockHandlerTests;
using Warehouse.Application.Features.Stock.Commands.IssueFabric;
using Warehouse.Application.Features.Stock.Queries.GetFabricMovements;
using Warehouse.Application.Features.Stock.Shared;

namespace UnitTests.Warehouse;

/// <summary>«Рух тканини» search, period, kind chips and pair balance (WH-007).</summary>
public sealed class WarehouseMovementJournalTests
{
    private static readonly WarehouseActor Chief = new(4, false, false, true);

    [Test]
    public async Task The_pair_balance_follows_every_code_of_the_fabric_in_ledger_order()
    {
        using var workshop = Workshop();
        await Seed(workshop);
        var rows = (await Page(workshop, new(Chief, 1, FabricMovementFilter.All, null, null, null))).Rows;

        Assert.Multiple(() =>
        {
            Assert.That(rows.Select(x => (x.Kind, x.PairMetersAfter)),
                Is.EqualTo(new (string, decimal?)[] { ("Issue", 12.5m), ("Receipt", 15.5m), ("Opening", 10m) }));
            Assert.That(rows[1].FabricCode, Is.EqualTo("FAB-067"), "the receipt is on the alias line");
        });
    }

    [Test]
    public async Task Counts_follow_the_search_and_period_but_not_the_kind()
    {
        using var workshop = Workshop();
        await Seed(workshop);
        var all = await Page(workshop, new(Chief, 1, FabricMovementFilter.OneOffIssue, null, null, null));
        var supplier = await Page(workshop,
            new(Chief, 1, FabricMovementFilter.All, null, null, null, "  ЧЕРКАСИ  ", null, null));
        var period = await Page(workshop, new(Chief, 1, FabricMovementFilter.All, null, null, null, null,
            new DateOnly(2026, 11, 5), new DateOnly(2026, 11, 5)));
        var words = await Page(workshop,
            new(Chief, 1, FabricMovementFilter.All, null, null, null, "зразки грета", null, null));

        Assert.Multiple(() =>
        {
            Assert.That(all.Rows.Single().Destination, Is.EqualTo("Зразки"));
            Assert.That(all.Total, Is.EqualTo(1));
            Assert.That(all.Counts, Is.EqualTo(new FabricMovementCountsResponse(3, 1, 0, 1, 0, 1, 0)));
            Assert.That(supplier.Rows.Single().Kind, Is.EqualTo("Receipt"));
            Assert.That(supplier.Counts!.All, Is.EqualTo(1));
            Assert.That(period.Rows.Single().Kind, Is.EqualTo("Receipt"));
            Assert.That(words.Rows.Single().Kind, Is.EqualTo("Issue"), "every word must be found");
        });
    }

    [Test]
    public void A_period_must_not_end_before_it_starts()
    {
        var result = new GetFabricMovementsValidator().Validate(new GetFabricMovementsQuery(Chief, 1,
            FabricMovementFilter.All, null, null, null, null, new DateOnly(2026, 11, 6), new DateOnly(2026, 11, 5)));
        Assert.That(result.IsValid, Is.False);
    }

    private static async Task Seed(WarehouseTestHarness workshop)
    {
        await Receive(workshop, Chief, "Opening", 10m, date: new DateOnly(2026, 11, 4));
        await Receive(workshop, Chief, "Receipt", 5.5m, fabricId: 67, supplier: "Черкаси ВВ",
            date: new DateOnly(2026, 11, 5));
        var version = Pair(await List(workshop, Chief), "Темно-сірий").Version;
        var issued = await new IssueFabricHandler(workshop.StockChanges(), workshop.Pairs(), workshop.Operations())
            .Handle(new(Chief, new(Guid.NewGuid(), "FAB-023", "Темно-сірий", version, 3m, new DateOnly(2026, 11, 6),
                "Зразки", null)), CancellationToken.None);
        Assert.That(issued.Status, Is.EqualTo(ResultStatus.Ok));
    }

    private static async Task<FabricMovementPageResponse> Page(WarehouseTestHarness workshop,
        GetFabricMovementsQuery query) =>
        (await new GetFabricMovementsHandler(workshop.Db).Handle(query, CancellationToken.None)).Value;
}
