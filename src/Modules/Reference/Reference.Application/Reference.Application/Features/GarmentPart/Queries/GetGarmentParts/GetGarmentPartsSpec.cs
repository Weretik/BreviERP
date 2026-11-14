using GarmentPartEntity = Reference.Domain.GarmentPartOperations.GarmentPart;

namespace Reference.Application.Features.GarmentPart.Queries.GetGarmentParts;

public sealed class GetGarmentPartsSpec : Specification<GarmentPartEntity, GarmentPartRowDTO>
{
    public GetGarmentPartsSpec()
    {
        Query.AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => new GarmentPartRowDTO(x.Id.Value, x.Name));
    }
}
