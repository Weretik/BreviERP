using Reference.Application.Features.ProductSection.Shared.DTOs;
using ProductSectionEntity = Reference.Domain.ProductSections.ProductSection;

namespace Reference.Application.Features.ProductSection.Shared.Validators;

public sealed class ProductSectionFieldsValidator : AbstractValidator<IProductSectionFields>
{
    public ProductSectionFieldsValidator()
    {
        RuleFor(x => x.Type)
            .Must(ProductSectionTypes.IsKnown)
            .WithMessage("Тип товару має бути Sewing або Ppe.");

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Назва розділу є обов'язковою.")
            .MaximumLength(ProductSectionEntity.NameMaxLength).WithMessage("Назва розділу: не більше 200 символів.");

        RuleFor(x => x.Comment)
            .MaximumLength(ProductSectionEntity.CommentMaxLength).WithMessage("Коментар: не більше 1000 символів.");

        RuleFor(x => x.SortOrder)
            .InclusiveBetween(0, 100_000).WithMessage("Порядок має бути від 0 до 100000.");
    }
}
