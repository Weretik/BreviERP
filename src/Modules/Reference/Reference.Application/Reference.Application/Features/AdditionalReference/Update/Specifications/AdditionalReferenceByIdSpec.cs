using AdditionalReferenceEntity = Reference.Domain.AdditionalReferences.Entities.AdditionalReference;
using Reference.Domain.AdditionalReferences.ValueObjects;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.GarmentPartOperations.ValueObjects;
using Reference.Domain.Products.ValueObjects;
using Reference.Domain.Suppliers.ValueObjects;

namespace Reference.Application.Features.AdditionalReference.Update.Specifications;

public sealed class AdditionalReferenceByIdSpec : Specification<AdditionalReferenceEntity>
{
    public AdditionalReferenceByIdSpec(int id)
    {
        Query.Where(x => x.Id == AdditionalReferenceId.From(id));
    }
}
