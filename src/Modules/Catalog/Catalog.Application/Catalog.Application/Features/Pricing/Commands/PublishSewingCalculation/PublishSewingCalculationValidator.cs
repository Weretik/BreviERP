using Catalog.Application.Features.Pricing.Shared.Validation;

namespace Catalog.Application.Features.Pricing.Commands.PublishSewingCalculation;

public sealed class PublishSewingCalculationValidator : AbstractValidator<PublishSewingCalculationCommand>
{
    public PublishSewingCalculationValidator()
    {
        RuleFor(x => x.ProductId).ValidProductId();
        RuleFor(x => x.ArticleId).ValidArticleId();
        RuleFor(x => x.CalculationId).NotEmpty().WithMessage("Неправильний розрахунок.");
        RuleFor(x => x.Actor).ValidActor();
    }
}
