using AdditionalReferenceEntity = Reference.Domain.Entities.AdditionalReference;
using Reference.Domain.ValueObjects;

namespace Reference.Application.Features.AdditionalReference.Update.Specifications;

public sealed class AdditionalReferenceByIdSpec : Specification<AdditionalReferenceEntity>
{
    public AdditionalReferenceByIdSpec(int id)
    {
        Query.Where(x => x.Id == AdditionalReferenceId.From(id));
    }
}
