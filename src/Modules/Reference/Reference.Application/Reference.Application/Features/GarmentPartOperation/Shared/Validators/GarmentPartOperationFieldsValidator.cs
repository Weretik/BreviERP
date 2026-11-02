using Reference.Application.Features.GarmentPartOperation.Shared.DTOs;
using GarmentPartOperationEntity = Reference.Domain.GarmentPartOperations.Entities.GarmentPartOperation;

namespace Reference.Application.Features.GarmentPartOperation.Shared.Validators;

public sealed class GarmentPartOperationFieldsValidator : AbstractValidator<IGarmentPartOperationFields>
{
    public GarmentPartOperationFieldsValidator()
    {
        RuleFor(x => x.GarmentPartName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Група деталей є обов'язковою.")
            .MaximumLength(150).WithMessage("Назва групи не може перевищувати 150 символів.");

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Назва операції є обов'язковою.")
            .MaximumLength(GarmentPartOperationEntity.NameMaxLength)
            .WithMessage("Назва операції не може перевищувати 255 символів.");

        RuleFor(x => x.Min)
            .PrecisionScale(20, 8, true).WithMessage("Норма часу має бути числом до 8 знаків після коми.")
            .InclusiveBetween(0, GarmentPartOperationEntity.MaxMinutes)
            .WithMessage("Норма часу має бути від 0 до 10000 хв.");

        RuleFor(x => x.Min)
            .GreaterThan(0).When(x => !x.MinutesPerProduct)
            .WithMessage("Вкажіть норму часу або позначте, що час вводиться для кожного виробу.");

        RuleFor(x => x.Code)
            .MaximumLength(GarmentPartOperationEntity.CodeMaxLength)
            .WithMessage("Код операції не може перевищувати 20 символів.");
        RuleFor(x => x.Element)
            .MaximumLength(GarmentPartOperationEntity.ElementMaxLength)
            .WithMessage("Вузол / елемент не може перевищувати 150 символів.");
        RuleFor(x => x.Variant)
            .MaximumLength(GarmentPartOperationEntity.VariantMaxLength)
            .WithMessage("Варіант виконання не може перевищувати 255 символів.");
        RuleFor(x => x.Notes)
            .MaximumLength(GarmentPartOperationEntity.NotesMaxLength)
            .WithMessage("Примітки не можуть перевищувати 1000 символів.");
    }
}
