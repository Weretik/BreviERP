using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Results;

namespace Reference.Domain.Suppliers;

/// <summary>
/// A supplier of fabrics, accessories, PPE or services. For a supplier linked to KeepinCRM, the card fields
/// (name, purchase categories, contacts, site, tags) are owned by KeepinCRM and change only through
/// synchronization; activity («Використовувати в проєкті») and notes are always owned by Brevi.
/// </summary>
public class Supplier : BaseEntity<SupplierId>, IAggregateRoot
{
    public const int NameMaxLength = 200;
    public const int LinkMaxLength = 2048;
    public const int ContactPersonMaxLength = 200;
    public const int PhoneNumberMaxLength = 100;
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
    #endregion

    #region Factories
    public static DomainResult<Supplier> Create(
        SupplierId id,
        string name,
        string? link = null,
        string? contactPerson = null,
        string? phoneNumber = null,
        string? notes = null)
    {
        RequireId(id);
        var check = DomainResult.Combine(CheckName(name), CheckNotes(notes));
        if (check.IsFailure) return check.Error!;
        var supplier = new Supplier { Id = id };
        supplier.SetCard(name, null, link, contactPerson, phoneNumber, null);
        supplier.Notes = Normalize(notes, NotesMaxLength);
        return supplier;
    }

    public static DomainResult<Supplier> CreateFromKeepinCrm(SupplierId id, KeepinCrmSupplierSnapshot snapshot,
        DateTimeOffset syncedAt)
    {
        RequireId(id);
        var supplier = new Supplier { Id = id };
        var applied = supplier.ApplyKeepinCrmSnapshot(snapshot, syncedAt);
        return applied.IsFailure ? applied.Error! : supplier;
    }

    /// <summary>
    /// Updates the card from the admin. For a KeepinCRM supplier the card fields must stay as they are;
    /// only notes change here.
    /// </summary>
    public DomainResult Update(
        string name,
        string? link = null,
        string? contactPerson = null,
        string? phoneNumber = null,
        string? notes = null,
        string? email = null,
        string? purchaseCategories = null)
    {
        if (IsManagedByKeepinCrm && CardDiffers(name, purchaseCategories, link, contactPerson, phoneNumber, email))
            return SupplierErrors.ContactsManagedByKeepinCrm();
        var check = DomainResult.Combine(CheckName(name), CheckNotes(notes));
        if (check.IsFailure) return check;

        SetCard(name, purchaseCategories, link, contactPerson, phoneNumber, email);
        Notes = Normalize(notes, NotesMaxLength);
        return DomainResult.Success();
    }

    /// <summary>«Використовувати в проєкті»: always owned by Brevi.</summary>
    public void SetActive(bool isActive) => IsActive = isActive;

    /// <summary>Overwrites KeepinCRM-owned fields with the current KeepinCRM state. Activity and notes stay.</summary>
    public DomainResult ApplyKeepinCrmSnapshot(KeepinCrmSupplierSnapshot snapshot, DateTimeOffset syncedAt)
    {
        if (string.IsNullOrWhiteSpace(snapshot.KeepinCrmId)) return SupplierErrors.KeepinCrmIdIsRequired();
        var name = CheckName(snapshot.Name);
        if (name.IsFailure) return name;

        KeepinCrmId = Normalize(snapshot.KeepinCrmId, KeepinCrmIdMaxLength);
        SetCard(
            snapshot.Name,
            snapshot.PurchaseCategories,
            snapshot.Link,
            snapshot.ContactPerson,
            snapshot.PhoneNumber,
            snapshot.Email);
        Tags = Normalize(snapshot.Tags, TagsMaxLength);
        KeepinSyncedAt = syncedAt;
        return DomainResult.Success();
    }

    /// <summary>Notes are always owned by Brevi.</summary>
    public DomainResult SetNotes(string? notes)
    {
        var check = CheckNotes(notes);
        if (check.IsFailure) return check;
        Notes = Normalize(notes, NotesMaxLength);
        return DomainResult.Success();
    }
    #endregion

    #region Rules
    private static DomainResult CheckName(string? name) =>
        string.IsNullOrWhiteSpace(name) ? SupplierErrors.NameIsRequired() : DomainResult.Success();

    private static DomainResult CheckNotes(string? notes) =>
        notes?.Trim() is { Length: > NotesMaxLength } trimmed
            ? SupplierErrors.NotesLengthInvalid(trimmed.Length)
            : DomainResult.Success();

    private static void RequireId(SupplierId id)
    {
        if (id.Value == default) throw new ArgumentException("Supplier id is required.", nameof(id));
    }

    private bool CardDiffers(
        string name,
        string? purchaseCategories,
        string? link,
        string? contactPerson,
        string? phoneNumber,
        string? email) =>
        !string.Equals(Name, Normalize(name, NameMaxLength), StringComparison.Ordinal)
        || PurchaseCategories != Normalize(purchaseCategories, PurchaseCategoriesMaxLength)
        || Link != Normalize(link, LinkMaxLength)
        || ContactPerson != Normalize(contactPerson, ContactPersonMaxLength)
        || PhoneNumber != Normalize(phoneNumber, PhoneNumberMaxLength)
        || Email != Normalize(email, EmailMaxLength);

    private void SetCard(
        string name,
        string? purchaseCategories,
        string? link,
        string? contactPerson,
        string? phoneNumber,
        string? email)
    {
        Name = Normalize(name, NameMaxLength)!;
        PurchaseCategories = Normalize(purchaseCategories, PurchaseCategoriesMaxLength);
        Link = Normalize(link, LinkMaxLength);
        ContactPerson = Normalize(contactPerson, ContactPersonMaxLength);
        PhoneNumber = Normalize(phoneNumber, PhoneNumberMaxLength);
        Email = Normalize(email, EmailMaxLength);
    }

    private static string? Normalize(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength].TrimEnd();
    }
    #endregion
}
