using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Exceptions;
using Reference.Domain.ValueObjects;
using Reference.Domain.Errors;

namespace Reference.Domain.Entities;

public class AdditionalReference : BaseEntity<AdditionalReferenceId>, IAggregateRoot
{
    private static readonly string[] AllowedUnits = ["шт.", "грн.", "%"];

    #region Properties
    public string Name { get; private set; } = null!;
    public string Key { get; private set; } = null!;
    public decimal Value { get; private set; }
    public string Unit { get; private set; } = null!;
    public string? Description { get; private set; }
    #endregion

    #region Constructors
    private AdditionalReference(){}
    private AdditionalReference(AdditionalReferenceId id, string name, string key, decimal value, string unit,
        string? description = null)
    {
        SetId(id);
        SetName(name);
        SetKey(key);
        SetValue(value);
        SetUnit(unit);
        SetDescription(description);
    }
    #endregion

    #region Factories
    public static AdditionalReference Create(AdditionalReferenceId id, string name, string key, decimal value, string unit,
        string? description = null)
        => new(id, name, key, value, unit, description);
    public void Update(string name, string key, decimal value, string unit, string? description = null)
    {
        SetName(name);
        SetKey(key);
        SetValue(value);
        SetUnit(unit);
        SetDescription(description);
    }
    #endregion

    #region Setters/Validation
    private void SetId(AdditionalReferenceId id)
    {
        if (id.Value == default)
            throw new DomainException(AdditionalReferenceErrors.IdIsRequired());

        Id = id;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(AdditionalReferenceErrors.NameIsRequired());

        Name = name;
    }

    private void SetKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new DomainException(AdditionalReferenceErrors.KeyIsRequired());

        Key = key;
    }

    private void SetValue(decimal value)
    {
        if (value < 0)
            throw new DomainException(AdditionalReferenceErrors.ValueMustBeNonNegative());

        Value = value;
    }

    private void SetUnit(string unit)
    {
        if (string.IsNullOrWhiteSpace(unit))
            throw new DomainException(AdditionalReferenceErrors.UnitIsRequired());

        if (!AllowedUnits.Contains(unit))
            throw new DomainException(AdditionalReferenceErrors.UnitIsInvalid());

        Unit = unit;
    }

    private void SetDescription(string? description) => Description = description;
    #endregion
}
