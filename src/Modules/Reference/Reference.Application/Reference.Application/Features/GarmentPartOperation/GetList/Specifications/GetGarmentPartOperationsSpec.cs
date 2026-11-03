using Reference.Application.Features.GarmentPartOperation.GetList.DTOs;
using GarmentPartOperationEntity = Reference.Domain.GarmentPartOperations.Entities.GarmentPartOperation;

namespace Reference.Application.Features.GarmentPartOperation.GetList.Specifications;

internal sealed class GetGarmentPartOperationsSpec
    : Specification<GarmentPartOperationEntity, GarmentPartOperationProjectionDTO>
{
    public GetGarmentPartOperationsSpec()
    {
        Query.AsNoTracking()
            .OrderBy(x => x.Code == null)
            .ThenBy(x => x.Code)
            .ThenBy(x => x.Id)
            .Select(x => new GarmentPartOperationProjectionDTO(x.Id.Value, x.GarmentPartId.Value, x.Name, x.Min,
                x.Code, x.Element, x.Variant, x.MinutesPerProduct, x.IsActive, x.Notes));
    }
}
