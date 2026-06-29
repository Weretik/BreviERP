using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Exceptions;
using Reference.Domain.Suppliers.Errors;
using Reference.Domain.Suppliers.ValueObjects;

namespace Reference.Domain.Suppliers.Entities;

public class Supplier : BaseEntity<SupplierId>, IAggregateRoot
{
    private const int NotesMaxLength = 500;

    #region Properties
    public string Name { get; private set; } = null!;
    public string? Link { get; private set; }
    public string? ContactPerson { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Notes { get; private set; }
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
        SetName(name);
        SetLink(link);
        SetContactPerson(contactPerson);
        SetPhoneNumber(phoneNumber);
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

    public void Update(
        string name,
        string? link = null,
        string? contactPerson = null,
        string? phoneNumber = null,
        string? notes = null)
    {
        SetName(name);
        SetLink(link);
        SetContactPerson(contactPerson);
        SetPhoneNumber(phoneNumber);
        SetNotes(notes);
    }
    #endregion

    #region Setters/Validation
    private void SetId(SupplierId id)
    {
        if (id.Value == default)
            throw new DomainException(SupplierErrors.IdIsRequired());

        Id = id;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(SupplierErrors.NameIsRequired());

        Name = name.Trim();
    }

    private void SetLink(string? link)
    {
        Link = string.IsNullOrWhiteSpace(link) ? null : link.Trim();
    }

    private void SetContactPerson(string? contactPerson)
    {
        ContactPerson = string.IsNullOrWhiteSpace(contactPerson) ? null : contactPerson.Trim();
    }

    private void SetPhoneNumber(string? phoneNumber)
    {
        PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();
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
    #endregion
}
