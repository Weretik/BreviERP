namespace Catalog.Application.Features.ProductCategory.Delete.Validators;

public sealed class DeleteProductCategoryCommandValidator : AbstractValidator<DeleteProductCategoryCommand>
{
    public DeleteProductCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Р†РґРµРЅС‚РёС„С–РєР°С‚РѕСЂ РєР°С‚РµРіРѕСЂС–С— С‚РѕРІР°СЂС–РІ РјР°С” Р±СѓС‚Рё Р±С–Р»СЊС€РёРј Р·Р° 0.");
    }
}


