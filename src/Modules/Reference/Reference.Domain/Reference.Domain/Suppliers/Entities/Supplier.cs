using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Exceptions;
using Reference.Domain.Suppliers.Errors;
using Reference.Domain.Suppliers.ValueObjects;

namespace Reference.Domain.Suppliers.Entities;

/// <summary>
/// A supplier of fabrics, accessories, PPE or services. Contacts of a supplier linked to KeepinCRM
/// are owned by KeepinCRM and change only through synchronization; purchase categories, activity and
/// notes are always owned by Brevi.
/// </summary>
public class Supplier : BaseEntity<SupplierId>, IAggregateRoot
{
    public const int NameMaxLength = 200;
    public const int LinkMaxLength = 2048;
    public const int ContactPersonMaxLength = 200;
    public const int PhoneNumberMaxLength = 32;
    public const int EmailMaxLength = 254;
    public const int PurchaseCategoriesMaxLength = 200;
    public const int TagsMaxLength = 500;
    public const int NotesMaxLength = 500;
    public const int KeepinCrmIdMaxLength = 32;

    #region Properties
    public string Name { get; private set; } = null!;
    public string? Link { get; private set; }
    public string? ContactPerson { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Email { get; private set; }
    public string? PurchaseCategories { get; private set; }
    public string? Tags { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? Notes { get; private set; }
    public string? KeepinCrmId { get; private set; }
    public DateTimeOffset? KeepinSyncedAt { get; private set; }

    public bool IsManagedByKeepinCrm => KeepinCrmId is not null;
    #endregion

    #region Constructors
    private Supplier() { }

    private Supplier(
        SupplierId id,
        string name,
        string? link,
        string? contactPerson,
        string? phoneNumber,
        string? notes)
    {
        SetId(id);
        SetContacts(name, link, contactPerson, phoneNumber, Email);
        SetNotes(notes);
    }
    #endregion

    #region Factories
    public static Supplier Create(
        SupplierId id,
        string name,
        string? link = null,
        string? contactPerson = null,
        string? phoneNumber = null,
        string? notes = null) => new(id, name, link, contactPerson, phoneNumber, notes);

    public static Supplier CreateFromKeepinCrm(SupplierId id, KeepinCrmSupplierSnapshot snapshot, DateTimeOffset syncedAt)
    {
        var supplier = new Supplier(id, snapshot.Name, null, null, null, null);
        supplier.ApplyKeepinCrmSnapshot(snapshot, syncedAt);
        return supplier;
    }

    /// <summary>Updates a manually managed supplier. Contacts of a KeepinCRM supplier cannot change here.</summary>
    public void Update(
        string name,
        string? link = null,
        string? contactPerson = null,
        string? phoneNumber = null,
        string? notes = null,
        string? email = null)
    {
        if (IsManagedByKeepinCrm && ContactsDiffer(name, link, contactPerson, phoneNumber, email))
            throw new DomainException(SupplierErrors.ContactsManagedByKeepinCrm());

        SetContacts(name, link, contactPerson, phoneNumber, email);
        SetNotes(notes);
    }

    /// <summary>Updates the fields Brevi always owns.</summary>
    public void UpdatePurchasing(string? purchaseCategories, bool isActive)
    {
        PurchaseCategories = Normalize(purchaseCategories, PurchaseCategoriesMaxLength);
        IsActive = isActive;
    }

    /// <summary>Overwrites KeepinCRM-owned fields with the current KeepinCRM state.</summary>
    public void ApplyKeepinCrmSnapshot(KeepinCrmSupplierSnapshot snapshot, DateTimeOffset syncedAt)
    {
        if (string.IsNullOrWhiteSpace(snapshot.KeepinCrmId))
            throw new DomainException(SupplierErrors.KeepinCrmIdIsRequired());

        KeepinCrmId = Normalize(snapshot.KeepinCrmId, KeepinCrmIdMaxLength);
        SetContacts(snapshot.Name, snapshot.Link, snapshot.ContactPerson, snapshot.PhoneNumber, snapshot.Email);
        Tags = Normalize(snapshot.Tags, TagsMaxLength);
        KeepinSyncedAt = syncedAt;
    }
    #endregion

    #region Setters/Validation
    private bool ContactsDiffer(string name, string? link, string? contactPerson, string? phoneNumber, string? email) =>
        !string.Equals(Name, name?.Trim(), StringComparison.Ordinal)
        || Link != Normalize(link, LinkMaxLength)
        || ContactPerson != Normalize(contactPerson, ContactPersonMaxLength)
        || PhoneNumber != Normalize(phoneNumber, PhoneNumberMaxLength)
        || Email != Normalize(email, EmailMaxLength);

    private void SetId(SupplierId id)
    {
        if (id.Value == default)
            throw new DomainException(SupplierErrors.IdIsRequired());

        Id = id;
    }

    private void SetContacts(string name, string? link, string? contactPerson, string? phoneNumber, string? email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(SupplierErrors.NameIsRequired());

        Name = Truncate(name.Trim(), NameMaxLength);
        Link = Normalize(link, LinkMaxLength);
        ContactPerson = Normalize(contactPerson, ContactPersonMaxLength);
        PhoneNumber = Normalize(phoneNumber, PhoneNumberMaxLength);
        Email = Normalize(email, EmailMaxLength);
    }

    private void SetNotes(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            Notes = null;
            return;
        }

        var normalizedNotes = notes.Trim();
        if (normalizedNotes.Length > NotesMaxLength)
            throw new DomainException(SupplierErrors.NotesLengthInvalid(normalizedNotes.Length));

        Notes = normalizedNotes;
    }

    private static string? Normalize(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        return Truncate(trimmed, maxLength);
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
    #endregion
}
