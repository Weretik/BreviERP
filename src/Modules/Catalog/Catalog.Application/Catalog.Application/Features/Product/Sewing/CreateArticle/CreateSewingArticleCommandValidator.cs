namespace Catalog.Application.Features.Product.Sewing;

public sealed class CreateSewingArticleCommandValidator : AbstractValidator<CreateSewingArticleCommand>
{
    public CreateSewingArticleCommandValidator()
    {
        RuleFor(x => x.ProductId).Must(SewingRules.ValidProductId).OverridePropertyName("productId")
            .WithMessage("Неправильний товар.");
        RuleFor(x => x.Request).NotNull().OverridePropertyName("request").WithMessage("Заповніть дані варіанта.");
        When(x => x.Request is not null, () =>
        {
            SewingRules.AddVariantRules(this, x => x.Request.SaleSku, x => x.Request.FabricId,
                x => x.Request.FabricQuantity, x => x.Request.ExpectedVersion);
            RuleFor(x => x.Request.SortOrder).GreaterThanOrEqualTo(0).OverridePropertyName("sortOrder")
                .WithMessage("Порядок не може бути від'ємним.");
        });
    }
}
