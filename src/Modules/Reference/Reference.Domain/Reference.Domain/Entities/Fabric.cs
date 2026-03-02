using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Exceptions;
using Reference.Domain.ValueObjects;
using Reference.Domain.Errors;

namespace Reference.Domain.Entities;

public class Fabric : BaseEntity<FabricId>, IAggregateRoot
{
    #region Properties
    public string Name { get; private set; }
    public int CounterpartyId { get; private set; }
    public Money Price { get; private set; }
    #endregion

    #region Constructors
    private Fabric(){}
    private Fabric(FabricId id, string name, int counterpartyId, Money price)
    {
        SetId(id);
        SetName(name);
        SetCounterpartyId(counterpartyId);
        SetPrice(price);
    }
    #endregion

    #region Factories
    public static Fabric Create(FabricId id, string name, int counterpartyId, Money price)
        => new(id, name, counterpartyId, price);

    public void Update(string name, int counterpartyId, Money price)
    {
        SetName(name);
        SetCounterpartyId(counterpartyId);
        SetPrice(price);
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

        Name = name;
    }

    private void SetCounterpartyId(int counterpartyId)
    {
        if (counterpartyId < 0)
            throw new DomainException(FabricErrors.CounterpartyIdIsRequired());

        CounterpartyId = counterpartyId;
    }

    private void SetPrice(Money price)
    {
        if (price.Amount < 0 || price.Amount > 10_000)
            throw new DomainException(FabricErrors.PriceOutOfRange(0, 10_000));

        Price = price;
    }
    #endregion
}
