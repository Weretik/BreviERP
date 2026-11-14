using GarmentAccessoryEntity = Reference.Domain.GarmentAccessories.GarmentAccessory;

namespace Reference.Application.Features.GarmentAccessory.Shared;

public sealed class GarmentAccessoryFieldsValidator : AbstractValidator<IGarmentAccessoryFields>
{
    public GarmentAccessoryFieldsValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Назва фурнітури є обов'язковою.")
            .MaximumLength(GarmentAccessoryEntity.NameMaxLength).WithMessage("Назва фурнітури не може перевищувати 500 символів.");

        RuleFor(x => x.Price)
            .InclusiveBetween(0, 10_000).WithMessage("Ціна фурнітури має бути від 0 до 10000.");

        RuleFor(x => x.SupplierName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Назва постачальника фурнітури є обов'язковою.")
            .MaximumLength(200).WithMessage("Назва постачальника фурнітури не може перевищувати 200 символів.");

        RuleFor(x => x.Code)
            .MaximumLength(GarmentAccessoryEntity.CodeMaxLength).WithMessage("Код фурнітури не може перевищувати 20 символів.");
        RuleFor(x => x.Unit)
            .Must(unit => unit is null || GarmentAccessoryEntity.NormalizeUnit(unit) is not null)
            .WithMessage($"Одиниця виміру: {string.Join(", ", GarmentAccessoryEntity.Units)}.");
        RuleFor(x => x.Comment)
            .MaximumLength(GarmentAccessoryEntity.CommentMaxLength).WithMessage("Коментар не може перевищувати 1000 символів.");
    }
}
