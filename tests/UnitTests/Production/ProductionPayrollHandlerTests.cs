using Ardalis.Result;
using Production.Application.Contracts;
using Production.Application.Orders.Access;
using Production.Application.Payroll;
using Production.Application.Payroll.Close;
using Production.Application.Payroll.GetMonth;
using Production.Application.Payroll.Payments;
using Production.Application.Payroll.Setup;
using Production.Application.Reports;
using Production.Application.PayrollParameters;
using Production.Application.Timesheet;
using Production.Application.Work;
using Production.Application.Work.RecordWork;
using Production.Domain.Payroll;
using Production.Domain.Staff;
using Production.Domain.Work;

namespace UnitTests.Production;

/// <summary>«Робочий час» and «Зарплата» (PRD-006).</summary>
public sealed class ProductionPayrollHandlerTests
{
    private static readonly ProductionActor Admin = new(1, true, false, false);
    private static readonly ProductionActor Manager = new(2, false, true, false);
    private static readonly ProductionActor Chief = new(4, false, false, true);
    private static readonly DateOnly Day = new(2026, 11, 4);
    private const string Month = "2026-11";

    [Test]
    public void Calculation_pays_every_part_by_the_shares()
    {
        var result = PayrollCalculation.Calculate(new PayrollInputs(
            [1, 2, 3],
            [
                new PayrollWorkLine(1, ProductionWorkKind.Sewing, 60, 300m),
                new PayrollWorkLine(2, ProductionWorkKind.Sewing, 10, 300m),
                new PayrollWorkLine(3, ProductionWorkKind.Cutting, 10, 300m)
            ],
            new Dictionary<int, decimal> { [2] = -100m },
            new Dictionary<int, decimal> { [1] = 5000m },
            new PayrollPeople(MasterId: 3, ChiefId: 4, EmbroideryId: 4),
            Stitches: 25_000,
            new PayrollRates(0.08m, 17_500m, 0.2174m, 0.11m, 0.12m, 1m)));

        var first = result.Lines.Single(x => x.EmployeeId == 1);
        var second = result.Lines.Single(x => x.EmployeeId == 2);
        var cutter = result.Lines.Single(x => x.EmployeeId == 3);
        var chief = result.Lines.Single(x => x.EmployeeId == 4);
        Assert.Multiple(() =>
        {
            Assert.That(result.TotalSewing, Is.EqualTo(21_000m));
            Assert.That(first.Sewing, Is.EqualTo(18_000m));
            Assert.That(first.Bonus, Is.EqualTo(1_440m), "the bonus once the sewing reaches the threshold");
            Assert.That(first.Due, Is.EqualTo(14_440m));
            Assert.That(second.Bonus, Is.EqualTo(0m), "below the threshold");
            Assert.That(second.Accrued, Is.EqualTo(2_900m));
            Assert.That(cutter.Cutting, Is.EqualTo(652.20m));
            Assert.That(cutter.Master, Is.EqualTo(2_310m));
            Assert.That(chief.Chief, Is.EqualTo(2_520m));
            Assert.That(chief.Embroidery, Is.EqualTo(25m));
            Assert.That(result.Warnings, Is.Empty);
        });
    }

    [Test]
    public void Missing_people_and_shares_are_warnings()
    {
        var result = PayrollCalculation.Calculate(new PayrollInputs([1],
            [new PayrollWorkLine(1, ProductionWorkKind.Sewing, 1, 100m)],
            new Dictionary<int, decimal>(), new Dictionary<int, decimal>(),
            new PayrollPeople(null, null, null), 10, new PayrollRates(null, 17_500m, 0.2m, 0.11m, 0.12m, null)));

        Assert.That(result.Warnings, Is.EquivalentTo(new[]
        {
            PayrollWarning.NoBonusRate, PayrollWarning.NoEmbroideryRate, PayrollWarning.NoMaster,
            PayrollWarning.NoChief, PayrollWarning.NoEmbroideryWorker
        }));
    }

    [Test]
    public async Task Month_counts_work_hours_and_payments_and_closing_locks_it()
    {
        using var workshop = Workshop(out var seamstress, out var master);
        var order = (await ProductionOrderHandlerTests.Create(workshop, Manager, ProductionOrderHandlerTests.Input())).Value;
        var item = order.Items[0];
        var recorded = await Record(workshop, order.Id, item.Id, new(seamstress, "Sewing", 3, Day, false, null, null));
        await SaveDay(workshop, Chief, new(Day, [new(seamstress, 480, null), new(master, 450, null)]));

        var incomplete = await Close(workshop, Admin);
        Rates(workshop);
        var chiefRoles = await new SetPayrollRolesCommandHandler(workshop.Db, Changes(workshop))
            .Handle(new(Chief, Month, new(null, master)), CancellationToken.None);
        var paidByChief = await Pay(workshop, Chief, seamstress, 500m);
        var paid = await Pay(workshop, Admin, seamstress, 500m);
        var deducted = await new CreatePayrollAdjustmentCommandHandler(workshop.Db, Changes(workshop))
            .Handle(new(Admin, Month, new(seamstress, -29.75m, "Брак тканини")), CancellationToken.None);
        var view = await new GetPayrollMonthQueryHandler(Changes(workshop)).Handle(new(Chief, Month), CancellationToken.None);
        var managerView = await new GetPayrollMonthQueryHandler(Changes(workshop))
            .Handle(new(Manager, Month), CancellationToken.None);
        var closed = await Close(workshop, Admin);
        var afterClose = await Record(workshop, order.Id, item.Id, new(seamstress, "Cutting", 1, Day, false, null, null));
        var voidAfterClose = await new VoidProductionWorkCommandHandler(workshop.Db, workshop.Staff, new WorkCards(workshop.Db),
            workshop.Time).Handle(new(Chief, recorded.Value.Rows[0].Id, new("Помилка")), CancellationToken.None);
        var hoursAfterClose = await SaveDay(workshop, Chief, new(Day, [new(seamstress, 0, 1)]));
        var payAfterClose = await Pay(workshop, Admin, seamstress, 1m);
        var detail = await new GetPayrollEmployeeQueryHandler(Sheets(workshop))
            .Handle(new(Admin, Month, seamstress), CancellationToken.None);
        var reopened = await new ReopenPayrollMonthCommandHandler(workshop.Db, workshop.Staff, Changes(workshop),
            workshop.Time).Handle(new(Admin, Month), CancellationToken.None);
        var future = await Pay(workshop, Admin, seamstress, 1m, "2026-12");

        var line = view.Value.Lines.Single(x => x.EmployeeId == seamstress);
        Assert.Multiple(() =>
        {
            Assert.That(incomplete.Status, Is.EqualTo(ResultStatus.Conflict), "shares are not filled in yet");
            Assert.That(chiefRoles.Value.Chief!.EmployeeId, Is.EqualTo(master));
            Assert.That(chiefRoles.Value.Master!.IsAssigned, Is.False, "the master is found by position");
            Assert.That(paidByChief.Status, Is.EqualTo(ResultStatus.Forbidden));
            Assert.That(paid.IsSuccess, Is.True);
            Assert.That(deducted.IsSuccess, Is.True);
            Assert.That(line.Minutes, Is.EqualTo(480));
            Assert.That(line.Sewing, Is.EqualTo(1029.75m));
            Assert.That(line.Other, Is.EqualTo(-29.75m));
            Assert.That(line.Due, Is.EqualTo(500m));
            Assert.That(view.Value.Lines.Single(x => x.EmployeeId == master).Master, Is.EqualTo(113.27m));
            Assert.That(view.Value.Permissions.CanPay, Is.False, "the chief does not pay");
            Assert.That(managerView.Status, Is.EqualTo(ResultStatus.Forbidden));
            Assert.That(closed.Value.IsClosed, Is.True);
            Assert.That(closed.Value.Permissions.CanReopen, Is.True);
            Assert.That(afterClose.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(voidAfterClose.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(hoursAfterClose.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(payAfterClose.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(detail.Value.Work.Single().Amount, Is.EqualTo(1029.75m));
            Assert.That(detail.Value.Payments, Has.Count.EqualTo(1));
            Assert.That(reopened.Value.IsClosed, Is.False);
            Assert.That(future.Status, Is.EqualTo(ResultStatus.Conflict));
        });
    }

    [Test]
    public async Task Timesheet_keeps_changes_and_refuses_stale_and_future_marks()
    {
        using var workshop = Workshop(out var seamstress, out var master);
        var first = await SaveDay(workshop, Chief, new(Day, [new(seamstress, 480, null)]));
        var changed = await SaveDay(workshop, Admin, new(Day, [new(seamstress, 240, 1), new(master, 0, null)]));
        var stale = await SaveDay(workshop, Chief, new(Day, [new(seamstress, 120, 1)]));
        var future = await SaveDay(workshop, Chief, new(Day.AddDays(1), [new(seamstress, 60, null)]));
        var byManager = await new GetTimesheetDayQueryHandler(Days(workshop))
            .Handle(new(Manager, Day), CancellationToken.None);
        var validator = new SaveTimesheetDayCommandValidator();

        Assert.Multiple(() =>
        {
            Assert.That(first.Value.Rows.Single(x => x.EmployeeId == seamstress).Minutes, Is.EqualTo(480));
            Assert.That(first.Value.Rows.Single(x => x.EmployeeId == master).Minutes, Is.Null, "not marked yet");
            Assert.That(changed.Value.Changes.Select(x => (x.BeforeMinutes, x.AfterMinutes)),
                Is.EqualTo(new (int?, int)[] { (null, 480), (480, 240), (null, 0) }));
            Assert.That(changed.Value.Month.Days.Single().People, Is.EqualTo(2));
            Assert.That(changed.Value.Month.Employees.Sum(x => x.Minutes), Is.EqualTo(240));
            Assert.That(stale.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(future.Status, Is.EqualTo(ResultStatus.Invalid));
            Assert.That(byManager.Status, Is.EqualTo(ResultStatus.Forbidden));
            Assert.That(validator.Validate(new SaveTimesheetDayCommand(Chief, new(Day, [new(seamstress, 50, null)])))
                .IsValid, Is.False, "15-minute steps only");
        });
    }

    private static ProductionTestHarness Workshop(out int seamstress, out int master)
    {
        var workshop = ProductionOrderHandlerTests.Workshop();
        var a = new WorkshopEmployee("Андреева", "Швачка", 1, workshop.Time.GetUtcNow());
        var b = new WorkshopEmployee("Витковська", "Майстер цеху", 1, workshop.Time.GetUtcNow());
        workshop.Db.Employees.AddRange(a, b);
        workshop.Db.SaveChanges();
        seamstress = a.Id;
        master = b.Id;
        return workshop;
    }

    private static void Rates(ProductionTestHarness workshop) => workshop.Parameters.Values.AddRange([
        new(PayrollParameterCodes.SeamstressBonus, "Премія швачки", 0.08m, "%", null),
        new(PayrollParameterCodes.SeamstressBonusThreshold, "Поріг", 17_500m, "грн", null),
        new(PayrollParameterCodes.CuttingRate, "Крій", 0.2174m, "%", null),
        new(PayrollParameterCodes.MasterRate, "Майстер", 0.11m, "%", null),
        new(PayrollParameterCodes.ShopManagerRate, "Начальник", 0.12m, "%", null),
        new(PayrollParameterCodes.EmbroideryRate, "Вишивка", 1m, "грн", null)
    ]);

    private static PayrollSheets Sheets(ProductionTestHarness workshop) => new(workshop.Db, workshop.Parameters);

    private static PayrollChanges Changes(ProductionTestHarness workshop) =>
        new(workshop.Db, workshop.Staff, Sheets(workshop), workshop.Time);

    private static TimesheetDays Days(ProductionTestHarness workshop) => new(workshop.Db, workshop.Time);

    private static async Task<Result<ProductionItemWorkResponse>> Record(ProductionTestHarness workshop, int orderId,
        int itemId, CreateProductionWorkInput input) =>
        await new CreateProductionWorkCommandHandler(workshop.Db, workshop.Staff, new WorkCards(workshop.Db), workshop.Time)
            .Handle(new(Chief, orderId, itemId, input), CancellationToken.None);

    private static async Task<Result<TimesheetDayResponse>> SaveDay(ProductionTestHarness workshop,
        ProductionActor actor, SaveTimesheetDayInput input) =>
        await new SaveTimesheetDayCommandHandler(workshop.Db, workshop.Staff, Days(workshop), workshop.Time)
            .Handle(new(actor, input), CancellationToken.None);

    private static async Task<Result<PayrollMonthResponse>> Pay(ProductionTestHarness workshop, ProductionActor actor,
        int employeeId, decimal amount, string month = Month) =>
        await new CreatePayrollPaymentCommandHandler(workshop.Db, Changes(workshop), workshop.Time)
            .Handle(new(actor, month, new(employeeId, Day, "Advance", amount, null)), CancellationToken.None);

    private static async Task<Result<PayrollMonthResponse>> Close(ProductionTestHarness workshop, ProductionActor actor) =>
        await new ClosePayrollMonthCommandHandler(Changes(workshop), Sheets(workshop),
                new WorkshopReports(workshop.Db, Sheets(workshop), workshop.Fabrics, workshop.Stock))
            .Handle(new(actor, Month), CancellationToken.None);
}
