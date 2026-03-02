using Application.Reference.GetAdditionalReference;
using Reference.Domain.Entities;

namespace Reference.Application.GetAdditionalReference.Specifications;

public class GetAdditionalReferenceSpec : Specification<AdditionalReference, AdditionalReferenceRowDTO>
{
    public GetAdditionalReferenceSpec()
    {
        Query.AsNoTracking()
            .Select(x=>
                new AdditionalReferenceRowDTO(x.Id.Value, x.Name, x.Value, x.Unit, x.Description));
    }
}
