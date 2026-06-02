namespace Reference.Application.Features.GarmentPart.Create.Validators;

public sealed class CreateGarmentPartCommandValidator : AbstractValidator<CreateGarmentPartCommand>
{
    public CreateGarmentPartCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();

        RuleFor(x => x.Request.Id)
            .GreaterThan(0);

        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .MaximumLength(150);
    }
}
