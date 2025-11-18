using Domain.CRM.Enums;
using Domain.CRM.ValueObjects;

namespace Domain.CRM.Entities;

public class Counterparty : BaseEntity<CounterpartyId>, IAggregateRoot
{
    #region Properties
    public CounterpartyType Type { get; private set; }
    public string Name { get; private set; }
    #endregion

    #region Constructors
    private Counterparty() { }
    private Counterparty(CounterpartyId id, CounterpartyType type, string name)
    {
        SetId(id);
        SetType(type);
        SetName(name);
    }
    #endregion

    #region Factories
    public static Counterparty Create(CounterpartyId id, CounterpartyType type, string name)
        => new(id, type, name);
    public void Update(CounterpartyType type, string name)
    {
        SetType(type);
        SetName(name);
    }
    #endregion

    #region Validation & Setters
    private void SetId(CounterpartyId id) => Id = Guard.Against.Default(id, nameof(id));
    private void SetName(string name) => Name = Guard.Against.NullOrWhiteSpace(name, nameof(name));
    private void SetType(CounterpartyType type) => Type = Guard.Against.Default(type, nameof(type));
    #endregion
}
