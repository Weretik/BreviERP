using Reference.Application.Features.GarmentAccessory.GetList.DTOs;
using GarmentAccessoryEntity = Reference.Domain.GarmentAccessories.Entities.GarmentAccessory;

namespace Reference.Application.Features.GarmentAccessory.GetList.Specifications;

internal sealed class GetGarmentAccessoriesSpec : Specification<GarmentAccessoryEntity, GarmentAccessoryProjectionDTO>
{
    public GetGarmentAccessoriesSpec()
    {
        Query.AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => new GarmentAccessoryProjectionDTO(
                x.Id.Value,
                x.Name,
                x.Price.Value,
                x.SupplierId.Value));
    }
}
