namespace Reference.Application.Features.GarmentPart.Create.Validators;

public sealed class CreateGarmentPartCommandValidator : AbstractValidator<CreateGarmentPartCommand>
{
    public CreateGarmentPartCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Запит на створення частини виробу не може бути порожнім.");

        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.Id)
                .GreaterThan(0)
                .WithMessage("Ідентифікатор частини виробу має бути більшим за 0.");

            RuleFor(x => x.Request.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Назва частини виробу є обов'язковою.")
                .MaximumLength(150)
                .WithMessage("Назва частини виробу не може перевищувати 150 символів.");
        });
    }
}
