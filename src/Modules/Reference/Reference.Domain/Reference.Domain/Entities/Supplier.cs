using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Exceptions;
using Reference.Domain.Errors;
using Reference.Domain.ValueObjects;

namespace Reference.Domain.Entities;

public class Supplier : BaseEntity<SupplierId>, IAggregateRoot
{
    #region Properties
    public string Name { get; private set; } = null!;
    public string? Link { get; private set; }
    public string? ContactPerson { get; private set; }
    public string? PhoneNumber { get; private set; }
    #endregion

    #region Constructors
    private Supplier() { }

    private Supplier(SupplierId id, string name, string? link, string? contactPerson, string? phoneNumber)
    {
        SetId(id);
        SetName(name);
        SetLink(link);
        SetContactPerson(contactPerson);
        SetPhoneNumber(phoneNumber);
    }
    #endregion

    #region Factories
    public static Supplier Create(
        SupplierId id,
        string name,
        string? link = null,
        string? contactPerson = null,
        string? phoneNumber = null) => new(id, name, link, contactPerson, phoneNumber);

    public void Update(
        string name,
        string? link = null,
        string? contactPerson = null,
        string? phoneNumber = null)
    {
        SetName(name);
        SetLink(link);
        SetContactPerson(contactPerson);
        SetPhoneNumber(phoneNumber);
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
    #endregion
}
