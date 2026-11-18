using BuildingBlocks.Domain.Identifiers.People;
using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Payroll.Commands.CreatePayrollPayment;
using Production.Application.Features.Payroll.Shared;
using Production.Domain.Payroll;
using Production.Domain.Staff;
using Production.Infrastructure.Persistence;
using UnitTests.Production.Orders;

namespace UnitTests.Production.Payroll;

/// <summary>
/// A change of «Зарплата» and the closing of its month at the same moment: the second to save gets 409 and nothing of
/// it lands in a closed month (database audit 2026-11-15, 5.1).
/// </summary>
public sealed class PayrollMonthRaceTests
{
    private static readonly ProductionActor Admin = new(1, true, false, false);
    private static readonly DateOnly Day = new(2026, 11, 4);
    private const string Month = "2026-11";

    [Test]
    public async Task Payment_saved_after_another_request_closed_the_month_is_refused()
    {
        using var workshop = ProductionOrderHandlerTests.Workshop();
        var seamstress = Employee(workshop);
        await new CreatePayrollPaymentHandler(workshop.Db, Changes(workshop), workshop.Time)
            .Handle(new(Admin, Month, new(seamstress, Day, "Advance", 100m, null)), CancellationToken.None);

        var changes = Changes(workshop);
        var raced = await changes.ApplyAsync(Admin, Month, adminOnly: true, async change =>
        {
            await CloseInAnotherRequestAsync(workshop);
            var payment = PayrollPayment.Create(new PayrollEntryTarget(change.Period.Month, new WorkshopEmployeeId(seamstress), "Андреева"), Day,
                PayrollPaymentKind.Advance, 50m, null, change.ActorName, change.Now);
            workshop.Db.PayrollPayments.Add(payment.Value);
            return null;
        }, CancellationToken.None);

        await using var check = workshop.OtherRequest();
        Assert.Multiple(() =>
        {
            Assert.That(raced.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(raced.Errors, Has.Some.Contains("щойно змінили"));
            Assert.That(check.PayrollPayments.Count(), Is.EqualTo(1), "the second payment is not in the closed month");
        });
    }

    [Test]
    public async Task Work_saved_after_another_request_closed_the_month_is_refused()
    {
        using var workshop = ProductionOrderHandlerTests.Workshop();
        await using (var first = workshop.OtherRequest())
        {
            first.PayrollMonths.Add(PayrollMonth.Create(Day));
            await first.SaveChangesAsync();
        }

        var marked = await PayrollMonths.MarkChangedAsync(workshop.Db, Day, "Начальник", workshop.Time.GetUtcNow(),
            CancellationToken.None);
        await CloseInAnotherRequestAsync(workshop);
        var saved = await PayrollMonths.SaveAsync(workshop.Db, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(marked, Is.True, "the month was open when the change started");
            Assert.That(saved.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(saved.Errors, Has.Some.Contains("щойно змінили"));
        });
    }

    [Test]
    public async Task Marking_a_closed_month_reports_it()
    {
        using var workshop = ProductionOrderHandlerTests.Workshop();
        await CloseInAnotherRequestAsync(workshop);

        var marked = await PayrollMonths.MarkChangedAsync(workshop.Db, Day.AddDays(10), "Начальник",
            workshop.Time.GetUtcNow(), CancellationToken.None);

        Assert.That(marked, Is.False);
    }

    private static int Employee(ProductionTestHarness workshop)
    {
        var employee = WorkshopEmployee.Create("Андреева", "Швачка", new UserId(1), workshop.Time.GetUtcNow()).Value;
        workshop.Db.Employees.Add(employee);
        workshop.Db.SaveChanges();
        return employee.Id.Value;
    }

    private static async Task CloseInAnotherRequestAsync(ProductionTestHarness workshop)
    {
        await using var other = workshop.OtherRequest();
        var month = await other.PayrollMonths.SingleOrDefaultAsync(x => x.Month == new DateOnly(2026, 11, 1));
        if (month is null)
        {
            month = PayrollMonth.Create(Day);
            other.PayrollMonths.Add(month);
        }

        month.Close("{}", "{}", "Адміністратор", workshop.Time.GetUtcNow());
        await other.SaveChangesAsync();
    }

    private static PayrollChanges Changes(ProductionTestHarness workshop) =>
        new(workshop.Db, workshop.Staff, new PayrollSheets(workshop.Db, workshop.Parameters), workshop.Time);
}
