namespace Catalog.Application.Features.Product.Sewing;

public sealed class DeleteSewingArticleCommandValidator : AbstractValidator<DeleteSewingArticleCommand>
{
    public DeleteSewingArticleCommandValidator()
    {
        RuleFor(x => x.ProductId).Must(SewingRules.ValidProductId).OverridePropertyName("productId")
            .WithMessage("Неправильний товар.");
        RuleFor(x => x.ArticleId).GreaterThan(0).OverridePropertyName("articleId")
            .WithMessage("Неправильний варіант.");
        RuleFor(x => x.ExpectedVersion).GreaterThanOrEqualTo(0).OverridePropertyName("expectedVersion")
            .WithMessage("Версія не може бути від'ємною.");
    }
}
