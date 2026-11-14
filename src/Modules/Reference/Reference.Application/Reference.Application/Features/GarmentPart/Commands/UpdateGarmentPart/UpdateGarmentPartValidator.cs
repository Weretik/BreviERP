namespace Reference.Application.Features.GarmentPart.Commands.UpdateGarmentPart;

public sealed class UpdateGarmentPartValidator : AbstractValidator<UpdateGarmentPartCommand>
{
    public UpdateGarmentPartValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Ідентифікатор частини виробу має бути більшим за 0.");

        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Запит на оновлення частини виробу не може бути порожнім.");

        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Назва частини виробу є обов'язковою.")
                .MaximumLength(150)
                .WithMessage("Назва частини виробу не може перевищувати 150 символів.");
        });
    }
}
