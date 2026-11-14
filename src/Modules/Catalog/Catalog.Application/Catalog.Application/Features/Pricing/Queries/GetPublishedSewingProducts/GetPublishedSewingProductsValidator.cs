using Catalog.Application.Features.Pricing.Shared.Validation;

namespace Catalog.Application.Features.Pricing.Queries.GetPublishedSewingProducts;

public sealed class GetPublishedSewingProductsValidator : AbstractValidator<GetPublishedSewingProductsQuery>
{
    public GetPublishedSewingProductsValidator() => RuleFor(x => x.ProductIds).DistinctProductIds();
}
