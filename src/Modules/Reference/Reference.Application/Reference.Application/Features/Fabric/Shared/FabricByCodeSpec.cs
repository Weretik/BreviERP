using Reference.Domain.GarmentAccessories;
using FabricEntity = Reference.Domain.Fabrics.Fabric;
using Reference.Domain.Fabrics;

namespace Reference.Application.Features.Fabric.Shared;

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
