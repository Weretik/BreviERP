using AdditionalReferenceEntity = Reference.Domain.Entities.AdditionalReference;
using Reference.Domain.ValueObjects;

namespace Reference.Application.Features.AdditionalReference.Update.Specifications;

public sealed class AdditionalReferenceByKeyOrNameExceptIdSpec : Specification<AdditionalReferenceEntity>
{
    public AdditionalReferenceByKeyOrNameExceptIdSpec(int id, string key, string name)
    {
        Query.Where(x => x.Id != AdditionalReferenceId.From(id) && (x.Key == key || x.Name == name));
    }
}
