using Catalog.Application.Features.Pricing.Validation;

namespace Catalog.Application.Features.Pricing.Sewing.Calculations.Get;

public sealed class GetSewingCalculationQueryValidator : AbstractValidator<GetSewingCalculationQuery>
{
    public GetSewingCalculationQueryValidator()
    {
        RuleFor(x => x.ProductId).ValidProductId();
        RuleFor(x => x.ArticleId).ValidArticleId();
        RuleFor(x => x.CalculationId).NotEmpty().WithMessage("Неправильний розрахунок.");
    }
}
