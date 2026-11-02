using Reference.Domain.GarmentAccessories.ValueObjects;
using FabricEntity = Reference.Domain.GarmentAccessories.Entities.Fabric;

namespace Reference.Application.Features.Fabric.Shared.Specifications;

/// <summary>Another fabric with this code; <paramref name="exceptId"/> skips the fabric being edited.</summary>
public sealed class FabricByCodeSpec : Specification<FabricEntity>
{
    public FabricByCodeSpec(string code, int? exceptId = null)
    {
        Query.Where(x => x.Code == code);
        if (exceptId is { } id)
            Query.Where(x => x.Id != FabricId.From(id));
    }
}
