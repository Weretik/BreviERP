using Reference.Application.Features.AdditionalReference.GetList.DTOs;
using AdditionalReferenceEntity = Reference.Domain.AdditionalReferences.Entities.AdditionalReference;

namespace Reference.Application.Features.AdditionalReference.GetList.Specifications;

public class GetAdditionalReferenceSpec : Specification<AdditionalReferenceEntity, AdditionalReferenceRowDTO>
{
    public GetAdditionalReferenceSpec()
    {
        Query.AsNoTracking()
            .Select(x =>
                new AdditionalReferenceRowDTO(x.Id.Value, x.Name, x.Key, x.Value, x.Unit, x.Description));
    }
}
