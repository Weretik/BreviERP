using GarmentPartEntity = Reference.Domain.Entities.GarmentPart;

namespace Reference.Application.Features.GarmentPart.GetList.Specifications;

internal sealed class GetGarmentPartsSpec : Specification<GarmentPartEntity>
{
    public GetGarmentPartsSpec()
    {
        Query.AsNoTracking()
            .OrderBy(x => x.Id);
    }
}
