namespace Reference.Application.Features.PricingParameter.Commands.DeletePricingParameter;

public sealed class DeletePricingParameterValidator : AbstractValidator<DeletePricingParameterCommand>
{
    public DeletePricingParameterValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Ідентифікатор параметра має бути більшим за 0.");
    }
}
