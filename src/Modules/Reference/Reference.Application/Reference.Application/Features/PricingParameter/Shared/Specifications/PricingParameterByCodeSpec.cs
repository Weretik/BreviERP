using PricingParameterEntity = Reference.Domain.PricingParameters.PricingParameter;

namespace Reference.Application.Features.PricingParameter.Shared.Specifications;

public sealed class PricingParameterByCodeSpec : Specification<PricingParameterEntity>
{
    public PricingParameterByCodeSpec(string code) => Query.Where(x => x.Code == code);
}
