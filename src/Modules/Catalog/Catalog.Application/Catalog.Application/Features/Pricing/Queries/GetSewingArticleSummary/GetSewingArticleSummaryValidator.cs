using Catalog.Application.Features.Pricing.Shared.Validation;

namespace Catalog.Application.Features.Pricing.Queries.GetSewingArticleSummary;

public sealed class GetSewingArticleSummaryValidator : AbstractValidator<GetSewingArticleSummaryQuery>
{
    public GetSewingArticleSummaryValidator() => RuleFor(x => x.ProductId).ValidProductId();
}
