using ActivityLog.Contracts.Activity;
using BuildingBlocks.Domain.Events;
using Production.Application.Features.Activity.EventHandlers;
using Production.Domain.Orders;
using Production.Domain.Orders.Events;

namespace UnitTests.Production.Activity;

/// <summary>«Виробниче завдання» actions read as «Журнал дій» entries; its history lines become the fields (ACT-001).</summary>
public sealed class ProductionOrderActivityTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 16, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = new(2026, 11, 16);
    private readonly ProductionOrderActivityDescriber _describer = new();

    private static ProductionOrder NewOrder()
    {
        var item = ProductionItem.Create(1,
            new ItemSource(null, null, null, null, "MOD-1", "Куртка", null, null, null, 120m, null),
            new ItemDetails(10, Today.AddDays(7), "Синій", [], null, null, null, []), null).Value;
        return ProductionOrder.Create(42, new OrderOrigin(5, "2026-11-003", null, 7, "Юлія", null, null, null),
            new OrderHeader("ТОВ «Агро»", Today, null), null, [item], "Юлія", Now).Value;
    }

    private static ProductionOrder CleanOrder()
    {
        var order = NewOrder();
        ((IHasDomainEvents)order).ClearDomainEvents();
        return order;
    }

    private ActivityDescription? Single(ProductionOrder order) =>
        _describer.Describe(((IHasDomainEvents)order).DomainEvents.Single());

    [Test]
    public void Creating_names_the_quote_it_came_from()
    {
        var entry = Single(NewOrder());

        Assert.That(entry!.Action, Is.EqualTo(ActivityActions.Created));
        Assert.That(entry.SubjectTitle, Is.EqualTo("№ 42 · ТОВ «Агро»"));
        Assert.That(entry.Summary, Is.EqualTo("З КП 2026-11-003"));
    }

    [Test]
    public void Editing_turns_history_lines_into_changed_fields()
    {
        var order = CleanOrder();

        order.Update(new OrderHeader("ТОВ «Агро-Плюс»", Today, "Терміново"), new Dictionary<int, ItemUpdate>(), "Юлія", Now);

        Assert.That(Single(order)!.Changes, Is.EqualTo(new[]
        {
            new ActivityChangeItem("Клієнт", "ТОВ «Агро»", "ТОВ «Агро-Плюс»"),
            new ActivityChangeItem("Коментар", null, "Терміново"),
        }));
    }

    [Test]
    public void A_shipment_that_completes_the_item_is_one_entry_with_the_stage()
    {
        var order = CleanOrder();
        var itemId = order.Items[0].Id;

        order.Ship(itemId, 10, Today, null, "Юлія", Now);

        var entry = Single(order);
        Assert.That(entry!.Summary, Does.StartWith("Куртка: Відвантаження: 10 шт."));
        Assert.That(entry.Changes.Select(x => x.Field), Is.EqualTo(new[] { "Куртка: Етапи" }));
    }

    [Test]
    public void Cancelling_keeps_the_reason_and_deleting_is_listed()
    {
        var order = CleanOrder();

        order.Cancel("Клієнт передумав", "Юлія", Now);
        var cancelled = _describer.Describe(((IHasDomainEvents)order).DomainEvents.Last());
        order.Delete("Юлія", Now);
        var deleted = _describer.Describe(((IHasDomainEvents)order).DomainEvents.Last());

        Assert.That(cancelled!.Action, Is.EqualTo(ActivityActions.Cancelled));
        Assert.That(cancelled.Summary, Is.EqualTo("Скасоване: Клієнт передумав"));
        Assert.That(deleted!.Action, Is.EqualTo(ActivityActions.Deleted));
    }

    [Test]
    public void History_only_lines_raise_nothing()
    {
        var order = CleanOrder();

        order.Record("Створено", "З КП 2026-11-003", "Юлія", Now);

        Assert.That(((IHasDomainEvents)order).DomainEvents, Is.Empty);
    }
}
