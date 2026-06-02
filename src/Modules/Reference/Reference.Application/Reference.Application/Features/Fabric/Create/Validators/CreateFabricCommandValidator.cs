namespace Reference.Application.Features.Fabric.Create.Validators;

public sealed class CreateFabricCommandValidator : AbstractValidator<CreateFabricCommand>
{
    public CreateFabricCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();

        RuleFor(x => x.Request.Id)
            .GreaterThan(0);

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
