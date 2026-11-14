using Catalog.Application.Features.Pricing.Validation;

namespace Catalog.Application.Features.Pricing.Sewing.Publications.GetCurrent;

public sealed class GetPublishedSewingPriceQueryValidator : AbstractValidator<GetPublishedSewingPriceQuery>
{
    public GetPublishedSewingPriceQueryValidator()
    {
        RuleFor(x => x.ProductId).ValidProductId();
        RuleFor(x => x.ArticleId).ValidArticleId();
    }
}
