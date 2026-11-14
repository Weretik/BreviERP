using Catalog.Application.Features.Pricing.Shared.Validation;

namespace Catalog.Application.Features.Pricing.Commands.SaveSewingCalculation;

public sealed class SaveSewingCalculationValidator : AbstractValidator<SaveSewingCalculationCommand>
{
    public SaveSewingCalculationValidator()
    {
        RuleFor(x => x.ProductId).ValidProductId();
        RuleFor(x => x.ArticleId).ValidArticleId();
        RuleFor(x => x.RequestId).ValidRequestId();
        RuleFor(x => x.Actor).ValidActor();
    }
}
