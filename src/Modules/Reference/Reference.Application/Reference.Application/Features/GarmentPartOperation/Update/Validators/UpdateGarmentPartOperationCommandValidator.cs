namespace Reference.Application.Features.GarmentPartOperation.Update.Validators;

public sealed class UpdateGarmentPartOperationCommandValidator : AbstractValidator<UpdateGarmentPartOperationCommand>
{
    public UpdateGarmentPartOperationCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Ідентифікатор операції частини виробу має бути більшим за 0.");

        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Запит на оновлення операції частини виробу не може бути порожнім.");

        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.GarmentPartName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Назва частини виробу є обов'язковою.")
                .MaximumLength(150)
                .WithMessage("Назва частини виробу не може перевищувати 150 символів.");

            RuleFor(x => x.Request.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Назва операції є обов'язковою.")
                .MaximumLength(255)
                .WithMessage("Назва операції не може перевищувати 255 символів.");

            RuleFor(x => x.Request.Min)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Тривалість операції не може бути від'ємною.");
        });
    }
}
