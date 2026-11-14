namespace Catalog.Application.Features.Product.Sewing;

public sealed class GetSewingCompositionQueryValidator : AbstractValidator<GetSewingCompositionQuery>
{
    public GetSewingCompositionQueryValidator()
    {
        RuleFor(x => x.ProductId).Must(SewingRules.ValidProductId).OverridePropertyName("productId")
            .WithMessage("Неправильний товар.");
        RuleFor(x => x.ArticleId).GreaterThan(0).When(x => x.ArticleId.HasValue).OverridePropertyName("articleId")
            .WithMessage("Неправильний варіант.");
    }
}
