using BuildingBlocks.Domain.Exceptions;
using Reference.Domain.Suppliers.Entities;
using Reference.Domain.Suppliers.ValueObjects;

namespace UnitTests.Reference;

public sealed class SupplierTests
{
    private static readonly DateTimeOffset SyncedAt = new(2026, 11, 2, 10, 0, 0, TimeSpan.Zero);

    private static KeepinCrmSupplierSnapshot Snapshot(string name = "Тексика") =>
        new("104551", name, "https://teksika.ua/", "Анна", "+380501112233", "a@teksika.com", "тканина, опт");

    [Test]
    public void New_manual_supplier_is_active_and_not_linked_to_keepin()
    {
        var supplier = Supplier.Create(SupplierId.From(7), " Спецтекстиль ");

        Assert.Multiple(() =>
        {
            Assert.That(supplier.Name, Is.EqualTo("Спецтекстиль"));
            Assert.That(supplier.IsActive, Is.True);
            Assert.That(supplier.IsManagedByKeepinCrm, Is.False);
        });
    }

    [Test]
    public void Manual_supplier_updates_contacts_and_purchasing()
    {
        var supplier = Supplier.Create(SupplierId.From(7), "Спецтекстиль");

        supplier.Update("Спецтекстиль", "spectex.ua", "Олег", "+380501112233", "нотатка", " info@spectex.ua ");
        supplier.UpdatePurchasing(" тканина ", false);

        Assert.Multiple(() =>
        {
            Assert.That(supplier.Email, Is.EqualTo("info@spectex.ua"));
            Assert.That(supplier.PurchaseCategories, Is.EqualTo("тканина"));
            Assert.That(supplier.IsActive, Is.False);
        });
    }

    [Test]
    public void Keepin_snapshot_overwrites_contacts_and_keeps_brevi_fields()
    {
        var supplier = Supplier.Create(SupplierId.From(7), "Тексика, ТОВ", notes: "домовились про знижку");
        supplier.UpdatePurchasing("тканина", true);

        supplier.ApplyKeepinCrmSnapshot(Snapshot(), SyncedAt);

        Assert.Multiple(() =>
        {
            Assert.That(supplier.KeepinCrmId, Is.EqualTo("104551"));
            Assert.That(supplier.Name, Is.EqualTo("Тексика"));
            Assert.That(supplier.Email, Is.EqualTo("a@teksika.com"));
            Assert.That(supplier.Tags, Is.EqualTo("тканина, опт"));
            Assert.That(supplier.KeepinSyncedAt, Is.EqualTo(SyncedAt));
            Assert.That(supplier.Notes, Is.EqualTo("домовились про знижку"));
            Assert.That(supplier.PurchaseCategories, Is.EqualTo("тканина"));
        });
    }

    [Test]
    public void Keepin_supplier_rejects_contact_changes_but_accepts_notes()
    {
        var supplier = Supplier.CreateFromKeepinCrm(SupplierId.From(7), Snapshot(), SyncedAt);

        Assert.Throws<DomainException>(() =>
            supplier.Update("Інша назва", supplier.Link, supplier.ContactPerson, supplier.PhoneNumber, null, supplier.Email));

        supplier.Update(supplier.Name, supplier.Link, supplier.ContactPerson, supplier.PhoneNumber, "нова нотатка", supplier.Email);
        Assert.That(supplier.Notes, Is.EqualTo("нова нотатка"));
    }

    [Test]
    public void Keepin_snapshot_requires_id_and_truncates_long_values()
    {
        var supplier = Supplier.Create(SupplierId.From(7), "Тексика");

        Assert.Throws<DomainException>(() => supplier.ApplyKeepinCrmSnapshot(Snapshot() with { KeepinCrmId = " " }, SyncedAt));

        supplier.ApplyKeepinCrmSnapshot(Snapshot(new string('я', 250)) with { Tags = new string('t', 600) }, SyncedAt);
        Assert.Multiple(() =>
        {
            Assert.That(supplier.Name, Has.Length.EqualTo(Supplier.NameMaxLength));
            Assert.That(supplier.Tags, Has.Length.EqualTo(Supplier.TagsMaxLength));
        });
    }
}
