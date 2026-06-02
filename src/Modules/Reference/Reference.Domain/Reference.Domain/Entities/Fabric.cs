using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Exceptions;
using Reference.Domain.Errors;
using Reference.Domain.ValueObjects;

namespace Reference.Domain.Entities;

public class Fabric : BaseEntity<FabricId>, IAggregateRoot
{
    #region Properties
    public string Name { get; private set; } = null!;
    public MoneyAmount Price { get; private set; }
    public SupplierId ProviderId { get; private set; }
    #endregion

    #region Constructors
    private Fabric() { }

    private Fabric(FabricId id, string name, MoneyAmount price, SupplierId providerId)
    {
        SetId(id);
        SetName(name);
        SetPrice(price);
        SetProviderId(providerId);
    }
    #endregion

    #region Factories
    public static Fabric Create(FabricId id, string name, MoneyAmount price, SupplierId providerId)
        => new(id, name, price, providerId);

    public void Update(string name, MoneyAmount price, SupplierId providerId)
    {
        SetName(name);
        SetPrice(price);
        SetProviderId(providerId);
    }
    #endregion

    #region Setters/Validation
    private void SetId(FabricId id)
    {
        if (id.Value == default)
            throw new DomainException(FabricErrors.IdIsRequired());

        Id = id;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(FabricErrors.NameIsRequired());

        Name = name.Trim();
    }

    private void SetPrice(MoneyAmount price)
    {
        if (price.Value < 0 || price.Value > 10_000)
            throw new DomainException(FabricErrors.PriceOutOfRange(0, 10_000));

        Price = price;
    }

    private void SetProviderId(SupplierId providerId)
    {
        if (providerId.Value == default)
            throw new DomainException(FabricErrors.ProviderIdIsRequired());

        ProviderId = providerId;
    }
    #endregion
}

