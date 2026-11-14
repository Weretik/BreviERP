using Catalog.Application.Features.Pricing.Validation;

namespace Catalog.Application.Features.Pricing.Sewing.Publications.Publish;

public sealed class PublishSewingCalculationCommandValidator : AbstractValidator<PublishSewingCalculationCommand>
{
    public PublishSewingCalculationCommandValidator()
    {
        RuleFor(x => x.ProductId).ValidProductId();
        RuleFor(x => x.ArticleId).ValidArticleId();
        RuleFor(x => x.CalculationId).NotEmpty().WithMessage("Неправильний розрахунок.");
        RuleFor(x => x.Actor).ValidActor();
    }
}
