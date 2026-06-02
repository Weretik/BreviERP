using Reference.Application.Features.GarmentPartOperation.GetList.DTOs;
using GarmentPartOperationEntity = Reference.Domain.Entities.GarmentPartOperation;

namespace Reference.Application.Features.GarmentPartOperation.GetList.Specifications;

internal sealed class GetGarmentPartOperationsSpec
    : Specification<GarmentPartOperationEntity, GarmentPartOperationProjectionDTO>
{
    public GetGarmentPartOperationsSpec()
    {
        Query.AsNoTracking()
            .OrderBy(x => x.GarmentPartId.Value)
            .ThenBy(x => x.Id.Value)
            .Select(x => new GarmentPartOperationProjectionDTO(x.Id.Value, x.GarmentPartId.Value, x.Name, x.Min));
    }
}
