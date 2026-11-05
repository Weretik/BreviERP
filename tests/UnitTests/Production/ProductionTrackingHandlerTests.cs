using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Production.Application.Contracts;
using Production.Application.Orders.Access;
using Production.Application.Orders.Files;
using Production.Application.Orders.ImportOrders;
using Production.Application.Orders.ImportProgress;
using Production.Application.Orders.ImportReportPrices;
using Production.Application.Orders.Telegram;
using Production.Application.Orders.Tracking;
using Production.Application.Settings;

namespace UnitTests.Production;

/// <summary>Етапи, відвантаження, status, files and the Telegram message of an order (PRD-003).</summary>
public sealed class ProductionTrackingHandlerTests
{
    private static readonly ProductionActor Admin = new(1, true, false, false);
    private static readonly ProductionActor Manager = new(2, false, true, false);
    private static readonly ProductionActor Chief = new(4, false, false, true);

    [Test]
    public async Task Stages_keep_their_order_and_the_furthest_one_is_current()
    {
        using var workshop = ProductionOrderHandlerTests.Workshop();
        var order = await Order(workshop);
        var itemId = order.Items[0].Id;

        var byManager = await Stages(workshop, Manager, order, itemId, "Cutting");
        var byChief = await Stages(workshop, Chief, order, itemId, "Sewing", "FabricAvailable", "Sewing");
        var history = await workshop.Db.OrderChanges.Where(x => x.Field == "Етапи").ToListAsync();

        Assert.Multiple(() =>
        {
            Assert.That(byManager.Status, Is.EqualTo(ResultStatus.Forbidden));
            Assert.That(byChief.Value.Items[0].Stages, Is.EqualTo(new[] { "FabricAvailable", "Sewing" }));
            Assert.That(byChief.Value.Items[0].CurrentStage, Is.EqualTo("Sewing"));
            Assert.That(history.Single().Before, Is.EqualTo("Нове"));
        });
    }

    [Test]
    public async Task Shipments_never_exceed_the_quantity_and_a_full_one_ticks_shipped()
    {
        using var workshop = ProductionOrderHandlerTests.Workshop();
        var order = await Order(workshop);
        var itemId = order.Items[0].Id;
        var ship = new CreateProductionShipmentCommandHandler(workshop.Tracking());

        var part = await ship.Handle(new(Chief, order.Id, itemId, new(order.Version, 2, new DateOnly(2026, 11, 5), "Перша партія")),
            CancellationToken.None);
        var tooMuch = await ship.Handle(new(Chief, order.Id, itemId, new(part.Value.Version, 2, new DateOnly(2026, 11, 6), null)),
            CancellationToken.None);
        var rest = await ship.Handle(new(Chief, order.Id, itemId, new(part.Value.Version, 1, new DateOnly(2026, 11, 6), null)),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(part.Value.Items[0].ShippedQuantity, Is.EqualTo(2));
            Assert.That(part.Value.Items[0].Stages, Does.Not.Contain("Shipped"));
            Assert.That(tooMuch.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(rest.Value.Items[0].ShippedQuantity, Is.EqualTo(3));
            Assert.That(rest.Value.Items[0].CurrentStage, Is.EqualTo("Shipped"));
            Assert.That(rest.Value.Permissions.CanClose, Is.True);
        });
    }

    [Test]
    public async Task Cancelled_shipment_returns_its_quantity()
    {
        using var workshop = ProductionOrderHandlerTests.Workshop();
        var order = await Order(workshop);
        var shipped = await new CreateProductionShipmentCommandHandler(workshop.Tracking()).Handle(
            new(Chief, order.Id, order.Items[0].Id, new(order.Version, 1, new DateOnly(2026, 11, 5), null)),
            CancellationToken.None);

        var cancelled = await new CancelProductionShipmentCommandHandler(workshop.Tracking()).Handle(
            new(Chief, order.Id, shipped.Value.Items[0].Shipments[0].Id, shipped.Value.Version), CancellationToken.None);

        Assert.That(cancelled.Value.Items[0].ShippedQuantity, Is.Zero);
    }

    [Test]
    public async Task Quantity_cannot_drop_below_what_is_shipped()
    {
        using var workshop = ProductionOrderHandlerTests.Workshop();
        var order = await Order(workshop);
        var shipped = await new CreateProductionShipmentCommandHandler(workshop.Tracking()).Handle(
            new(Chief, order.Id, order.Items[0].Id, new(order.Version, 2, new DateOnly(2026, 11, 5), null)),
            CancellationToken.None);

        var updated = await new global::Production.Application.Orders.UpdateOrder.UpdateProductionOrderCommandHandler(
                workshop.Db, workshop.Staff, workshop.Cards(), workshop.EmbroideryLinks(), workshop.Time)
            .Handle(new(Admin, order.Id, new(shipped.Value.Version, order.ClientName, order.OrderDate, null,
                [new UpdateProductionItemInput(order.Items[0].Id, ProductionOrderHandlerTests.Details(1), null)])),
                CancellationToken.None);

        Assert.That(updated.Status, Is.EqualTo(ResultStatus.Invalid));
    }

    [Test]
    public async Task Close_needs_everything_shipped_cancel_needs_a_reason_and_only_admin_reopens()
    {
        using var workshop = ProductionOrderHandlerTests.Workshop();
        var order = await Order(workshop);
        var status = new ProductionOrderStatusCommandHandler(workshop.Tracking());

        var earlyClose = await status.Handle(new CloseProductionOrderCommand(Chief, order.Id, order.Version),
            CancellationToken.None);
        var cancelled = await status.Handle(new CancelProductionOrderCommand(Manager, order.Id,
            new(order.Version, "Клієнт відмовився")), CancellationToken.None);
        var reopenByManager = await status.Handle(new ReopenProductionOrderCommand(Manager, order.Id,
            cancelled.Value.Version), CancellationToken.None);
        var reopened = await status.Handle(new ReopenProductionOrderCommand(Admin, order.Id, cancelled.Value.Version),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(earlyClose.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(cancelled.Value.Status, Is.EqualTo("Cancelled"));
            Assert.That(cancelled.Value.CancelReason, Is.EqualTo("Клієнт відмовився"));
            Assert.That(reopenByManager.Status, Is.EqualTo(ResultStatus.Forbidden));
            Assert.That(reopened.Value.Status, Is.EqualTo("New"));
            Assert.That(reopened.Value.CancelReason, Is.Null);
        });
    }

    [Test]
    public async Task Files_go_to_the_order_folder_and_a_deleted_one_leaves_the_cdn()
    {
        using var workshop = ProductionOrderHandlerTests.Workshop();
        var order = await Order(workshop);
        await using var content = new MemoryStream([1, 2, 3]);

        var uploaded = await new UploadProductionOrderFileCommandHandler(workshop.Db, workshop.Staff, workshop.Files,
                workshop.Cards(), workshop.Time)
            .Handle(new(Manager, order.Id, order.Items[0].Id, "sizes", content, "C:\\docs\\Розміри.xlsx",
                "application/vnd.ms-excel", 3), CancellationToken.None);
        var file = uploaded.Value.Files.Single();
        var deleted = await new DeleteProductionOrderFileCommandHandler(workshop.Db, workshop.Staff, workshop.Files,
                workshop.Cards(), workshop.Time, NullLogger<DeleteProductionOrderFileCommandHandler>.Instance)
            .Handle(new(Manager, order.Id, file.Id), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(file.Name, Is.EqualTo("Розміри.xlsx"));
            Assert.That(file.Kind, Is.EqualTo("Sizes"));
            Assert.That(file.ItemId, Is.EqualTo(order.Items[0].Id));
            Assert.That(workshop.Files.Stored.Single(), Does.StartWith($"/production/orders/{order.Number}/").And.EndWith(".xlsx"));
            Assert.That(deleted.Value.Files, Is.Empty);
            Assert.That(workshop.Files.Deleted, Is.EqualTo(workshop.Files.Stored));
        });
    }

    [Test]
    public async Task Telegram_message_is_sent_once_and_a_failure_can_be_retried()
    {
        using var workshop = ProductionOrderHandlerTests.Workshop();
        var order = await Order(workshop);
        var send = SendTelegram(workshop);

        var notReady = await send.Handle(new(Manager, order.Id), CancellationToken.None);
        await Settings(workshop).Handle(new UpdateWorkshopSettingsCommand(Chief, new(true, -100500, 434)),
            CancellationToken.None);
        workshop.Telegram.FailWith = "Bad Request: chat not found";
        var failed = await send.Handle(new(Manager, order.Id), CancellationToken.None);
        workshop.Telegram.FailWith = null;
        var sent = await send.Handle(new(Manager, order.Id), CancellationToken.None);
        var again = await send.Handle(new(Manager, order.Id), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(notReady.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(failed.Value.Telegram.Status, Is.EqualTo("Failed"));
            Assert.That(failed.Value.Telegram.Error, Does.Contain("chat not found"));
            Assert.That(failed.Value.Permissions.CanSendTelegram, Is.True);
            Assert.That(sent.Value.Telegram.Status, Is.EqualTo("Sent"));
            Assert.That(sent.Value.Permissions.CanSendTelegram, Is.False);
            Assert.That(again.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(workshop.Telegram.Sent, Has.Count.EqualTo(1));
            Assert.That(workshop.Telegram.Sent[0].ThreadId, Is.EqualTo(434));
            Assert.That(workshop.Telegram.Sent[0].Html, Does.Contain($"Ц-{order.Number:D5}").Or.Contain(order.Number.ToString()));
            Assert.That(workshop.Telegram.Sent[0].Html, Does.Contain($"https://office.test/production/orders/{order.Id}"));
        });
    }

    [Test]
    public async Task Settings_belong_to_the_chief_and_admin_and_list_the_bot_groups()
    {
        using var workshop = ProductionOrderHandlerTests.Workshop();
        workshop.Telegram.Chats.Add(new(-100500, "Цех Brevi"));
        var settings = Settings(workshop);

        var byManager = await settings.Handle(new GetWorkshopSettingsQuery(Manager), CancellationToken.None);
        var saved = await settings.Handle(new UpdateWorkshopSettingsCommand(Admin, new(false, -100500, null)),
            CancellationToken.None);
        var test = await settings.Handle(new SendWorkshopTelegramTestCommand(Chief), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(byManager.Status, Is.EqualTo(ResultStatus.Forbidden));
            Assert.That(saved.Value.TelegramEnabled, Is.False);
            Assert.That(saved.Value.Chats.Single().Title, Is.EqualTo("Цех Brevi"));
            Assert.That(saved.Value.UpdatedByName, Is.EqualTo("Власниця"));
            Assert.That(test.IsSuccess, Is.True);
            Assert.That(workshop.Telegram.Sent.Single().ChatId, Is.EqualTo(-100500));
        });
    }

    [Test]
    public async Task Progress_of_old_orders_moves_stages_file_links_and_the_telegram_mark_once()
    {
        using var workshop = ProductionOrderHandlerTests.Workshop();
        await new ImportProductionOrdersCommandHandler(workshop.Db, workshop.Staff, workshop.Time).Handle(new(Admin,
        [
            new ProductionOrderImportRow("old-1", 7, "27-10-2026", "Геофізика", "anastasia@brevi.ua", "Анастасія", "New",
                new DateOnly(2026, 10, 9), null,
                [new ProductionItemImportRow("MOD-052", "Куртка «Віол»", "FAB-023", "Грета", 343.25m,
                    ProductionOrderHandlerTests.Details(3))])
        ], false), CancellationToken.None);
        ProductionProgressImportRow[] rows =
        [
            new("old-1", [new(1, ["FabricAvailable", "Cutting"])],
                [new(1, "Sizes", "Розміри.pdf", "application/pdf", 1200, "https://drive.google.com/file/d/abc",
                    new DateTimeOffset(2026, 10, 10, 9, 0, 0, TimeSpan.Zero), null)],
                true, new DateTimeOffset(2026, 10, 10, 9, 5, 0, TimeSpan.Zero)),
            new("missing", [], [], false, null)
        ];
        var import = new ImportProductionProgressCommandHandler(workshop.Db, workshop.Time);

        var withError = await import.Handle(new(Admin, rows, false), CancellationToken.None);
        var preview = await import.Handle(new(Admin, rows[..1], true), CancellationToken.None);
        var applied = await import.Handle(new(Admin, rows[..1], false), CancellationToken.None);
        var repeat = await import.Handle(new(Admin, rows[..1], false), CancellationToken.None);
        var order = await workshop.Db.Orders.Include(x => x.Items).SingleAsync();
        var file = await workshop.Db.OrderFiles.SingleAsync();
        var delivery = await workshop.Db.TelegramDeliveries.SingleAsync();

        Assert.Multiple(() =>
        {
            Assert.That(withError.Value.Errors, Is.EqualTo(1));
            Assert.That(withError.Value.DryRun, Is.True);
            Assert.That(preview.Value.Lines.Single().Action, Is.EqualTo("updated"));
            Assert.That(applied.Value.Updated, Is.EqualTo(1));
            Assert.That(repeat.Value.Unchanged, Is.EqualTo(1));
            Assert.That(order.Items[0].CurrentStage.ToString(), Is.EqualTo("Cutting"));
            Assert.That(file.StorageKey, Is.Null);
            Assert.That(delivery.Status.ToString(), Is.EqualTo("Sent"));
        });
    }

    [Test]
    public async Task Old_orders_take_the_kp_price_once_and_an_item_with_a_price_keeps_it()
    {
        using var workshop = ProductionOrderHandlerTests.Workshop();
        await new ImportProductionOrdersCommandHandler(workshop.Db, workshop.Staff, workshop.Time).Handle(new(Admin,
        [
            new ProductionOrderImportRow("old-1", 7, "27-10-2026", "Геофізика", "anastasia@brevi.ua", "Анастасія", "New",
                new DateOnly(2026, 10, 9), null,
                [
                    new ProductionItemImportRow("MOD-052", "Куртка «Віол»", "FAB-023", "Грета", 343.25m,
                        ProductionOrderHandlerTests.Details(3)),
                    new ProductionItemImportRow("MOD-064", "Напівкомбінезон", "FAB-062", "Тритон", 233.73m,
                        ProductionOrderHandlerTests.Details(2))
                ])
        ], false), CancellationToken.None);
        ProductionReportPriceImportRow[] rows = [new("old-1", [new(1, 1713.17m), new(2, 1282.54m)])];
        var import = new ImportProductionReportPricesCommandHandler(workshop.Db);

        var byManager = await import.Handle(new(Manager, rows, true), CancellationToken.None);
        var noPosition = await import.Handle(new(Admin, [new("old-1", [new(3, 10m)])], false), CancellationToken.None);
        var preview = await import.Handle(new(Admin, rows, true), CancellationToken.None);
        var unpriced = (await workshop.Db.Orders.Include(x => x.Items).AsNoTracking().SingleAsync()).Items
            .Count(x => x.ReportUnitPrice is null);
        var applied = await import.Handle(new(Admin, rows, false), CancellationToken.None);
        var changed = await import.Handle(new(Admin, [new("old-1", [new(1, 1m)])], false), CancellationToken.None);
        var invalid = new ImportProductionReportPricesCommandValidator()
            .Validate(new ImportProductionReportPricesCommand(Admin, [new("old-1", [new(1, 0m)])], false));
        var items = (await workshop.Db.Orders.Include(x => x.Items).AsNoTracking().SingleAsync()).Items
            .OrderBy(x => x.Position).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(byManager.Status, Is.EqualTo(ResultStatus.Forbidden));
            Assert.That(noPosition.Value.Errors, Is.EqualTo(1));
            Assert.That(noPosition.Value.Lines.Single().Error, Does.Contain("позиції 3"));
            Assert.That(preview.Value.Lines.Single().Prices, Is.EqualTo(2));
            Assert.That(unpriced, Is.EqualTo(2), "a preview changes nothing");
            Assert.That(applied.Value.Updated, Is.EqualTo(1));
            Assert.That(changed.Value.Unchanged, Is.EqualTo(1), "an item with a price keeps it");
            Assert.That(invalid.IsValid, Is.False);
            Assert.That(items.Select(x => x.ReportUnitPrice), Is.EqualTo(new decimal?[] { 1713.17m, 1282.54m }));
        });
    }

    private static async Task<ProductionOrderResponse> Order(ProductionTestHarness workshop) =>
        (await ProductionOrderHandlerTests.Create(workshop, Manager, ProductionOrderHandlerTests.Input())).Value;

    private static async Task<Result<ProductionOrderResponse>> Stages(ProductionTestHarness workshop,
        ProductionActor actor, ProductionOrderResponse order, int itemId, params string[] stages) =>
        await new SetProductionItemStagesCommandHandler(workshop.Tracking())
            .Handle(new(actor, order.Id, itemId, new(order.Version, stages)), CancellationToken.None);

    private static SendProductionOrderTelegramCommandHandler SendTelegram(ProductionTestHarness workshop) =>
        new(workshop.Db, workshop.Staff, workshop.Telegram, workshop.Links, workshop.Cards(), workshop.Time,
            NullLogger<SendProductionOrderTelegramCommandHandler>.Instance);

    private static WorkshopSettingsHandler Settings(ProductionTestHarness workshop) =>
        new(workshop.Db, workshop.Staff, workshop.Telegram, workshop.Time);
}
