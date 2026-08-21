using AdditionalReferenceEntity = Reference.Domain.AdditionalReferences.Entities.AdditionalReference;
using AdditionalReferenceId = Reference.Domain.AdditionalReferences.ValueObjects.AdditionalReferenceId;

namespace Reference.Application.Features.PricingReference.GetByIds.Specifications;

internal sealed class PricingAdditionalReferencesByIdsOrKeysSpec
    : Specification<AdditionalReferenceEntity, PricingReferenceItem>
{
    public PricingAdditionalReferencesByIdsOrKeysSpec(
        IReadOnlyCollection<int> ids,
        IReadOnlyCollection<string> keys)
    {
        var additionalReferenceIds = ids.Select(AdditionalReferenceId.From).ToArray();

        Query.AsNoTracking()
            .Where(x => additionalReferenceIds.Contains(x.Id) || keys.Contains(x.Key))
            .Select(x => new PricingReferenceItem(x.Id.Value, x.Value, x.Key, x.Unit));
    }
}
