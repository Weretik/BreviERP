using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Results;

using Reference.Domain.GarmentPartOperations;

namespace Reference.Domain.GarmentParts;

public class GarmentPart : ReferenceEntity<GarmentPartId>, IAggregateRoot
{
    #region Properties
    public string Name { get; private set; } = null!;
    #endregion

    #region Constructors
    private GarmentPart() { }
    #endregion

    #region Factories
    public static DomainResult<GarmentPart> Create(GarmentPartId id, string name)
    {
        if (id.Value == default) throw new ArgumentException("Garment part id is required.", nameof(id));
        var check = CheckName(name);
        if (check.IsFailure) return check.Error!;
        var part = new GarmentPart { Id = id, Name = name.Trim() };
        part.RecordCreated();
        return part;
    }

    public DomainResult Update(string name)
    {
        var nameBefore = Name;
        var check = CheckName(name);
        if (check.IsFailure) return check;
        Name = name.Trim();
        RecordChanged(nameBefore);
        return DomainResult.Success();
    }
    #endregion

    #region Rules
    private static DomainResult CheckName(string? name) =>
        string.IsNullOrWhiteSpace(name) ? GarmentPartErrors.NameIsRequired() : DomainResult.Success();
    #endregion
}
