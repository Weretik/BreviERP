namespace Reference.Application.Features.PricingParameter.Delete.Validators;

public sealed class DeletePricingParameterCommandValidator : AbstractValidator<DeletePricingParameterCommand>
{
    public DeletePricingParameterCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Ідентифікатор параметра має бути більшим за 0.");
    }
}
