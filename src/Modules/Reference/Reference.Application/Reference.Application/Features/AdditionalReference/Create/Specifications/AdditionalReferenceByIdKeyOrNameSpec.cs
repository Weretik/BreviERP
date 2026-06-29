using AdditionalReferenceEntity = Reference.Domain.AdditionalReferences.Entities.AdditionalReference;
using Reference.Domain.AdditionalReferences.ValueObjects;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.GarmentPartOperations.ValueObjects;
using Reference.Domain.Products.ValueObjects;
using Reference.Domain.Suppliers.ValueObjects;

namespace Reference.Application.Features.AdditionalReference.Create.Specifications;

public sealed class AdditionalReferenceByIdKeyOrNameSpec : Specification<AdditionalReferenceEntity>
{
    public AdditionalReferenceByIdKeyOrNameSpec(int id, string key, string name)
    {
        Query.Where(x => x.Id == AdditionalReferenceId.From(id) || x.Key == key || x.Name == name);
    }
}
