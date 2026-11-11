using Ardalis.Result;
using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Application.Orders.Access;
using Production.Application.Orders.GetOrders;
using Production.Application.Orders.ImportOrders;
using Production.Domain.Orders;

namespace UnitTests.Production;

/// <summary>«Фільтри» and «Зведення за моделями» of «Виробничі завдання» (PRD-012).</summary>
public sealed class ProductionOrderFilterTests
{
    private static readonly ProductionActor Admin = new(1, true, false, false);

    [Test]
    public async Task List_filters_by_client_period_model_fabric_colour_and_season()
    {
        using var workshop = await Seeded();
        var handler = new GetProductionOrdersQueryHandler(workshop.Db, workshop.Models);

        async Task<int[]> Numbers(ProductionOrderFilters filters) =>
            (await handler.Handle(new(null, null, null, null, Filters: filters), CancellationToken.None)).Value
            .Items.Select(x => x.Number).Order().ToArray();

        var client = await Numbers(new(Client: "Школа"));
        var period = await Numbers(new(From: new DateOnly(2026, 10, 5), To: new DateOnly(2026, 10, 15)));
        var model = await Numbers(new(Model: "mod-060"));
        var fabric = await Numbers(new(Fabric: "FAB-024"));
        var color = await Numbers(new(Color: "  жовтий "));
        var additional = await Numbers(new(Color: "червоний"));
        var season = await Numbers(new(Season: "Зима"));
        var both = await Numbers(new(Model: "MOD-060", Season: "Зима"));

        Assert.Multiple(() =>
        {
            Assert.That(client, Is.EqualTo(new[] { 2 }));
            Assert.That(period, Is.EqualTo(new[] { 2 }));
            Assert.That(model, Is.EqualTo(new[] { 2, 3 }));
            Assert.That(fabric, Is.EqualTo(new[] { 3 }));
            Assert.That(color, Is.EqualTo(new[] { 2 }));
            Assert.That(additional, Is.EqualTo(new[] { 1, 3 }), "additional colour counts");
            Assert.That(season, Is.EqualTo(new[] { 1 }));
            Assert.That(both, Is.Empty);
        });
    }

    [Test]
    public async Task Summary_groups_every_filtered_item_by_model_regardless_of_page()
    {
        using var workshop = await Seeded();
        var handler = new GetProductionModelSummaryQueryHandler(workshop.Db, workshop.Models);

        var all = (await handler.Handle(new(new(null, null, null, null, PageSize: 1)), CancellationToken.None)).Value;
        var cancelled = (await handler.Handle(new(new(null, "Cancelled", null, null)), CancellationToken.None)).Value;

        Assert.Multiple(() =>
        {
            Assert.That(all.Rows.Select(x => ((string?)x.ModelCode, x.Season, x.Category, x.Quantity, x.OrdersCount)), Is.EqualTo(new[]
            {
                ((string?)"MOD-060", (string?)"Літо", "Костюм куртка + брюки", 9, 2),
                ((string?)"MOD-052", (string?)"Зима", "Куртка", 3, 1),
            }));
            Assert.That(all.TotalQuantity, Is.EqualTo(12));
            Assert.That(cancelled.Rows.Single().ModelCode, Is.EqualTo("MOD-060"));
            Assert.That(cancelled.TotalQuantity, Is.EqualTo(5));
        });
    }

    [Test]
    public async Task Filter_options_list_values_used_by_orders()
    {
        using var workshop = await Seeded();

        var options = (await new GetProductionOrderFilterOptionsQueryHandler(workshop.Db, workshop.Models)
            .Handle(new(), CancellationToken.None)).Value;

        Assert.Multiple(() =>
        {
            Assert.That(options.Clients, Is.EquivalentTo(new[] { "Геофізика", "Школа", "Завод" }));
            Assert.That(options.Models.Select(x => x.Code), Is.EqualTo(new[] { "MOD-052", "MOD-060" }));
            Assert.That(options.Fabrics.Select(x => x.Code), Is.EqualTo(new[] { "FAB-023", "FAB-024" }));
            Assert.That(options.Colors, Is.EquivalentTo(new[] { "Темно-сірий", "Червоний", "Жовтий" }));
            Assert.That(options.Seasons, Is.EquivalentTo(new[] { "Зима", "Літо" }));
        });
    }

    [Test]
    public async Task Period_end_before_start_is_rejected()
    {
        var result = await new GetProductionOrdersQueryValidator().ValidateAsync(new GetProductionOrdersQuery(null,
            null, null, null, Filters: new(From: new DateOnly(2026, 11, 2), To: new DateOnly(2026, 11, 1))));

        Assert.That(result.Errors.Single().ErrorMessage, Does.Contain("«Період до»"));
    }

    [TestCase("Куртка «Віол»", "Куртка")]
    [TestCase("Костюм «Майстер» куртка + брюки", "Костюм куртка + брюки")]
    [TestCase("Куртка + напівкомбінезон «Зима»", "Костюм куртка + напівкомбінезон")]
    [TestCase("Напівкомбінезон утеплений", "Напівкомбінезон")]
    [TestCase("Брюки робочі", "Штани")]
    [TestCase("Жилет сигнальний", "Жилет")]
    [TestCase("Фліска", "Фліска")]
    [TestCase("Футболка", ModelCategory.Other)]
    public void Category_is_read_from_the_model_name(string name, string category) =>
        Assert.That(ModelCategory.Of(name), Is.EqualTo(category));

    private static async Task<ProductionTestHarness> Seeded()
    {
        var workshop = ProductionOrderHandlerTests.Workshop();
        workshop.Models.Models.AddRange([
            new("MOD-052", "Куртка «Віол»", true, 5, "Зима"),
            new("MOD-060", "Костюм «Літо» куртка + брюки", true, 6, "Літо"),
        ]);
        var details = ProductionOrderHandlerTests.Details(3);
        var result = await new ImportProductionOrdersCommandHandler(workshop.Db, workshop.Staff, workshop.Time)
            .Handle(new(Admin,
            [
                Row("a", 1, "New", "Геофізика", new DateOnly(2026, 10, 1),
                    new("MOD-052", "Куртка «Віол»", "FAB-023", "Грета", 300m, details)),
                Row("b", 2, "New", "Школа", new DateOnly(2026, 10, 10),
                    new("MOD-060", "Костюм «Літо» куртка + брюки", "FAB-023", "Грета", 500m,
                        details with { Quantity = 4, MainColor = "Жовтий", AdditionalColors = [] })),
                Row("c", 3, "Cancelled", "Завод", new DateOnly(2026, 10, 20),
                    new("MOD-060", "Костюм «Літо» куртка + брюки", "FAB-024", "Оксфорд", 500m,
                        details with { Quantity = 5 })),
            ], false), CancellationToken.None);
        Assert.That(result.IsSuccess, Is.True, string.Join("; ", result.ValidationErrors.Select(x => x.ErrorMessage)));
        return workshop;
    }

    private static ProductionOrderImportRow Row(string id, int number, string status, string client, DateOnly date,
        ProductionItemImportRow item) => new(id, number, "27-10-2026", client, "anastasia@brevi.ua", "Анастасія",
        status, date, null, [item]);
}
