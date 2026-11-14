using Catalog.Application.Features.Pricing.Validation;

namespace Catalog.Application.Features.Pricing.Ppe.Products;

public sealed class GetPpeProductPricesQueryValidator : AbstractValidator<GetPpeProductPricesQuery>
{
    public GetPpeProductPricesQueryValidator() => RuleFor(x => x.ProductIds).DistinctProductIds();
}
