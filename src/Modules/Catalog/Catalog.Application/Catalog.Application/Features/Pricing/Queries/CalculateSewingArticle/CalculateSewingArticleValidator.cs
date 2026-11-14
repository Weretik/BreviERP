using Catalog.Application.Features.Pricing.Shared.Validation;

namespace Catalog.Application.Features.Pricing.Queries.CalculateSewingArticle;

public sealed class CalculateSewingArticleValidator : AbstractValidator<CalculateSewingArticleQuery>
{
    public CalculateSewingArticleValidator()
    {
        RuleFor(x => x.ProductId).ValidProductId();
        RuleFor(x => x.ArticleId).ValidArticleId();
    }
}
