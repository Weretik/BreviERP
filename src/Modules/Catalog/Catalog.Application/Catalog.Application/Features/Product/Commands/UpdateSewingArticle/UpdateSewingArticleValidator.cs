using Catalog.Application.Features.Product.Shared.Sewing;
namespace Catalog.Application.Features.Product.Commands.UpdateSewingArticle;

public sealed class UpdateSewingArticleValidator : AbstractValidator<UpdateSewingArticleCommand>
{
    public UpdateSewingArticleValidator()
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
