using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using Reference.Domain.ValueObjects;

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
    private void SetId(FabricId id) => Id = Guard.Against.Default(id, nameof(id));
    private void SetName(string name) => Name = Guard.Against.NullOrWhiteSpace(name, nameof(name));
    private void SetCounterpartyId(int counterpartyId) => CounterpartyId = Guard.Against.Negative(counterpartyId, nameof(counterpartyId));
    private void SetPrice(Money price)
    {
        Guard.Against.OutOfRange(price.Amount, nameof(price), 0, 10_000);
        Price = price;
    }
    #endregion
}
