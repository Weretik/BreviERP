using Ardalis.Result;
using Production.Domain.Staff;
using UnitTests.Warehouse;
using Production.Application.Features.Dashboard.Queries.GetWorkshopDashboard;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Payroll.Commands.ClosePayrollMonth;
using Production.Application.Features.Payroll.Queries.GetPayrollMonth;
using Production.Application.Features.Payroll.Shared;
using Production.Application.Features.PayrollParameters.Shared;
using Production.Application.Features.Reports.Queries.GetWorkshopReport;
using Production.Application.Features.Reports.Shared;
using Production.Application.Features.Work.Commands.CreateProductionWork;
using Production.Application.Features.Work.Shared;
using Production.Application.Features.Dashboard.Queries.GetWorkshopDashboard.Results;
using Production.Application.Features.Reports.Shared.Rows;
using UnitTests.Production.Orders;
using UnitTests.Warehouse.Stock;

namespace UnitTests.Production.Workshop;

/// <summary>«Звіти цеху» and «Огляд виробництва» (PRD-007).</summary>
public sealed class WorkshopReportHandlerTests
{
    private static readonly ProductionActor Admin = new(1, true, false, false);
    private static readonly ProductionActor Manager = new(2, false, true, false);
    private static readonly ProductionActor Chief = new(4, false, false, true);
    private static readonly DateOnly Day = new(2026, 11, 4);
    private const string Month = "2026-11";

    [Test]
    public async Task Report_values_output_materials_and_stock_and_a_closed_month_keeps_it()
    {
        using var workshop = Workshop(out var seamstress);
        var (orderId, itemId) = await SewnOrderAsync(workshop, seamstress);

        var report = await Report(workshop, Admin, Month, Month, "2026-10", "2026-10");
        var byChief = await Report(workshop, Chief, Month, Month);
        var future = await Report(workshop, Admin, Month, "2026-12");
        var tooLong = new GetWorkshopReportValidator()
            .Validate(new GetWorkshopReportQuery(Admin, "2025-01", Month, null, null));
        Rates(workshop);
        var month = await new GetPayrollMonthHandler(Changes(workshop)).Handle(new(Admin, Month), CancellationToken.None);
        var closed = await new ClosePayrollMonthHandler(Changes(workshop), Sheets(workshop), Reports(workshop))
            .Handle(new(Admin, Month), CancellationToken.None);
        workshop.Fabrics.Fabrics[0] = workshop.Fabrics.Fabrics[0] with { Price = 1m };
        var kept = await Report(workshop, Admin, Month, Month);

        var current = report.Value.Current;
        Assert.Multiple(() =>
        {
            Assert.That(current.Output.Single().ItemId, Is.EqualTo(itemId));
            Assert.That(current.Output.Single().OrderId, Is.EqualTo(orderId));
            Assert.That(current.Totals.SewnPieces, Is.EqualTo(3), "over-plan work does not count");
            Assert.That(current.Totals.OutputValue, Is.EqualTo(4500m), "3 pieces at the КП price 1500");
            Assert.That(current.Totals.IssuedMeters, Is.EqualTo(4m));
            Assert.That(current.Totals.IssuedValue, Is.EqualTo(480m));
            Assert.That(current.Stock.Single().Meters, Is.EqualTo(6m));
            Assert.That(current.Totals.StockValue, Is.EqualTo(720m));
            Assert.That(current.Staff.Single().Sewn, Is.EqualTo(4), "over-plan work is still paid");
            Assert.That(current.Staff.Single().Minutes, Is.EqualTo(0));
            Assert.That(current.Staff.Single().DayAverage, Is.Null, "no hours, no average");
            Assert.That(current.OpenMonths, Is.EqualTo(new[] { Month }));
            Assert.That(report.Value.Comparison!.Totals.SewnPieces, Is.EqualTo(0));
            Assert.That(report.Value.Comparison.Stock, Is.Empty, "nothing was in stock before November");
            Assert.That(byChief.Status, Is.EqualTo(ResultStatus.Forbidden));
            Assert.That(future.Status, Is.EqualTo(ResultStatus.Invalid));
            Assert.That(tooLong.IsValid, Is.False);
            Assert.That(month.Value.Permissions.CanClose, Is.True, "the warning about the chief does not block");
            Assert.That(closed.IsSuccess, Is.True, "no chief is not a reason to keep the month open");
            Assert.That(closed.Value.Warnings, Has.Some.Contains("начальника"));
            Assert.That(kept.Value.Current.ClosedMonths, Is.EqualTo(new[] { Month }));
            Assert.That(kept.Value.Current.Totals.IssuedValue, Is.EqualTo(480m), "a closed month keeps its prices");
        });
    }

    [Test]
    public void Months_add_up_and_the_stock_is_the_last_one()
    {
        var october = Month("2026-10", 2, 100m, 5m);
        var november = Month("2026-11", 3, 100m, 8m);

        var sum = WorkshopReports.Merge("2026-10", "2026-11", [(october, true), (november, false)]);

        Assert.Multiple(() =>
        {
            Assert.That(sum.Output.Single().Quantity, Is.EqualTo(5));
            Assert.That(sum.Totals.OutputValue, Is.EqualTo(500m));
            Assert.That(sum.Staff.Single().Minutes, Is.EqualTo(960));
            Assert.That(sum.Staff.Single().DayAverage, Is.EqualTo(250m), "500 for two 8-hour days");
            Assert.That(sum.Totals.StockMeters, Is.EqualTo(8m));
            Assert.That(sum.ClosedMonths, Is.EqualTo(new[] { "2026-10" }));
            Assert.That(sum.OpenMonths, Is.EqualTo(new[] { "2026-11" }));
        });

        static WorkshopReportResponse Month(string month, int pieces, decimal price, decimal stock)
        {
            ReportOutputRowResponse[] output = [new(1, 5, "Клієнт", 9, "Халат", pieces, price, pieces * price)];
            ReportStaffRowResponse[] staff = [new(3, "Андреева", "Швачка", 480, pieces, 0, pieces * price, null)];
            ReportStockRowResponse[] rows = [new(1, "FAB-023", "Грета", "Сірий", stock, null, null)];
            return WorkshopReports.Merge(month, month, [(new WorkshopReportResponse(month, month, [], [], null!,
                output, staff, [], rows, []), false)]);
        }
    }

    [Test]
    public async Task Dashboard_shows_open_orders_by_their_slowest_stage()
    {
        using var workshop = Workshop(out var seamstress);
        await SewnOrderAsync(workshop, seamstress);
        workshop.Time.Advance(TimeSpan.FromDays(60));

        var forManager = await Dashboard(workshop, Manager);
        var forChief = await Dashboard(workshop, Chief);

        var card = forManager.Stages.Single(x => x.Orders.Count > 0);
        Assert.Multiple(() =>
        {
            Assert.That(forManager.OpenOrders, Is.EqualTo(1));
            Assert.That(card.Label, Is.EqualTo("Нове"));
            Assert.That(card.Orders.Single().Progress, Is.GreaterThan(0));
            Assert.That(card.Orders.Single().IsLate, Is.True);
            Assert.That(forManager.LateOrders, Is.EqualTo(1));
            Assert.That(forManager.ReadyOrders, Is.EqualTo(0));
            Assert.That(forManager.Hours, Is.Null, "managers do not keep the hours");
            Assert.That(forChief.Hours!.Select(x => x.EmployeeId), Does.Contain(seamstress), "every active worker");
        });
    }

    private static async Task<(int OrderId, int ItemId)> SewnOrderAsync(ProductionTestHarness workshop, int seamstress)
    {
        var stock = new WarehouseTestHarness(workshop, ownsWorkshop: false);
        workshop.Attached.Add(stock);
        workshop.Stock = new HarnessProductionStockReader(stock.Facts());
        var keeper = new global::Warehouse.Application.Features.Stock.Shared.WarehouseActor(4, false, false, true);
        await WarehouseStockHandlerTests.Receive(stock, keeper, "Opening", 10m);
        var order = (await ProductionOrderHandlerTests.Create(workshop, Manager, ProductionOrderHandlerTests.Input())).Value;
        var itemId = order.Items[0].Id;
        await WarehouseStockHandlerTests.Issue(stock, keeper, order.Id, itemId, order.Version,
            WarehouseStockHandlerTests.L("темно-сірий", "Темно-сірий", 4m));
        var record = new CreateProductionWorkHandler(workshop.Db, workshop.Staff, new WorkCards(workshop.Db),
            workshop.Time);
        await record.Handle(new(Chief, order.Id, itemId, new(seamstress, "Sewing", 3, Day, false, null, null)),
            CancellationToken.None);
        await record.Handle(new(Chief, order.Id, itemId, new(seamstress, "Sewing", 1, Day, true, "Зразок", null)),
            CancellationToken.None);
        return (order.Id, itemId);
    }

    private static ProductionTestHarness Workshop(out int seamstress)
    {
        var workshop = ProductionOrderHandlerTests.Workshop();
        workshop.Fabrics.Fabrics.Clear();
        workshop.Fabrics.Fabrics.Add(new(23, "FAB-023", "Тканина «Грета»", 120m));
        var a = WorkshopEmployee.Create("Андреева", "Швачка", 1, workshop.Time.GetUtcNow()).Value;
        var b = WorkshopEmployee.Create("Витковська", "Майстер цеху", 1, workshop.Time.GetUtcNow()).Value;
        workshop.Db.Employees.AddRange(a, b);
        workshop.Db.SaveChanges();
        seamstress = a.Id;
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

    private static WorkshopReports Reports(ProductionTestHarness workshop) =>
        new(workshop.Db, Sheets(workshop), workshop.Fabrics, workshop.Stock);

    private static async Task<Result<WorkshopReportPairResponse>> Report(ProductionTestHarness workshop,
        ProductionActor actor, string from, string to, string? compareFrom = null, string? compareTo = null) =>
        await new GetWorkshopReportHandler(Reports(workshop), workshop.Time)
            .Handle(new(actor, from, to, compareFrom, compareTo), CancellationToken.None);

    private static async Task<WorkshopDashboardResponse> Dashboard(ProductionTestHarness workshop,
        ProductionActor actor) =>
        (await new GetWorkshopDashboardHandler(workshop.Db, new WorkCards(workshop.Db), workshop.Time)
            .Handle(new(actor), CancellationToken.None)).Value;
}
