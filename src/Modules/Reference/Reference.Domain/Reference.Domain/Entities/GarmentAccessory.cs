using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Exceptions;
using Reference.Domain.Errors;
using Reference.Domain.ValueObjects;

namespace Reference.Domain.Entities;

public class GarmentAccessory : BaseEntity<GarmentAccessoryId>, IAggregateRoot
{
    #region Properties
    public string Name { get; private set; } = null!;
    public MoneyAmount Price { get; private set; }
    public SupplierId SupplierId { get; private set; }
    #endregion

    #region Constructors
    private GarmentAccessory() { }

    private GarmentAccessory(GarmentAccessoryId id, string name, MoneyAmount price, int supplierId = 1)
    {
        SetId(id);
        SetName(name);
        SetPrice(price);
        SetSupplierId(supplierId);
    }
    #endregion

    #region Factories
    public static GarmentAccessory Create(
        GarmentAccessoryId id,
        string name,
        MoneyAmount price,
        int supplierId = 1)
        => new(id, name, price, supplierId);

    public void Update(string name, MoneyAmount price, int supplierId = 1)
    {
        SetName(name);
        SetPrice(price);
        SetSupplierId(supplierId);
    }
    #endregion

    #region Setters/Validation
    private void SetId(GarmentAccessoryId id)
    {
        if (id.Value == default)
            throw new DomainException(GarmentAccessoryErrors.IdIsRequired());

        Id = id;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(GarmentAccessoryErrors.NameIsRequired());

        Name = name.Trim();
    }

    private void SetPrice(MoneyAmount price)
    {
        if (price.Value < 0 || price.Value > 10_000)
            throw new DomainException(GarmentAccessoryErrors.PriceOutOfRange(0, 10_000));

        Price = price;
    }

    private void SetSupplierId(int supplierId)
    {
        if (supplierId <= 0)
            throw new DomainException(GarmentAccessoryErrors.SupplierIdIsRequired());

        SupplierId = Reference.Domain.ValueObjects.SupplierId.From(supplierId);
    }
    #endregion
}
