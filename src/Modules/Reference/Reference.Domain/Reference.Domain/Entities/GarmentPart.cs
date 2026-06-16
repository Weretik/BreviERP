using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Exceptions;
using Reference.Domain.Errors;
using Reference.Domain.ValueObjects;

namespace Reference.Domain.Entities;

public class GarmentPart : BaseEntity<GarmentPartId>, IAggregateRoot
{
    #region Properties
    public string Name { get; private set; } = null!;
    public SupplierId SupplierId { get; private set; }
    public string? ContactPerson { get; private set; }
    public string? PhoneNumber { get; private set; }
    #endregion

    #region Constructors
    private GarmentPart() { }

    private GarmentPart(
        GarmentPartId id,
        string name,
        int supplierId = 1,
        string? contactPerson = null,
        string? phoneNumber = null)
    {
        SetId(id);
        SetName(name);
        SetSupplierId(supplierId);
        SetContactPerson(contactPerson);
        SetPhoneNumber(phoneNumber);
    }
    #endregion

    #region Factories
    public static GarmentPart Create(
        GarmentPartId id,
        string name,
        int supplierId = 1,
        string? contactPerson = null,
        string? phoneNumber = null) => new(id, name, supplierId, contactPerson, phoneNumber);

    public void Update(
        string name,
        int supplierId = 1,
        string? contactPerson = null,
        string? phoneNumber = null)
    {
        SetName(name);
        SetSupplierId(supplierId);
        SetContactPerson(contactPerson);
        SetPhoneNumber(phoneNumber);
    }
    #endregion

    #region Setters/Validation
    private void SetId(GarmentPartId id)
    {
        if (id.Value == default)
            throw new DomainException(GarmentPartErrors.IdIsRequired());

        Id = id;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(GarmentPartErrors.NameIsRequired());

        Name = name.Trim();
    }

    private void SetSupplierId(int supplierId)
    {
        if (supplierId <= 0)
            throw new DomainException(GarmentPartErrors.SupplierIdIsRequired());

        SupplierId = Reference.Domain.ValueObjects.SupplierId.From(supplierId);
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
