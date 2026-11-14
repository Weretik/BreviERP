using Reference.Domain.Suppliers;
using Reference.Domain.Suppliers;

namespace UnitTests.Reference;

public sealed class SupplierTests
{
    private static readonly DateTimeOffset SyncedAt = new(2026, 11, 2, 10, 0, 0, TimeSpan.Zero);

    private static KeepinCrmSupplierSnapshot Snapshot(string name = "Тексика") =>
        new("104551", name, "тканина", "https://teksika.ua/", "Анна", "+380501112233, 0532671109", "a@teksika.com", "опт");

    [Test]
    public void New_manual_supplier_is_active_and_not_linked_to_keepin()
    {
        var supplier = Supplier.Create(SupplierId.From(7), " Спецтекстиль ").Value;

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
        var supplier = Supplier.Create(SupplierId.From(7), "Спецтекстиль").Value;

        supplier.Update("Спецтекстиль", "spectex.ua", "Олег", "+380501112233", "нотатка", " info@spectex.ua ", " тканина ");
        supplier.SetActive(false);

        Assert.Multiple(() =>
        {
            Assert.That(supplier.Email, Is.EqualTo("info@spectex.ua"));
            Assert.That(supplier.PurchaseCategories, Is.EqualTo("тканина"));
            Assert.That(supplier.IsActive, Is.False);
        });
    }

    [Test]
    public void Keepin_snapshot_overwrites_card_and_keeps_brevi_fields()
    {
        var supplier = Supplier.Create(SupplierId.From(7), "Тексика, ТОВ", notes: "домовились про знижку").Value;
        supplier.SetActive(false);

        supplier.ApplyKeepinCrmSnapshot(Snapshot(), SyncedAt);

        Assert.Multiple(() =>
        {
            Assert.That(supplier.KeepinCrmId, Is.EqualTo("104551"));
            Assert.That(supplier.Name, Is.EqualTo("Тексика"));
            Assert.That(supplier.Email, Is.EqualTo("a@teksika.com"));
            Assert.That(supplier.PhoneNumber, Is.EqualTo("+380501112233, 0532671109"));
            Assert.That(supplier.Tags, Is.EqualTo("опт"));
            Assert.That(supplier.KeepinSyncedAt, Is.EqualTo(SyncedAt));
            Assert.That(supplier.Notes, Is.EqualTo("домовились про знижку"));
            Assert.That(supplier.PurchaseCategories, Is.EqualTo("тканина"));
            Assert.That(supplier.IsActive, Is.False);
        });
    }

    [Test]
    public void Keepin_supplier_rejects_card_changes_but_accepts_notes_and_activity()
    {
        var supplier = Supplier.CreateFromKeepinCrm(SupplierId.From(7), Snapshot(), SyncedAt).Value;

        Assert.That(supplier.Update(
            "Інша назва", supplier.Link, supplier.ContactPerson, supplier.PhoneNumber, null, supplier.Email,
            supplier.PurchaseCategories).IsFailure, Is.True);
        Assert.That(supplier.Update(
            supplier.Name, supplier.Link, supplier.ContactPerson, supplier.PhoneNumber, null, supplier.Email, "фурнітура")
            .IsFailure, Is.True);

        supplier.SetNotes("нова нотатка");
        supplier.SetActive(false);
        Assert.Multiple(() =>
        {
            Assert.That(supplier.Notes, Is.EqualTo("нова нотатка"));
            Assert.That(supplier.IsActive, Is.False);
            Assert.That(supplier.PurchaseCategories, Is.EqualTo("тканина"));
        });
    }

    [Test]
    public void Keepin_snapshot_requires_id_and_truncates_long_values()
    {
        var supplier = Supplier.Create(SupplierId.From(7), "Тексика").Value;

        Assert.That(supplier.ApplyKeepinCrmSnapshot(Snapshot() with { KeepinCrmId = " " }, SyncedAt).IsFailure, Is.True);

        supplier.ApplyKeepinCrmSnapshot(Snapshot(new string('я', 250)) with { Tags = new string('t', 600) }, SyncedAt);
        Assert.Multiple(() =>
        {
            Assert.That(supplier.Name, Has.Length.EqualTo(Supplier.NameMaxLength));
            Assert.That(supplier.Tags, Has.Length.EqualTo(Supplier.TagsMaxLength));
        });
    }
}
