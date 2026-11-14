using PricingParameterEntity = Reference.Domain.PricingParameters.PricingParameter;
using PricingParameterId = Reference.Domain.PricingParameters.PricingParameterId;

namespace Reference.Application.Features.PricingReference.Queries.GetPricingReferencesByIds.Specs;

internal sealed class PricingParametersByIdsOrCodesSpec
    : Specification<PricingParameterEntity, PricingReferenceItem>
{
    public PricingParametersByIdsOrCodesSpec(
        IReadOnlyCollection<int> ids,
        IReadOnlyCollection<string> keys)
    {
        var parameterIds = ids.Select(PricingParameterId.From).ToArray();

        Query.AsNoTracking()
            .Where(x => parameterIds.Contains(x.Id) || keys.Contains(x.Code))
            .Select(x => new PricingReferenceItem(x.Id.Value, x.Value ?? 0m, x.Code, x.Unit));
    }
}
