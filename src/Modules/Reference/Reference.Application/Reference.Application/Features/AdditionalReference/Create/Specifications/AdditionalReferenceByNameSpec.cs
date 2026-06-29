using AdditionalReferenceEntity = Reference.Domain.AdditionalReferences.Entities.AdditionalReference;

namespace Reference.Application.Features.AdditionalReference.Create.Specifications;

public sealed class AdditionalReferenceByNameSpec : Specification<AdditionalReferenceEntity>
{
    public AdditionalReferenceByNameSpec(string name)
    {
        Query.Where(x => x.Name == name);
    }
}
