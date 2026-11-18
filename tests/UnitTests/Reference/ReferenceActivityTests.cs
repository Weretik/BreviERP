using BuildingBlocks.Domain.Identifiers.ReferenceData;
using ActivityLog.Contracts.Activity;
using BuildingBlocks.Domain.Events;
using Reference.Application.Features.Activity.EventHandlers;
using Reference.Domain.Fabrics;
using Reference.Domain.Suppliers;
using BuildingBlocks.Domain;

namespace UnitTests.Reference;

/// <summary>«Довідники» entries read as «Журнал дій» entries (ACT-001): created, changed, deleted.</summary>
public sealed class ReferenceActivityTests
{
    private static Fabric NewFabric() =>
        Fabric.Create(new FabricId(3), "Сантана", MoneyAmount.From(50m), new SupplierId(5)).Value;

    private static ActivityDescription? Last(IHasDomainEvents source) =>
        new ReferenceActivity().Describe(source.DomainEvents.Last());

    [Test]
    public void A_new_entry_is_listed_once_as_created()
    {
        var fabric = NewFabric();

        Assert.That(((IHasDomainEvents)fabric).DomainEvents, Has.Count.EqualTo(1));
        var entry = Last(fabric);
        Assert.That(entry!.Action, Is.EqualTo(ActivityActions.Created));
        Assert.That(entry.SubjectType, Is.EqualTo("fabric"));
        Assert.That(entry.SubjectId, Is.EqualTo("3"));
        Assert.That(entry.Changes, Is.Empty);
    }

    [Test]
    public void A_rename_lists_the_name_was_and_became()
    {
        var fabric = NewFabric();
        ((IHasDomainEvents)fabric).ClearDomainEvents();

        fabric.Update("Сантана люкс", MoneyAmount.From(50m), new SupplierId(5));

        var entry = Last(fabric);
        Assert.That(entry!.Action, Is.EqualTo(ActivityActions.Changed));
        var change = entry.Changes.Single();
        Assert.That(change.Field, Is.EqualTo("Назва"));
        Assert.That(change.From, Is.EqualTo("Сантана"));
        Assert.That(change.To, Is.EqualTo("Сантана люкс"));
    }

    [Test]
    public void A_new_price_lists_the_price_in_hryvnias()
    {
        var fabric = NewFabric();

        fabric.SetPrice(MoneyAmount.From(62.5m), null);

        var change = Last(fabric)!.Changes.Single();
        Assert.That(change.Field, Is.EqualTo("Ціна"));
        Assert.That(change.From, Is.EqualTo("50 грн"));
        Assert.That(change.To, Is.EqualTo("62,50 грн"));
    }

    [Test]
    public void Deleting_an_entry_is_listed_with_its_name()
    {
        var supplier = Supplier.Create(new SupplierId(7), "Спецтекстиль, ТОВ").Value;

        supplier.MarkDeleted();

        var entry = Last(supplier);
        Assert.That(entry!.Action, Is.EqualTo(ActivityActions.Deleted));
        Assert.That(entry.SubjectTitle, Is.EqualTo("Спецтекстиль, ТОВ"));
        Assert.That(entry.SubjectTypeTitle, Is.EqualTo("Постачальник"));
    }
}
