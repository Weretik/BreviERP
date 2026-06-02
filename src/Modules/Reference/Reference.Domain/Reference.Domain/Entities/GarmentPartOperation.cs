using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Exceptions;
using Reference.Domain.Errors;
using Reference.Domain.ValueObjects;

namespace Reference.Domain.Entities;

public class GarmentPartOperation : BaseEntity<GarmentPartOperationId>, IAggregateRoot
{
    #region Properties
    public GarmentPartId GarmentPartId { get; private set; }
    public string Name { get; private set; } = null!;
    public decimal Min { get; private set; }
    #endregion

    #region Constructors
    private GarmentPartOperation() { }

    private GarmentPartOperation(GarmentPartOperationId id, GarmentPartId garmentPartId, string name, decimal min)
    {
        SetId(id);
        SetGarmentPartId(garmentPartId);
        SetName(name);
        SetMin(min);
    }
    #endregion

    #region Factories
    public static GarmentPartOperation Create(
        GarmentPartOperationId id,
        GarmentPartId garmentPartId,
        string name,
        decimal min) => new(id, garmentPartId, name, min);

    public void Update(GarmentPartId garmentPartId, string name, decimal min)
    {
        SetGarmentPartId(garmentPartId);
        SetName(name);
        SetMin(min);
    }
    #endregion

    #region Setters/Validation
    private void SetId(GarmentPartOperationId id)
    {
        if (id.Value == default)
            throw new DomainException(GarmentPartOperationErrors.IdIsRequired());

        Id = id;
    }

    private void SetGarmentPartId(GarmentPartId garmentPartId)
    {
        if (garmentPartId.Value == default)
            throw new DomainException(GarmentPartOperationErrors.GarmentPartIdIsRequired());

        GarmentPartId = garmentPartId;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(GarmentPartOperationErrors.NameIsRequired());

        Name = name.Trim();
    }

    private void SetMin(decimal min)
    {
        if (min < 0)
            throw new DomainException(GarmentPartOperationErrors.MinMustBeNonNegative());

        Min = min;
    }
    #endregion
}
