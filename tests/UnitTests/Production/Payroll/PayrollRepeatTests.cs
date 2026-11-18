using BuildingBlocks.Domain.Identifiers.People;
using Ardalis.Result;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Payroll.Commands.CreatePayrollAdjustment;
using Production.Application.Features.Payroll.Commands.CreatePayrollPayment;
using Production.Application.Features.Payroll.Shared;
using Production.Domain.Staff;
using UnitTests.Production.Orders;

namespace UnitTests.Production.Payroll;

/// <summary>
/// «Зберегти» pressed twice, or a request repeated by a weak connection, adds one payment or adjustment (database
/// audit 2026-11-15, 5.2): the repeat with the same request id returns the month as it is.
/// </summary>
public sealed class PayrollRepeatTests
{
    private static readonly ProductionActor Admin = new(1, true, false, false);
    private static readonly DateOnly Day = new(2026, 11, 4);
    private const string Month = "2026-11";

    [Test]
    public async Task A_repeated_payment_is_saved_once()
    {
        using var workshop = ProductionOrderHandlerTests.Workshop();
        var seamstress = Employee(workshop);
        var handler = new CreatePayrollPaymentHandler(workshop.Db, Changes(workshop), workshop.Time);
        var input = new CreatePayrollPaymentInput(seamstress, Day, "Advance", 100m, null, Guid.NewGuid());

        var first = await handler.Handle(new(Admin, Month, input), CancellationToken.None);
        var repeat = await handler.Handle(new(Admin, Month, input), CancellationToken.None);
        var other = await handler.Handle(new(Admin, Month, input with { RequestId = Guid.NewGuid() }),
            CancellationToken.None);
        var changed = await handler.Handle(new(Admin, Month, input with { Amount = 200m }), CancellationToken.None);

        await using var check = workshop.OtherRequest();
        Assert.Multiple(() =>
        {
            Assert.That((first.Status, repeat.Status, other.Status),
                Is.EqualTo((ResultStatus.Ok, ResultStatus.Ok, ResultStatus.Ok)));
            Assert.That(check.PayrollPayments.Count(), Is.EqualTo(2), "the repeat adds nothing, a new request does");
            Assert.That(changed.Status, Is.EqualTo(ResultStatus.Conflict), "the same id with other data is refused");
        });
    }

    [Test]
    public async Task A_repeated_adjustment_is_saved_once()
    {
        using var workshop = ProductionOrderHandlerTests.Workshop();
        var seamstress = Employee(workshop);
        var handler = new CreatePayrollAdjustmentHandler(workshop.Db, Changes(workshop));
        var input = new CreatePayrollAdjustmentInput(seamstress, -50m, "Брак тканини", Guid.NewGuid());

        await handler.Handle(new(Admin, Month, input), CancellationToken.None);
        var repeat = await handler.Handle(new(Admin, Month, input), CancellationToken.None);
        var changed = await handler.Handle(new(Admin, Month, input with { Reason = "Інша причина" }),
            CancellationToken.None);

        await using var check = workshop.OtherRequest();
        Assert.Multiple(() =>
        {
            Assert.That(repeat.Status, Is.EqualTo(ResultStatus.Ok));
            Assert.That(changed.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(check.PayrollAdjustments.Count(), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task A_repeat_by_someone_without_the_right_is_refused()
    {
        using var workshop = ProductionOrderHandlerTests.Workshop();
        var seamstress = Employee(workshop);
        var handler = new CreatePayrollPaymentHandler(workshop.Db, Changes(workshop), workshop.Time);
        var input = new CreatePayrollPaymentInput(seamstress, Day, "Advance", 100m, null, Guid.NewGuid());
        await handler.Handle(new(Admin, Month, input), CancellationToken.None);

        var foreign = await handler.Handle(new(new ProductionActor(2, false, false, false), Month, input),
            CancellationToken.None);

        Assert.That(foreign.Status, Is.EqualTo(ResultStatus.Forbidden));
    }

    private static int Employee(ProductionTestHarness workshop)
    {
        var employee = WorkshopEmployee.Create("Андреева", "Швачка", new UserId(1), workshop.Time.GetUtcNow()).Value;
        workshop.Db.Employees.Add(employee);
        workshop.Db.SaveChanges();
        return employee.Id.Value;
    }

    private static PayrollChanges Changes(ProductionTestHarness workshop) =>
        new(workshop.Db, workshop.Staff, new PayrollSheets(workshop.Db, workshop.Parameters), workshop.Time);
}
