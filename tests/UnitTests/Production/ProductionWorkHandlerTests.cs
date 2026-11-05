using Ardalis.Result;
using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Application.Forecast;
using Production.Application.Forecast.GetForecast;
using Production.Application.Forecast.SaveQueue;
using Production.Application.Orders.Access;
using Production.Application.PayrollParameters;
using Production.Application.Work;
using Production.Application.Work.GetWorkLog;
using Production.Application.Work.ImportWork;
using Production.Application.Work.RecordWork;
using Production.Domain.Forecast;
using Production.Domain.Staff;

namespace UnitTests.Production;

/// <summary>«Журнал робіт» and «Прогноз пошиття» (PRD-005).</summary>
public sealed class ProductionWorkHandlerTests
{
    private static readonly ProductionActor Admin = new(1, true, false, false);
    private static readonly ProductionActor Manager = new(2, false, true, false);
    private static readonly ProductionActor Chief = new(4, false, false, true);
    private static readonly DateOnly Day = new(2026, 11, 4);

    [Test]
    public async Task Work_within_the_plan_is_limited_and_over_plan_needs_a_reason()
    {
        using var workshop = Workshop(out var employeeId);
        var order = await Order(workshop);
        var itemId = order.Items[0].Id;
        var sewn = await Record(workshop, Chief, order.Id, itemId, new(employeeId, "Sewing", 2, Day, false, null, null));
        var tooMany = await Record(workshop, Chief, order.Id, itemId, new(employeeId, "Sewing", 2, Day, false, null, null));
        var over = await Record(workshop, Chief, order.Id, itemId, new(employeeId, "Sewing", 2, Day, true, "Запас", null));
        var cut = await Record(workshop, Chief, order.Id, itemId, new(employeeId, "Cutting", 3, Day, false, null, null));
        var future = await Record(workshop, Chief, order.Id, itemId,
            new(employeeId, "Sewing", 1, Day.AddDays(1), false, null, null));
        var byManager = await Record(workshop, Manager, order.Id, itemId, new(employeeId, "Sewing", 1, Day, false, null, null));

        Assert.Multiple(() =>
        {
            Assert.That(sewn.Value.Sewn, Is.EqualTo(2));
            Assert.That(sewn.Value.Rows[0].Rate, Is.EqualTo(343.25m));
            Assert.That(tooMany.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(tooMany.Errors.Single(), Does.Contain("1 шт."));
            Assert.That(over.Value.Sewn, Is.EqualTo(2), "over-plan work does not count to the plan");
            Assert.That(over.Value.Rows, Has.Count.EqualTo(2));
            Assert.That(cut.Value.Cut, Is.EqualTo(3));
            Assert.That(future.Status, Is.EqualTo(ResultStatus.Invalid));
            Assert.That(byManager.Status, Is.EqualTo(ResultStatus.Forbidden));
        });
    }

    [Test]
    public async Task Void_keeps_the_record_and_frees_the_plan()
    {
        using var workshop = Workshop(out var employeeId);
        var order = await Order(workshop);
        var itemId = order.Items[0].Id;
        var sewn = await Record(workshop, Chief, order.Id, itemId, new(employeeId, "Sewing", 3, Day, false, null, null));
        var workId = sewn.Value.Rows[0].Id;
        var voided = await Void(workshop, Chief, workId, "Помилка");
        var twice = await Void(workshop, Chief, workId, "Ще раз");
        var again = await Record(workshop, Chief, order.Id, itemId, new(employeeId, "Sewing", 3, Day, false, null, null));
        var log = await new GetWorkLogQueryHandler(workshop.Db)
            .Handle(new(Admin, "2026-11", employeeId, null), CancellationToken.None);
        var managerLog = await new GetWorkLogQueryHandler(workshop.Db)
            .Handle(new(Manager, "2026-11", null, null), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(voided.Value.Sewn, Is.EqualTo(0));
            Assert.That(voided.Value.Rows.Single().VoidReason, Is.EqualTo("Помилка"));
            Assert.That(twice.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(again.Value.Sewn, Is.EqualTo(3));
            Assert.That(log.Value.Rows, Has.Count.EqualTo(2));
            Assert.That(log.Value.Totals.Sewn, Is.EqualTo(3));
            Assert.That(managerLog.Status, Is.EqualTo(ResultStatus.Forbidden));
        });
    }

    [Test]
    public async Task Import_moves_old_work_once_and_stops_on_an_unknown_worker()
    {
        using var workshop = Workshop(out _);
        var order = await Order(workshop);
        workshop.Db.ChangeTracker.Clear();
        var tracked = workshop.Db.Orders.Single(x => x.Id == order.Id);
        typeof(global::Production.Domain.Orders.ProductionOrder).GetProperty("LegacyId")!.SetValue(tracked, "old-1");
        await workshop.Db.SaveChangesAsync();
        ProductionWorkImportRow Row(string id, string name, DateTimeOffset? voidedAt = null) => new(id, "old-1", 1, name,
            "Sewing", 1, false, null, null, 196.62m, Day, workshop.Time.GetUtcNow(), "a@b.c", voidedAt,
            voidedAt is null ? null : "видалити");
        var handler = new ImportProductionWorkCommandHandler(workshop.Db);

        var withError = await handler.Handle(new(Admin, [Row("w1", "андреева"), Row("w2", "Невідома")], false),
            CancellationToken.None);
        var preview = await handler.Handle(new(Admin, [Row("w1", "Андреева"), Row("w2", "Андреева", Day.ToDateTime(default))], true),
            CancellationToken.None);
        var applied = await handler.Handle(new(Admin, [Row("w1", "Андреева"), Row("w2", "Андреева", Day.ToDateTime(default))], false),
            CancellationToken.None);
        var repeat = await handler.Handle(new(Admin, [Row("w1", "Андреева")], false), CancellationToken.None);
        var byChief = await handler.Handle(new(Chief, [Row("w3", "Андреева")], false), CancellationToken.None);
        var work = workshop.Db.Work.OrderBy(x => x.Id).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(withError.Value.Errors, Is.EqualTo(1));
            Assert.That(withError.Value.DryRun, Is.True);
            Assert.That(preview.Value.Created, Is.EqualTo(2));
            Assert.That(applied.Value.Created, Is.EqualTo(2));
            Assert.That(repeat.Value.Unchanged, Is.EqualTo(1));
            Assert.That(byChief.Status, Is.EqualTo(ResultStatus.Forbidden));
            Assert.That(work, Has.Count.EqualTo(2));
            Assert.That(work[0].Rate, Is.EqualTo(196.62m));
            Assert.That(work[1].IsVoided, Is.True);
        });
    }

    [Test]
    public void Forecast_counts_working_days_and_stops_after_an_order_without_rate()
    {
        var settings = new ForecastSettings(2, 480m, 1m); // 960 UAH of rate a day
        var start = SewingForecast.NextWorkingDay(new DateOnly(2026, 11, 6)); // Friday → Monday 9th
        var lines = SewingForecast.Calculate([
            new(1, [new(10, 50m)]), // 500
            new(2, [new(10, 100m)]), // 1500 total → 2 days
            new(3, []),
            new(4, [new(5, null)]),
            new(5, [new(1, 10m)])
        ], settings, start);

        Assert.Multiple(() =>
        {
            Assert.That(start, Is.EqualTo(new DateOnly(2026, 11, 9)));
            Assert.That(lines[0].FinishOn, Is.EqualTo(new DateOnly(2026, 11, 9)));
            Assert.That(lines[1].FinishOn, Is.EqualTo(new DateOnly(2026, 11, 10)));
            Assert.That(lines[1].Minutes, Is.EqualTo(1000m));
            Assert.That(lines[2].Status, Is.EqualTo(ForecastStatus.Complete));
            Assert.That(lines[3].Reason, Is.EqualTo(ForecastReason.MissingRate));
            Assert.That(lines[4].Reason, Is.EqualTo(ForecastReason.UnknownPredecessor));
            Assert.That(SewingForecast.AddWorkingDays(new DateOnly(2026, 11, 13), 2), Is.EqualTo(new DateOnly(2026, 11, 16)));
        });
    }

    [Test]
    public async Task Forecast_shows_dates_to_everyone_and_minutes_to_the_workshop()
    {
        using var workshop = Workshop(out var employeeId);
        var first = await Order(workshop);
        var second = await Order(workshop);
        var withoutSettings = await Forecast(workshop, Chief);
        workshop.Parameters.Values.AddRange([
            new(PayrollParameterCodes.SeamstressCount, "Швачок", 1m, "шт", null),
            new(PayrollParameterCodes.ShiftMinutes, "Хвилин", 480m, "хв", null),
            new(PayrollParameterCodes.MinuteCost, "Вартість", 1m, "грн", null)
        ]);
        await Record(workshop, Chief, first.Id, first.Items[0].Id, new(employeeId, "Sewing", 1, Day, false, null, null));
        var forChief = await Forecast(workshop, Chief);
        var forManager = await Forecast(workshop, Manager);
        var forAdmin = await Forecast(workshop, Admin);

        var stale = await SaveQueue(workshop, Chief, new(5, [second.Id, first.Id]));
        var byManager = await SaveQueue(workshop, Manager, new(0, [second.Id, first.Id]));
        var saved = await SaveQueue(workshop, Chief, new(0, [second.Id]));
        var reordered = await Forecast(workshop, Chief);

        Assert.Multiple(() =>
        {
            Assert.That(withoutSettings.Value.Problem, Does.Contain("Параметри ціноутворення"));
            Assert.That(withoutSettings.Value.Rows.All(x => x.FinishOn is null), Is.True);
            Assert.That(forChief.Value.Problem, Is.Null);
            Assert.That(forChief.Value.StartDate, Is.EqualTo(new DateOnly(2026, 11, 5)));
            Assert.That(forChief.Value.Rows.Select(x => x.OrderId), Is.EqualTo(new[] { first.Id, second.Id }));
            Assert.That(forChief.Value.Rows[0].Remaining, Is.EqualTo(2));
            Assert.That(forChief.Value.Rows[0].Minutes, Is.Not.Null);
            Assert.That(forChief.Value.Rows[1].FinishOn, Is.GreaterThan(forChief.Value.Rows[0].FinishOn));
            Assert.That(forChief.Value.Settings!.MinuteCost, Is.Null);
            Assert.That(forAdmin.Value.Settings!.MinuteCost, Is.EqualTo(1m));
            Assert.That(forManager.Value.Settings, Is.Null);
            Assert.That(forManager.Value.Rows[0].Minutes, Is.Null);
            Assert.That(forManager.Value.Rows[0].FinishOn, Is.EqualTo(forChief.Value.Rows[0].FinishOn));
            Assert.That(forManager.Value.CanReorder, Is.False);
            Assert.That(stale.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(byManager.Status, Is.EqualTo(ResultStatus.Forbidden));
            Assert.That(saved.IsSuccess, Is.True);
            Assert.That(reordered.Value.Rows.Select(x => x.OrderId), Is.EqualTo(new[] { second.Id, first.Id }));
            Assert.That(reordered.Value.Version, Is.EqualTo(1));
        });
    }

    private static ProductionTestHarness Workshop(out int employeeId)
    {
        var workshop = ProductionOrderHandlerTests.Workshop();
        var employee = new WorkshopEmployee("Андреева", "Швачка", 1, workshop.Time.GetUtcNow());
        workshop.Db.Employees.Add(employee);
        workshop.Db.SaveChanges();
        employeeId = employee.Id;
        return workshop;
    }

    private static async Task<ProductionOrderResponse> Order(ProductionTestHarness workshop) =>
        (await ProductionOrderHandlerTests.Create(workshop, Manager, ProductionOrderHandlerTests.Input())).Value;

    private static async Task<Result<ProductionItemWorkResponse>> Record(ProductionTestHarness workshop,
        ProductionActor actor, int orderId, int itemId, CreateProductionWorkInput input) =>
        await new CreateProductionWorkCommandHandler(workshop.Db, workshop.Staff, new WorkCards(workshop.Db), workshop.Time)
            .Handle(new(actor, orderId, itemId, input), CancellationToken.None);

    private static async Task<Result<ProductionItemWorkResponse>> Void(ProductionTestHarness workshop,
        ProductionActor actor, int workId, string reason) =>
        await new VoidProductionWorkCommandHandler(workshop.Db, workshop.Staff, new WorkCards(workshop.Db), workshop.Time)
            .Handle(new(actor, workId, new(reason)), CancellationToken.None);

    private static async Task<Result<ForecastResponse>> Forecast(ProductionTestHarness workshop, ProductionActor actor) =>
        await new GetForecastQueryHandler(new ForecastQueueReader(workshop.Db), new WorkCards(workshop.Db),
            workshop.Parameters, workshop.Time).Handle(new(actor), CancellationToken.None);

    private static async Task<Result> SaveQueue(ProductionTestHarness workshop, ProductionActor actor,
        SaveForecastQueueInput input) =>
        await new SaveForecastQueueCommandHandler(workshop.Db, new ForecastQueueReader(workshop.Db), workshop.Staff,
            workshop.Time).Handle(new(actor, input), CancellationToken.None);
}
