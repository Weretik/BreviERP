using Catalog.Application.Features.Pricing.Validation;

namespace Catalog.Application.Features.Pricing.Sewing.ArticleSummary;

public sealed class GetSewingArticleSummaryQueryValidator : AbstractValidator<GetSewingArticleSummaryQuery>
{
    public GetSewingArticleSummaryQueryValidator() => RuleFor(x => x.ProductId).ValidProductId();
}
