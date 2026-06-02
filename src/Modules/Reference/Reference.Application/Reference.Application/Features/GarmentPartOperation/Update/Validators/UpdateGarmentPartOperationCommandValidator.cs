namespace Reference.Application.Features.GarmentPartOperation.Update.Validators;

public sealed class UpdateGarmentPartOperationCommandValidator : AbstractValidator<UpdateGarmentPartOperationCommand>
{
    public UpdateGarmentPartOperationCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Request).NotNull();

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
