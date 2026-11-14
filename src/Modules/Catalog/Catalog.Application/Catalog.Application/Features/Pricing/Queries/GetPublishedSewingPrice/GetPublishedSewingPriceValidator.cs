using Catalog.Application.Features.Pricing.Shared.Validation;

namespace Catalog.Application.Features.Pricing.Queries.GetPublishedSewingPrice;

public sealed class GetPublishedSewingPriceValidator : AbstractValidator<GetPublishedSewingPriceQuery>
{
    public GetPublishedSewingPriceValidator()
    {
        RuleFor(x => x.ProductId).ValidProductId();
        RuleFor(x => x.ArticleId).ValidArticleId();
    }
}
