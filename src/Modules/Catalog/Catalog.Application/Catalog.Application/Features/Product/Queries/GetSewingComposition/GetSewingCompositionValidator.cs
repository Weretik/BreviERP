using Catalog.Application.Features.Product.Shared.Sewing;
namespace Catalog.Application.Features.Product.Queries.GetSewingComposition;

public sealed class GetSewingCompositionValidator : AbstractValidator<GetSewingCompositionQuery>
{
    public GetSewingCompositionValidator()
    {
        RuleFor(x => x.ProductId).Must(SewingRules.ValidProductId).OverridePropertyName("productId")
            .WithMessage("Неправильний товар.");
        RuleFor(x => x.ArticleId).GreaterThan(0).When(x => x.ArticleId.HasValue).OverridePropertyName("articleId")
            .WithMessage("Неправильний варіант.");
    }
}
