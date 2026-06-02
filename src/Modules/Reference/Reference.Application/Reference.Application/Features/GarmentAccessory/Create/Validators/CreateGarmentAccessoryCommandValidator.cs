namespace Reference.Application.Features.GarmentAccessory.Create.Validators;

public sealed class CreateGarmentAccessoryCommandValidator : AbstractValidator<CreateGarmentAccessoryCommand>
{
    public CreateGarmentAccessoryCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();

        RuleFor(x => x.Request.Id)
            .GreaterThan(0);

        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Request.Price)
            .InclusiveBetween(0, 10_000);
    }
}
