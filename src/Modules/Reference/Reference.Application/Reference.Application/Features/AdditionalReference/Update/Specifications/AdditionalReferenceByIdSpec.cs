using AdditionalReferenceEntity = Reference.Domain.Entities.AdditionalReference;

namespace Reference.Application.Features.AdditionalReference.Update.Specifications;

public sealed class AdditionalReferenceByIdSpec : Specification<AdditionalReferenceEntity>
{
    public AdditionalReferenceByIdSpec(int id)
    {
        Query.Where(x => x.Id.Value == id);
    }
}
