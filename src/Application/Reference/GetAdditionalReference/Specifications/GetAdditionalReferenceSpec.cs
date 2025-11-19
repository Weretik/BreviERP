using Domain.Reference.Entities;
using Domain.Reference.ValueObjects;

namespace Application.Reference.GetAdditionalReference;

public class GetAdditionalReferenceSpec : Specification<AdditionalReference, AdditionalReferenceRowDTO>
{
    public GetAdditionalReferenceSpec()
    {
        Query.AsNoTracking()
            .Select(x=>
                new AdditionalReferenceRowDTO(x.Id.Value, x.Name, x.Value, x.Unit, x.Description));
    }
}
