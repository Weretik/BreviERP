using AdditionalReferenceEntity = Reference.Domain.AdditionalReferences.Entities.AdditionalReference;

namespace Reference.Application.Features.AdditionalReference.Create.Specifications;

public sealed class AdditionalReferenceByKeySpec : Specification<AdditionalReferenceEntity>
{
    public AdditionalReferenceByKeySpec(string key)
    {
        Query.Where(x => x.Key == key);
    }
}
