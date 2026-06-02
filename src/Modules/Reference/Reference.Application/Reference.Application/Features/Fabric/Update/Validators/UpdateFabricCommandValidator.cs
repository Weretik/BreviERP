namespace Reference.Application.Features.Fabric.Update.Validators;

public sealed class UpdateFabricCommandValidator : AbstractValidator<UpdateFabricCommand>
{
    public UpdateFabricCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Request).NotNull();

        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Request.Price)
            .InclusiveBetween(0, 10_000);

        RuleFor(x => x.Request.ProviderName)
            .NotEmpty()
            .MaximumLength(200);
    }
}
