using Reference.Application.Features.Fabric.GetList.DTOs;
using FabricEntity = Reference.Domain.Entities.Fabric;

namespace Reference.Application.Features.Fabric.GetList.Specifications;

internal sealed class AllFabricsSpec : Specification<FabricEntity, FabricProjectionDTO>
{
    public AllFabricsSpec()
    {
        Query.AsNoTracking()
            .OrderBy(x => x.Id.Value)
            .Select(x => new FabricProjectionDTO(x.Id.Value, x.Name, x.Price.Value, x.ProviderId.Value));
    }
}

