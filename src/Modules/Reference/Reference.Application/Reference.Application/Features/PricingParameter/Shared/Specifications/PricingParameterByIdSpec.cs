using Reference.Domain.PricingParameters;
using PricingParameterEntity = Reference.Domain.PricingParameters.PricingParameter;

namespace Reference.Application.Features.PricingParameter.Shared.Specifications;

public sealed class PricingParameterByIdSpec : Specification<PricingParameterEntity>
{
    public PricingParameterByIdSpec(int id) => Query.Where(x => x.Id == PricingParameterId.From(id));
}
