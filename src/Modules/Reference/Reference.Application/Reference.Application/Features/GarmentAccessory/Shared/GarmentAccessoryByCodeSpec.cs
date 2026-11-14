using Reference.Domain.GarmentAccessories;
using GarmentAccessoryEntity = Reference.Domain.GarmentAccessories.GarmentAccessory;

namespace Reference.Application.Features.GarmentAccessory.Shared;

/// <summary>Another accessory with this code; <paramref name="exceptId"/> skips the accessory being edited.</summary>
public sealed class GarmentAccessoryByCodeSpec : Specification<GarmentAccessoryEntity>
{
    public GarmentAccessoryByCodeSpec(string code, int? exceptId = null)
    {
        Query.Where(x => x.Code == code);
        if (exceptId is { } id)
            Query.Where(x => x.Id != GarmentAccessoryId.From(id));
    }
}
