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
    #endregion

    #region Constructors
    private GarmentPart() { }

    private GarmentPart(GarmentPartId id, string name)
    {
        SetId(id);
        SetName(name);
    }
    #endregion

    #region Factories
    public static GarmentPart Create(GarmentPartId id, string name) => new(id, name);

    public void Update(string name) => SetName(name);
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
    #endregion
}
