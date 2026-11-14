namespace Catalog.Application.Features.Product.Sewing;

public sealed class UpdateSewingArticleCommandValidator : AbstractValidator<UpdateSewingArticleCommand>
{
    public UpdateSewingArticleCommandValidator()
    {
        RuleFor(x => x.ProductId).Must(SewingRules.ValidProductId).OverridePropertyName("productId")
            .WithMessage("Неправильний товар.");
        RuleFor(x => x.ArticleId).GreaterThan(0).OverridePropertyName("articleId")
            .WithMessage("Неправильний варіант.");
        RuleFor(x => x.Request).NotNull().OverridePropertyName("request").WithMessage("Заповніть дані варіанта.");
        When(x => x.Request is not null, () =>
            SewingRules.AddVariantRules(this, x => x.Request.SaleSku, x => x.Request.FabricId,
                x => x.Request.FabricQuantity, x => x.Request.ExpectedVersion));
    }
}
