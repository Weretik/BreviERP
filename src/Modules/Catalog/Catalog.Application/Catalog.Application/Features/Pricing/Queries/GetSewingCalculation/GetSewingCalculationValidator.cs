using Catalog.Application.Features.Pricing.Shared.Validation;

namespace Catalog.Application.Features.Pricing.Queries.GetSewingCalculation;

public sealed class GetSewingCalculationValidator : AbstractValidator<GetSewingCalculationQuery>
{
    public GetSewingCalculationValidator()
    {
        RuleFor(x => x.ProductId).ValidProductId();
        RuleFor(x => x.ArticleId).ValidArticleId();
        RuleFor(x => x.CalculationId).NotEmpty().WithMessage("Неправильний розрахунок.");
    }
}
