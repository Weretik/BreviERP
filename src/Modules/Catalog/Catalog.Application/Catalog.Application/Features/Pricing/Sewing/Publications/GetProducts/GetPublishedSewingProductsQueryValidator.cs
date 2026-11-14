using Catalog.Application.Features.Pricing.Validation;

namespace Catalog.Application.Features.Pricing.Sewing.Publications.GetProducts;

public sealed class GetPublishedSewingProductsQueryValidator : AbstractValidator<GetPublishedSewingProductsQuery>
{
    public GetPublishedSewingProductsQueryValidator() => RuleFor(x => x.ProductIds).DistinctProductIds();
}
