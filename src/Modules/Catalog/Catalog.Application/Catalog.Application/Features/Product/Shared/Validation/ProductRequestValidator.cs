using Catalog.Application.Features.Product.Create.DTOs;
using Catalog.Application.Features.Product.Shared.Validation.Common;
using Catalog.Application.Features.Product.Shared.Validation.Content;
using Catalog.Application.Features.Product.Shared.Validation.Ppe;
using Catalog.Application.Features.Product.Shared.Validation.Resources;
using Catalog.Application.Features.Product.Shared.Validation.Sewing;
using Catalog.Domain.Products.Enums;

namespace Catalog.Application.Features.Product.Shared.Validation;

internal sealed class ProductRequestValidator : AbstractValidator<CreateProductCommandRequest>
{
    public ProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(_ => ProductValidationMessages.Required("Українська назва", "Украинское название"))
            .MaximumLength(200)
            .WithMessage(_ => ProductValidationMessages.MaximumLength("Українська назва", "Украинское название", 200));

        RuleFor(x => x.RuName)
            .NotEmpty()
            .WithMessage(_ => ProductValidationMessages.Required("Російська назва", "Русское название"))
            .MaximumLength(200)
            .WithMessage(_ => ProductValidationMessages.MaximumLength("Російська назва", "Русское название", 200));

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage(_ => ProductValidationMessages.Invalid("Тип товару", "Тип товара"));

        RuleFor(x => x.DescriptionUk)
            .NotEmpty()
            .WithMessage(_ => ProductValidationMessages.Required("Український опис", "Украинское описание"))
            .MaximumLength(20_000)
            .WithMessage(_ => ProductValidationMessages.MaximumLength("Український опис", "Украинское описание", 20_000));

        RuleFor(x => x.DescriptionRu)
            .NotEmpty()
            .WithMessage(_ => ProductValidationMessages.Required("Російський опис", "Русское описание"))
            .MaximumLength(20_000)
            .WithMessage(_ => ProductValidationMessages.MaximumLength("Російський опис", "Русское описание", 20_000));

        RuleFor(x => x.CategoryIds)
            .NotNull()
            .WithMessage(_ => ProductValidationMessages.Required("Категорії", "Категории"));

        When(x => x.CategoryIds is not null, () =>
        {
            RuleForEach(x => x.CategoryIds)
                .GreaterThan(0)
                .WithMessage(_ => ProductValidationMessages.Positive("ID категорії", "ID категории"));
            RuleFor(x => x.CategoryIds)
                .Must(ids => ids.Distinct().Count() == ids.Count)
                .WithMessage(_ => ProductValidationMessages.Duplicate("Категорії", "Категории"));
        });

        RuleFor(x => x.Photos)
            .NotNull()
            .WithMessage(_ => ProductValidationMessages.Required("Фото", "Фото"));

        When(x => x.Photos is not null, () =>
        {
            RuleForEach(x => x.Photos).SetValidator(new ProductPhotoRequestValidator());
            RuleFor(x => x.Photos)
                .Must(photos => photos.Select(x => x.MediaFileId).Distinct().Count() == photos.Count)
                .WithMessage(_ => ProductValidationMessages.Duplicate("Фото", "Фото"));
            RuleFor(x => x.Photos)
                .Must(photos => photos.Count == 0 || photos.Any(x => x.IsMain))
                .WithMessage(_ => ProductValidationMessages.Localize(
                    "Серед фото має бути головне.",
                    "Среди фото должно быть главное."));
        });

        RuleFor(x => x.InformationBlocks)
            .NotNull()
            .WithMessage(_ => ProductValidationMessages.Required("Інформаційні блоки", "Информационные блоки"));
        When(x => x.InformationBlocks is not null, () =>
            RuleForEach(x => x.InformationBlocks).SetValidator(new InformationBlockRequestValidator()));

        RuleFor(x => x.CharacteristicTables)
            .NotNull()
            .WithMessage(_ => ProductValidationMessages.Required("Таблиці характеристик", "Таблицы характеристик"));
        When(x => x.CharacteristicTables is not null, () =>
            RuleForEach(x => x.CharacteristicTables).SetValidator(new CharacteristicTableRequestValidator()));

        When(x => x.Type == ProductType.Sewing, () =>
        {
            RuleFor(x => x.Sewing)
                .NotNull()
                .WithMessage(_ => ProductValidationMessages.Required(
                    "Дані швейного товару",
                    "Данные швейного товара"));
            RuleFor(x => x.Ppe)
                .Null()
                .WithMessage(_ => ProductValidationMessages.Localize(
                    "Дані засобів індивідуального захисту не дозволені для швейного товару.",
                    "Данные средств индивидуальной защиты не разрешены для швейного товара."));
            When(x => x.Sewing is not null, () =>
                RuleFor(x => x.Sewing!).SetValidator(new SewingProductRequestValidator()));
        });

        When(x => x.Type == ProductType.Ppe, () =>
        {
            RuleFor(x => x.Ppe)
                .NotNull()
                .WithMessage(_ => ProductValidationMessages.Required(
                    "Дані засобів індивідуального захисту",
                    "Данные средств индивидуальной защиты"));
            RuleFor(x => x.Sewing)
                .Null()
                .WithMessage(_ => ProductValidationMessages.Localize(
                    "Дані швейного товару не дозволені для засобів індивідуального захисту.",
                    "Данные швейного товара не разрешены для средств индивидуальной защиты."));
            When(x => x.Ppe is not null, () =>
                RuleFor(x => x.Ppe!).SetValidator(new PpeProductRequestValidator()));
        });
    }
}
