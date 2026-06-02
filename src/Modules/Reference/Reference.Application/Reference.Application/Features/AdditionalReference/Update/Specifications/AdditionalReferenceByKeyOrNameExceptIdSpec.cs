using AdditionalReferenceEntity = Reference.Domain.Entities.AdditionalReference;

namespace Reference.Application.Features.AdditionalReference.Update.Specifications;

public sealed class AdditionalReferenceByKeyOrNameExceptIdSpec : Specification<AdditionalReferenceEntity>
{
    public AdditionalReferenceByKeyOrNameExceptIdSpec(int id, string key, string name)
    {
        Query.Where(x => x.Id.Value != id && (x.Key == key || x.Name == name));
    }
}
