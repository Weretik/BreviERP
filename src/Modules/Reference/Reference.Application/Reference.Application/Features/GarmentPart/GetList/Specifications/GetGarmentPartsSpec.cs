using Reference.Application.Features.GarmentPart.GetList.DTOs;
using GarmentPartEntity = Reference.Domain.GarmentPartOperations.Entities.GarmentPart;

namespace Reference.Application.Features.GarmentPart.GetList.Specifications;

public sealed class GetGarmentPartsSpec : Specification<GarmentPartEntity, GarmentPartRowDTO>
{
    public GetGarmentPartsSpec()
    {
        Query.AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => new GarmentPartRowDTO(x.Id.Value, x.Name));
    }
}
