namespace Reference.Application.Features.GarmentAccessory.Update.Validators;

public sealed class UpdateGarmentAccessoryCommandValidator : AbstractValidator<UpdateGarmentAccessoryCommand>
{
    public UpdateGarmentAccessoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Request).NotNull();

        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Request.Price)
            .InclusiveBetween(0, 10_000);
    }
}
