using Reference.Domain.PricingParameters.ValueObjects;
using PricingParameterEntity = Reference.Domain.PricingParameters.Entities.PricingParameter;

namespace Reference.Application.Features.PricingParameter.Shared.Specifications;

public sealed class PricingParameterByIdSpec : Specification<PricingParameterEntity>
{
    public PricingParameterByIdSpec(int id) => Query.Where(x => x.Id == PricingParameterId.From(id));
}
