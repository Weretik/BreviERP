using AdditionalReferenceEntity = Reference.Domain.Entities.AdditionalReference;

namespace Reference.Application.Features.AdditionalReference.Create.Specifications;

public sealed class AdditionalReferenceByKeySpec : Specification<AdditionalReferenceEntity>
{
    public AdditionalReferenceByKeySpec(string key)
    {
        Query.Where(x => x.Key == key);
    }
}
