using Domain.Reference.Entities;
using Domain.Reference.ValueObjects;

namespace Application.Reference.GetAdditionalReference;

public class GetAdditionalReferenceSpec : Specification<AdditionalReference>
{
    public GetAdditionalReferenceSpec()
    {
        Query.AsNoTracking()
            .Where(x => x.Id == AdditionalReferenceId.From(1));
    }
}
