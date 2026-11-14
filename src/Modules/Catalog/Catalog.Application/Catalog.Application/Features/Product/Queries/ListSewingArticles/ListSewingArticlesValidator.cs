using Catalog.Application.Features.Product.Shared.Sewing;
namespace Catalog.Application.Features.Product.Queries.ListSewingArticles;

public sealed class ListSewingArticlesValidator : AbstractValidator<ListSewingArticlesQuery>
{
    public ListSewingArticlesValidator() =>
        RuleFor(x => x.ProductId).Must(SewingRules.ValidProductId).OverridePropertyName("productId")
            .WithMessage("Неправильний товар.");
}
