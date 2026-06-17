namespace Reference.Application.Features.GarmentPart.Update.Validators;

public sealed class UpdateGarmentPartCommandValidator : AbstractValidator<UpdateGarmentPartCommand>
{
    public UpdateGarmentPartCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Request).NotNull();

        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .MaximumLength(150);
    }
}
