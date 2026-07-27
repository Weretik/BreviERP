namespace Catalog.Application.Features.ProductCategory.GetStoreList.Validators;

public sealed class GetStoreProductCategoriesQueryValidator : AbstractValidator<GetStoreProductCategoriesQuery>
{
    private static readonly string[] SupportedLanguages = ["uk", "ru"];

    public GetStoreProductCategoriesQueryValidator()
    {
        RuleFor(x => x.Language)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("РњРѕРІР° РєР°С‚РµРіРѕСЂС–Р№ С‚РѕРІР°СЂС–РІ С” РѕР±РѕРІ'СЏР·РєРѕРІРѕСЋ.")
            .Must(language => !string.IsNullOrWhiteSpace(language)
                              && SupportedLanguages.Contains(language.Trim().ToLowerInvariant()))
            .WithMessage("РњРѕРІР° РєР°С‚РµРіРѕСЂС–Р№ С‚РѕРІР°СЂС–РІ РјР°С” Р±СѓС‚Рё 'uk' Р°Р±Рѕ 'ru'.");
    }
}


