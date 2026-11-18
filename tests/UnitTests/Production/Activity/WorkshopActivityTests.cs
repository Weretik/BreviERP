using BuildingBlocks.Domain.Identifiers.Orders;
using BuildingBlocks.Domain.Identifiers.People;
using Production.Domain.Orders.ValueObjects;
using ActivityLog.Contracts.Activity;
using BuildingBlocks.Domain.Events;
using Production.Application.Features.Activity.EventHandlers;
using Production.Domain.FabricNorms;
using Production.Domain.Payroll;
using Production.Domain.Staff;
using Production.Domain.Work;

namespace UnitTests.Production.Activity;

/// <summary>Workshop work, staff, fabric norms and payroll read as «Журнал дій» entries (ACT-001).</summary>
public sealed class WorkshopActivityTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 16, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = new(2026, 11, 16);

    private static ActivityDescription? Last(IActivityDescriber describer, IHasDomainEvents source) =>
        describer.Describe(source.DomainEvents.Last());

    [Test]
    public void Recorded_and_voided_work_is_listed_but_moved_work_is_not()
    {
        var target = new WorkTarget(new ProductionOrderId(1), new ProductionItemId(2), new WorkshopEmployeeId(3), "Ірина Бондар");
        var facts = new WorkFacts(ProductionWorkKind.Sewing, 15, Today, false, null, null, 120m);
        var work = ProductionWork.Create(target, facts, "Майстер", Now).Value;
        var describer = new ProductionWorkActivity();

        Assert.That(Last(describer, work)!.Summary, Is.EqualTo("Пошиття: 15 шт."));
        work.Void("Помилка в кількості", "Майстер", Now);
        Assert.That(Last(describer, work)!.Action, Is.EqualTo(ActivityActions.Voided));

        var moved = ProductionWork.Create(target, facts, "Перенесення", Now, legacyId: "old-1").Value;
        Assert.That(((IHasDomainEvents)moved).DomainEvents, Is.Empty);
    }

    [Test]
    public void Renaming_an_employee_lists_the_change()
    {
        var employee = WorkshopEmployee.Create("Олена Коваль", "Швачка", new UserId(1), Now).Value;
        var describer = new WorkshopActivity();
        Assert.That(Last(describer, employee)!.Action, Is.EqualTo(ActivityActions.Created));

        employee.Update("Олена Коваль", "Майстер цеху", new UserId(1), Now);

        Assert.That(Last(describer, employee)!.Changes,
            Is.EqualTo(new[] { new ActivityChangeItem("Посада", "Швачка", "Майстер цеху") }));
    }

    [Test]
    public void A_changed_norm_lists_meters()
    {
        var norm = ModelFabricNorm.Create("MOD-1", new FabricNormValues(1.5m, null, null, null, null, null), new UserId(1), Now).Value;
        ((IHasDomainEvents)norm).ClearDomainEvents();

        norm.Set(new FabricNormValues(1.75m, null, null, null, null, null), new UserId(1), Now.AddMinutes(1));

        Assert.That(Last(new FabricNormActivity(), norm)!.Changes,
            Is.EqualTo(new[] { new ActivityChangeItem("Один колір", "1,50 м", "1,75 м") }));
    }

    [Test]
    public void A_payment_shows_kind_and_amount()
    {
        var payment = PayrollPayment.Create(new PayrollEntryTarget(new DateOnly(2026, 11, 1), new WorkshopEmployeeId(3), "Ірина Бондар"), Today,
            PayrollPaymentKind.Card, 12000m, null, "Директор", Now).Value;

        var entry = Last(new PayrollActivity(), payment);

        Assert.That(entry!.Summary, Is.EqualTo("На картку: 12 000 грн"));
        Assert.That(entry.SubjectTitle, Is.EqualTo("Ірина Бондар · 11.2026"));
    }
}
