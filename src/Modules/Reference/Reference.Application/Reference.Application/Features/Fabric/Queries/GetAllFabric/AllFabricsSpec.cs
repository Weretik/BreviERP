using FabricEntity = Reference.Domain.Fabrics.Fabric;

namespace Reference.Application.Features.Fabric.Queries.GetAllFabric;

internal sealed class AllFabricsSpec : Specification<FabricEntity, FabricProjectionDTO>
{
    public AllFabricsSpec()
    {
        Query.AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => new FabricProjectionDTO(
                x.Id.Value, x.Name, x.Price.Value, x.ProviderId.Value, x.Code, x.NameRu, x.Composition,
                x.CompositionRu, x.Density, x.PriceUpdatedAt, x.IsActive, x.Comment, x.ProposalDescription));
    }
}
