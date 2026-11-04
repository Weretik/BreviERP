using Microsoft.EntityFrameworkCore;
using Ardalis.Result;
using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Application.Orders.Access;
using Production.Application.Orders.CopyOrder;
using Production.Application.Orders.CreateOrder;
using Production.Application.Orders.DeleteOrder;
using Production.Application.Orders.GetHistory;
using Production.Application.Orders.GetOrder;
using Production.Application.Orders.GetOrders;
using Production.Application.Orders.ImportOrders;
using Production.Application.Orders.QuoteSources;
using Production.Application.Orders.UpdateOrder;
using Production.Domain.FabricNorms;

namespace UnitTests.Production;

/// <summary>«Виробничі завдання» from КП (PRD-002).</summary>
public sealed class ProductionOrderHandlerTests
{
    private static readonly ProductionActor Admin = new(1, true, false, false);
    private static readonly ProductionActor Manager = new(2, false, true, false);
    private static readonly ProductionActor OtherManager = new(3, false, true, false);
    private static readonly ProductionActor Chief = new(4, false, false, true);

    [Test]
    public async Task Order_from_a_sent_quote_copies_model_fabric_rate_price_and_plans_fabric()
    {
        using var workshop = Workshop();

        var result = await Create(workshop, Manager, Input());

        Assert.That(result.IsSuccess, Is.True, string.Join("; ", result.Errors));
        var order = await workshop.Db.Orders.Include(x => x.Items).SingleAsync();
        var item = order.Items.Single();
        Assert.Multiple(() =>
        {
            Assert.That(order.Number, Is.EqualTo(1));
            Assert.That(order.QuoteNumber, Is.EqualTo("12-11-2026"));
            Assert.That(order.ManagerName, Is.EqualTo("Анастасія"));
            Assert.That(item.ModelCode, Is.EqualTo("MOD-052"));
            Assert.That(item.FabricCode, Is.EqualTo("FAB-023"));
            Assert.That(item.SewingRate, Is.EqualTo(343.25m));
            Assert.That(item.ReportUnitPrice, Is.EqualTo(1500m), "wholesale price of the variant");
            Assert.That(item.PlannedFabricMeters, Is.EqualTo(7.5m), "two colours: 2.2 + 0.3 per item, 3 items");
            Assert.That(result.Value.Items[0].SewingRate, Is.Null, "a manager never sees the rate");
            Assert.That(result.Value.Items[0].ReportUnitPrice, Is.Null, "only Admin sees КП prices");
        });
    }

    [Test]
    public async Task Repeated_press_returns_the_same_order()
    {
        using var workshop = Workshop();
        var input = Input();

        var first = await Create(workshop, Manager, input);
        var second = await Create(workshop, Manager, input);

        Assert.Multiple(() =>
        {
            Assert.That(second.Value.Id, Is.EqualTo(first.Value.Id));
            Assert.That(workshop.Db.Orders.Count(), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Manager_cannot_use_another_managers_quote_and_the_workshop_cannot_create()
    {
        using var workshop = Workshop();

        var other = await Create(workshop, OtherManager, Input());
        var chief = await Create(workshop, Chief, Input());

        Assert.Multiple(() =>
        {
            Assert.That(other.Status, Is.EqualTo(ResultStatus.NotFound));
            Assert.That(chief.Status, Is.EqualTo(ResultStatus.Forbidden));
        });
    }

    [Test]
    public async Task Draft_quote_is_not_passed_to_the_workshop()
    {
        using var workshop = Workshop();
        workshop.Quotes.Quotes[0] = workshop.Quotes.Quotes[0] with { IsSent = false };

        var result = await Create(workshop, Manager, Input());

        Assert.That(result.Status, Is.EqualTo(ResultStatus.Conflict));
    }

    [Test]
    public async Task Draft_shows_rates_to_admin_but_not_to_a_manager()
    {
        using var workshop = Workshop();
        var handler = new GetProductionQuoteDraftQueryHandler(workshop.Quotes, workshop.Resolver());

        var admin = await handler.Handle(new(Admin, 10), CancellationToken.None);
        var manager = await handler.Handle(new(Manager, 10), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(admin.Value.Items.Single().Options.Single().SewingRate, Is.EqualTo(343.25m));
            Assert.That(manager.Value.Items.Single().Options.Single().SewingRate, Is.Null);
            Assert.That(manager.Value.Items.Single().Norm!.TwoColorsMain, Is.EqualTo(2.2m));
        });
    }

    [Test]
    public async Task Update_records_every_changed_field_and_replans_fabric()
    {
        using var workshop = Workshop();
        var created = (await Create(workshop, Manager, Input())).Value;
        var item = created.Items.Single();
        var details = Details(5) with { AdditionalColors = [], SizesText = "M-3, L-2" };

        var updated = await new UpdateProductionOrderCommandHandler(workshop.Db, workshop.Staff, workshop.Time)
            .Handle(new(Chief, created.Id, new UpdateProductionOrderInput(created.Version, created.ClientName,
                created.OrderDate, null, [new(item.Id, details, 300m)])), CancellationToken.None);
        var history = (await new GetProductionOrderHistoryQueryHandler(workshop.Db)
            .Handle(new(created.Id), CancellationToken.None)).Value;

        Assert.Multiple(() =>
        {
            Assert.That(updated.IsSuccess, Is.True, string.Join("; ", updated.Errors));
            Assert.That(updated.Value.Items[0].PlannedFabricMeters, Is.EqualTo(12.5m), "one colour: 2.5 per item");
            Assert.That(updated.Value.Items[0].SewingRate, Is.EqualTo(300m), "the head of the workshop sets the rate");
            Assert.That(history.Select(x => x.Field), Is.SupersetOf(new[]
                { "Кількість", "Додаткові кольори", "Розміри", "Розцінка пошиття", "Створено" }));
            Assert.That(history.First(x => x.Field == "Кількість").Before, Is.EqualTo("3"));
            Assert.That(history.First(x => x.Field == "Кількість").ActorName, Is.EqualTo("Начальник"));
        });
    }

    [Test]
    public async Task Stale_save_and_other_managers_edit_are_refused()
    {
        using var workshop = Workshop();
        var created = (await Create(workshop, Manager, Input())).Value;
        var lines = created.Items.Select(x => new UpdateProductionItemInput(x.Id, Details(4), null)).ToArray();
        var handler = new UpdateProductionOrderCommandHandler(workshop.Db, workshop.Staff, workshop.Time);

        var other = await handler.Handle(new(OtherManager, created.Id,
            new(created.Version, "Інший", created.OrderDate, null, lines)), CancellationToken.None);
        var stale = await handler.Handle(new(Manager, created.Id,
            new(created.Version + 1, "Інший", created.OrderDate, null, lines)), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(other.Status, Is.EqualTo(ResultStatus.Forbidden));
            Assert.That(stale.Status, Is.EqualTo(ResultStatus.Conflict));
        });
    }

    [Test]
    public async Task Manager_cannot_change_the_rate()
    {
        using var workshop = Workshop();
        var created = (await Create(workshop, Manager, Input())).Value;
        var item = created.Items.Single();

        await new UpdateProductionOrderCommandHandler(workshop.Db, workshop.Staff, workshop.Time)
            .Handle(new(Manager, created.Id, new(created.Version, created.ClientName, created.OrderDate, null,
                [new(item.Id, Details(3), 1m)])), CancellationToken.None);

        Assert.That((await workshop.Db.Orders.Include(x => x.Items).SingleAsync()).Items.Single().SewingRate,
            Is.EqualTo(343.25m));
    }

    [Test]
    public async Task Copy_is_a_new_order_of_the_person_who_copies()
    {
        using var workshop = Workshop();
        var created = (await Create(workshop, Manager, Input())).Value;

        var copy = await new CopyProductionOrderCommandHandler(workshop.Db, workshop.Staff, workshop.Time)
            .Handle(new(Admin, created.Id, new(Guid.NewGuid())), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(copy.Value.Number, Is.EqualTo(2));
            Assert.That(copy.Value.ManagerName, Is.EqualTo("Власниця"));
            Assert.That(copy.Value.CopiedFromOrderId, Is.EqualTo(created.Id));
            Assert.That(copy.Value.Items.Single().ReportUnitPrice, Is.EqualTo(1500m));
        });
    }

    [Test]
    public async Task Only_admin_deletes_and_a_deleted_order_leaves_the_list()
    {
        using var workshop = Workshop();
        var created = (await Create(workshop, Manager, Input())).Value;
        var handler = new DeleteProductionOrderCommandHandler(workshop.Db, workshop.Staff, workshop.Time);

        var manager = await handler.Handle(new(Manager, created.Id, created.Version), CancellationToken.None);
        var admin = await handler.Handle(new(Admin, created.Id, created.Version), CancellationToken.None);
        var list = (await new GetProductionOrdersQueryHandler(workshop.Db)
            .Handle(new(null, null, null, null), CancellationToken.None)).Value;
        var detail = await new GetProductionOrderQueryHandler(workshop.Db)
            .Handle(new(Admin, created.Id), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(manager.Status, Is.EqualTo(ResultStatus.Forbidden));
            Assert.That(admin.IsSuccess, Is.True);
            Assert.That(list.Total, Is.Zero);
            Assert.That(detail.Status, Is.EqualTo(ResultStatus.NotFound));
        });
    }

    [Test]
    public async Task List_filters_by_status_search_and_due_date_and_counts_statuses()
    {
        using var workshop = Workshop();
        await Import(workshop, false, Row("a", 1, "Closed", "Геофізика"), Row("b", 2, "New", "School 5"));
        var handler = new GetProductionOrdersQueryHandler(workshop.Db);

        var newOnly = (await handler.Handle(new(null, "New", null, null), CancellationToken.None)).Value;
        var search = (await handler.Handle(new("school", null, null, null), CancellationToken.None)).Value;
        var due = (await handler.Handle(new(null, null, null, new DateOnly(2026, 11, 1)), CancellationToken.None)).Value;

        Assert.Multiple(() =>
        {
            Assert.That(newOnly.Items.Select(x => x.Number), Is.EqualTo(new[] { 2 }));
            Assert.That(newOnly.Counts, Is.EqualTo(new ProductionOrderCounts(2, 1, 1, 0)));
            Assert.That(search.Items.Single().ClientName, Is.EqualTo("School 5"), "SQLite lowers only Latin; PostgreSQL lowers Cyrillic too");
            Assert.That(due.Total, Is.Zero);
            Assert.That(newOnly.Managers, Is.EqualTo(new[] { "Анастасія" }));
        });
    }

    [Test]
    public async Task Import_preview_saves_nothing_and_a_repeat_keeps_moved_orders()
    {
        using var workshop = Workshop();

        var preview = await Import(workshop, true, Row("a", 1, "Closed", "Геофізика"));
        var applied = await Import(workshop, false, Row("a", 1, "Closed", "Геофізика"));
        var repeat = await Import(workshop, false, Row("a", 1, "Closed", "Геофізика"));
        var order = await workshop.Db.Orders.Include(x => x.Items).SingleAsync();

        Assert.Multiple(() =>
        {
            Assert.That(preview.Value.DryRun, Is.True);
            Assert.That(applied.Value.Created, Is.EqualTo(1));
            Assert.That(repeat.Value.Unchanged, Is.EqualTo(1));
            Assert.That(order.ManagerUserId, Is.EqualTo(2), "manager found by email");
            Assert.That(order.Items.Single().PlannedFabricMeters, Is.EqualTo(7.5m));
        });
    }

    [Test]
    public async Task Import_with_a_taken_number_saves_nothing()
    {
        using var workshop = Workshop();
        await Create(workshop, Manager, Input());

        var result = await Import(workshop, false, Row("a", 1, "New", "Геофізика"), Row("b", 2, "New", "Інший"));

        Assert.Multiple(() =>
        {
            Assert.That(result.Value.Errors, Is.EqualTo(1));
            Assert.That(result.Value.DryRun, Is.True);
            Assert.That(workshop.Db.Orders.Count(), Is.EqualTo(1));
        });
    }

    private static ProductionTestHarness Workshop()
    {
        var workshop = new ProductionTestHarness();
        workshop.Staff.Staff.AddRange([
            new(1, "Власниця", "owner@brevi.ua"), new(2, "Анастасія", "anastasia@brevi.ua"),
            new(3, "Інший менеджер", null), new(4, "Начальник", null)
        ]);
        workshop.Fabrics.Fabrics.Add(new(23, "FAB-023", "Тканина «Грета»"));
        workshop.Models.Costs[5] = new(5, "MOD-052", new Dictionary<long, decimal?> { [77] = 343.254m });
        workshop.Db.FabricNorms.Add(new ModelFabricNorm("MOD-052",
            new FabricNormValues(2.5m, 2.2m, 0.3m, null, null, null), 1, workshop.Time.GetUtcNow()));
        workshop.Db.SaveChanges();
        workshop.Quotes.Quotes.Add(new ProductionQuote(10, "12-11-2026", true, false, "Геофізика", 9, 2, "Анастасія",
        [
            new ProductionQuoteItem(100, 5, "MOD-052", "Куртка «Віол»",
                [new ProductionQuoteOption(1000, 77, 23, "Грета", 1800m, 1500m, 1300m)])
        ]));
        return workshop;
    }

    private static ProductionItemDetailsInput Details(int quantity) => new(quantity, new DateOnly(2026, 11, 19),
        "Темно-сірий", ["Червоний"], null, "Світловідбивна стрічка", null,
        [new ItemApplicationInput("Embroidery", "Спина", "Логотип", "Помаранчевий")]);

    private static CreateProductionOrderInput Input() => new(Guid.NewGuid(), 10, "Геофізика",
        new DateOnly(2026, 11, 4), null, "Wholesale", [new CreateProductionItemInput(100, 1000, Details(3))]);

    private static Task<Result<ProductionOrderResponse>> Create(ProductionTestHarness workshop, ProductionActor actor,
        CreateProductionOrderInput input) =>
        new CreateProductionOrderCommandHandler(workshop.Db, workshop.Quotes, workshop.Staff, workshop.Resolver(),
            workshop.Time).Handle(new(actor, input), CancellationToken.None).AsTask();

    private static ProductionOrderImportRow Row(string id, int number, string status, string client) => new(id, number,
        "27-10-2026", client, "anastasia@brevi.ua", "Анастасія", status, new DateOnly(2026, 10, 9), null,
        [new ProductionItemImportRow("MOD-052", "Куртка «Віол»", "FAB-023", "Грета", 343.25m, Details(3))]);

    private static Task<Result<ProductionOrderImportResult>> Import(ProductionTestHarness workshop, bool dryRun,
        params ProductionOrderImportRow[] rows) =>
        new ImportProductionOrdersCommandHandler(workshop.Db, workshop.Staff, workshop.Time)
            .Handle(new(Admin, rows, dryRun), CancellationToken.None).AsTask();
}
