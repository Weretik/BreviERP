using Reference.Application.Features.AdditionalReference.Update;

namespace Reference.Application.Features.AdditionalReference.Update.Validators;

public sealed class UpdateAdditionalReferenceCommandValidator : AbstractValidator<UpdateAdditionalReferenceCommand>
{
    private static readonly string[] AllowedUnits = ["шт.", "грн.", "%"];

    public UpdateAdditionalReferenceCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Ідентифікатор додаткового довідника має бути більшим за 0.");

        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Запит на оновлення додаткового довідника не може бути порожнім.");

        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Назва додаткового довідника є обов'язковою.")
                .MaximumLength(100)
                .WithMessage("Назва додаткового довідника не може перевищувати 100 символів.");

            RuleFor(x => x.Request.Key)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Ключ додаткового довідника є обов'язковим.")
                .MaximumLength(100)
                .WithMessage("Ключ додаткового довідника не може перевищувати 100 символів.");

            RuleFor(x => x.Request.Value)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Значення додаткового довідника не може бути від'ємним.");

            RuleFor(x => x.Request.Unit)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Одиниця виміру є обов'язковою.")
                .MaximumLength(5)
                .WithMessage("Одиниця виміру не може перевищувати 5 символів.")
                .Must(unit => AllowedUnits.Contains(unit))
                .WithMessage("Одиниця виміру має бути однією з: шт., грн., %.");

            RuleFor(x => x.Request.Description)
                .MaximumLength(255)
                .WithMessage("Опис додаткового довідника не може перевищувати 255 символів.")
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Description));
        });
    }
}
