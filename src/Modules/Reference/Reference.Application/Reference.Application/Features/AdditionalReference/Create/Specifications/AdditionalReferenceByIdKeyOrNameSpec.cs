using AdditionalReferenceEntity = Reference.Domain.Entities.AdditionalReference;
using Reference.Domain.ValueObjects;

namespace Reference.Application.Features.AdditionalReference.Create.Specifications;

public sealed class AdditionalReferenceByIdKeyOrNameSpec : Specification<AdditionalReferenceEntity>
{
    public AdditionalReferenceByIdKeyOrNameSpec(int id, string key, string name)
    {
        Query.Where(x => x.Id == AdditionalReferenceId.From(id) || x.Key == key || x.Name == name);
    }
}
