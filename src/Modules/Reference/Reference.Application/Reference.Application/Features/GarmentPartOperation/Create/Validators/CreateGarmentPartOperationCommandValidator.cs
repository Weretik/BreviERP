namespace Reference.Application.Features.GarmentPartOperation.Create.Validators;

public sealed class CreateGarmentPartOperationCommandValidator : AbstractValidator<CreateGarmentPartOperationCommand>
{
    public CreateGarmentPartOperationCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();

        RuleFor(x => x.Request.Id)
            .GreaterThan(0);

        RuleFor(x => x.Request.GarmentPartName)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.Request.Min)
            .GreaterThanOrEqualTo(0);
    }
}
