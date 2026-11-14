using GarmentAccessoryEntity = Reference.Domain.GarmentAccessories.GarmentAccessory;

namespace Reference.Application.Features.GarmentAccessory.Queries.GetGarmentAccessories;

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
                x.SupplierId.Value,
                x.Code,
                x.Unit,
                x.PriceUpdatedAt,
                x.IsActive,
                x.Comment));
    }
}
