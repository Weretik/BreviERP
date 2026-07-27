namespace Catalog.Application.Features.ProductCategory.Update.Validators;

public sealed class UpdateProductCategoryCommandValidator : AbstractValidator<UpdateProductCategoryCommand>
{
    public UpdateProductCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Р†РґРµРЅС‚РёС„С–РєР°С‚РѕСЂ РєР°С‚РµРіРѕСЂС–С— С‚РѕРІР°СЂС–РІ РјР°С” Р±СѓС‚Рё Р±С–Р»СЊС€РёРј Р·Р° 0.");

        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Р—Р°РїРёС‚ РЅР° РѕРЅРѕРІР»РµРЅРЅСЏ РєР°С‚РµРіРѕСЂС–С— С‚РѕРІР°СЂС–РІ РЅРµ РјРѕР¶Рµ Р±СѓС‚Рё РїРѕСЂРѕР¶РЅС–Рј.");

        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("РќР°Р·РІР° РєР°С‚РµРіРѕСЂС–С— С‚РѕРІР°СЂС–РІ С” РѕР±РѕРІ'СЏР·РєРѕРІРѕСЋ.")
                .MaximumLength(200)
                .WithMessage("РќР°Р·РІР° РєР°С‚РµРіРѕСЂС–С— С‚РѕРІР°СЂС–РІ РЅРµ РјРѕР¶Рµ РїРµСЂРµРІРёС‰СѓРІР°С‚Рё 200 СЃРёРјРІРѕР»С–РІ.");

            RuleFor(x => x.Request.RuName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("РќР°Р·РІР° РєР°С‚РµРіРѕСЂС–С— С‚РѕРІР°СЂС–РІ СЂРѕСЃС–Р№СЃСЊРєРѕСЋ С” РѕР±РѕРІ'СЏР·РєРѕРІРѕСЋ.")
                .MaximumLength(200)
                .WithMessage("РќР°Р·РІР° РєР°С‚РµРіРѕСЂС–С— С‚РѕРІР°СЂС–РІ СЂРѕСЃС–Р№СЃСЊРєРѕСЋ РЅРµ РјРѕР¶Рµ РїРµСЂРµРІРёС‰СѓРІР°С‚Рё 200 СЃРёРјРІРѕР»С–РІ.");

            RuleFor(x => x.Request.Slug)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Slug РєР°С‚РµРіРѕСЂС–С— С” РѕР±РѕРІ'СЏР·РєРѕРІРёРј.")
                .MaximumLength(160)
                .WithMessage("Slug РєР°С‚РµРіРѕСЂС–С— РЅРµ РјРѕР¶Рµ РїРµСЂРµРІРёС‰СѓРІР°С‚Рё 160 СЃРёРјРІРѕР»С–РІ.")
                .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
                .WithMessage("Slug РєР°С‚РµРіРѕСЂС–С— РјРѕР¶Рµ РјС–СЃС‚РёС‚Рё Р»РёС€Рµ РјР°Р»С– Р»Р°С‚РёРЅСЃСЊРєС– Р»С–С‚РµСЂРё, С†РёС„СЂРё С‚Р° РґРµС„С–СЃРё.");

            RuleFor(x => x.Request.ParentId)
                .GreaterThan(0)
                .WithMessage("Р†РґРµРЅС‚РёС„С–РєР°С‚РѕСЂ Р±Р°С‚СЊРєС–РІСЃСЊРєРѕС— РєР°С‚РµРіРѕСЂС–С— РјР°С” Р±СѓС‚Рё Р±С–Р»СЊС€РёРј Р·Р° 0.")
                .When(x => x.Request.ParentId.HasValue);

            RuleFor(x => x.Request.SortOrder)
                .GreaterThanOrEqualTo(0)
                .WithMessage("РџРѕСЂСЏРґРѕРє СЃРѕСЂС‚СѓРІР°РЅРЅСЏ РЅРµ РјРѕР¶Рµ Р±СѓС‚Рё РІС–Рґ'С”РјРЅРёРј.");

            RuleFor(x => x.Request.Description)
                .MaximumLength(1000)
                .WithMessage("РћРїРёСЃ РєР°С‚РµРіРѕСЂС–С— С‚РѕРІР°СЂС–РІ РЅРµ РјРѕР¶Рµ РїРµСЂРµРІРёС‰СѓРІР°С‚Рё 1000 СЃРёРјРІРѕР»С–РІ.")
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Description));
        });
    }
}


