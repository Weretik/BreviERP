namespace Catalog.Application.Features.Product.Sewing;

public sealed class ListSewingArticlesQueryValidator : AbstractValidator<ListSewingArticlesQuery>
{
    public ListSewingArticlesQueryValidator() =>
        RuleFor(x => x.ProductId).Must(SewingRules.ValidProductId).OverridePropertyName("productId")
            .WithMessage("Неправильний товар.");
}
