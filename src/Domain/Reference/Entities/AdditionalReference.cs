using Domain.Reference.ValueObjects;

namespace Domain.Reference.Entities;

public class AdditionalReference : BaseEntity<AdditionalReferenceId>, IAggregateRoot
{
    #region Properties
    public string Name { get; private set; }
    public decimal Value { get; private set; }
    public string? Description { get; private set; }
    #endregion

    #region Constructors & Factories
    private AdditionalReference(){}
    private AdditionalReference(AdditionalReferenceId id, string name, decimal value, string? description = null)
    {
        SetId(id);
        SetName(name);
        SetValue(value);
        SetDescription(description);
    }

    public static AdditionalReference Create(AdditionalReferenceId id, string name, decimal value, string? description = null)
        => new(id, name, value, description);
    public void Update(string name, decimal value, string? description = null)
    {
        SetName(name);
        SetValue(value);
        SetDescription(description);
    }
    #endregion

    #region Setters/Validation
    private void SetId(AdditionalReferenceId id) => Id = Guard.Against.Default(id, nameof(id));
    private void SetName(string name) => Name = Guard.Against.NullOrWhiteSpace(name, nameof(name));
    private void SetValue(decimal value) => Value = Guard.Against.Negative(value, nameof(value));
    private void SetDescription(string? description) => Description = description;
    #endregion
}
