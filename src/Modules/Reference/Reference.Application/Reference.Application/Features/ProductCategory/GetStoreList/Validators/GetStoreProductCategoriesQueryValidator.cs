namespace Reference.Application.Features.ProductCategory.GetStoreList.Validators;

public sealed class GetStoreProductCategoriesQueryValidator : AbstractValidator<GetStoreProductCategoriesQuery>
{
    private static readonly string[] SupportedLanguages = ["uk", "ru"];

    public GetStoreProductCategoriesQueryValidator()
    {
        RuleFor(x => x.Language)
            .NotEmpty()
            .Must(language => !string.IsNullOrWhiteSpace(language)
                              && SupportedLanguages.Contains(language.Trim().ToLowerInvariant()))
            .WithMessage("Language must be 'uk' or 'ru'.");
    }
}
