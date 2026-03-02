using Crm.Domain.Enums;
using Crm.Domain.ValueObjects;
using Crm.Domain.Errors;
using BuildingBlocks.Domain.Exceptions;

namespace Crm.Domain.Entities;

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
    private void SetId(CounterpartyId id)
    {
        if (id.Value == default)
            throw new DomainException(CounterpartyErrors.IdIsRequired());

        Id = id;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(CounterpartyErrors.NameIsRequired());

        Name = name;
    }

    private void SetType(CounterpartyType type)
    {
        if (type is null)
            throw new DomainException(CounterpartyErrors.TypeIsRequired());

        Type = type;
    }
    #endregion
}
