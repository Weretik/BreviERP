using Catalog.Application.Features.Pricing.Shared.Validation;

namespace Catalog.Application.Features.Pricing.Queries.GetPpeProductPrices;

public sealed class GetPpeProductPricesValidator : AbstractValidator<GetPpeProductPricesQuery>
{
    public GetPpeProductPricesValidator() => RuleFor(x => x.ProductIds).DistinctProductIds();
}
