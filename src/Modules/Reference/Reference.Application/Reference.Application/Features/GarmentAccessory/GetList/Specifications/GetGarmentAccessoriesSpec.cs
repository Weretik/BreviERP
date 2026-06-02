using Reference.Application.Features.GarmentAccessory.GetList.DTOs;
using GarmentAccessoryEntity = Reference.Domain.Entities.GarmentAccessory;

namespace Reference.Application.Features.GarmentAccessory.GetList.Specifications;

public sealed class GetGarmentAccessoriesSpec : Specification<GarmentAccessoryEntity, GarmentAccessoryRowDTO>
{
    public GetGarmentAccessoriesSpec()
    {
        Query.AsNoTracking()
            .OrderBy(x => x.Id.Value)
            .Select(x => new GarmentAccessoryRowDTO(x.Id.Value, x.Name, x.Price.Value));
    }
}
