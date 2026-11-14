using Reference.Application.Features.Fabric.Shared.DTOs;
using Reference.Domain.GarmentAccessories;
using Reference.Domain.Suppliers;

namespace Reference.Application.Features.Fabric.Shared;

internal static class FabricDetailsFactory
{
    public static FabricDetails From(IFabricFields fields, SupplierId providerId) => new(
        fields.Code, fields.Name, fields.NameRu, providerId, fields.Composition, fields.CompositionRu,
        fields.Density, fields.IsActive, fields.Comment, fields.ProposalDescription);
}
