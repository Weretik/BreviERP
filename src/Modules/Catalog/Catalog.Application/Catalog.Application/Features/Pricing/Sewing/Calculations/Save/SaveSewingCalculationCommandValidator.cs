using Catalog.Application.Features.Pricing.Validation;

namespace Catalog.Application.Features.Pricing.Sewing.Calculations.Save;

public sealed class SaveSewingCalculationCommandValidator : AbstractValidator<SaveSewingCalculationCommand>
{
    public SaveSewingCalculationCommandValidator()
    {
        RuleFor(x => x.ProductId).ValidProductId();
        RuleFor(x => x.ArticleId).ValidArticleId();
        RuleFor(x => x.RequestId).ValidRequestId();
        RuleFor(x => x.Actor).ValidActor();
    }
}
