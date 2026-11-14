using Reference.Application.Features.PricingParameter.Shared;

namespace Reference.Application.Features.PricingParameter.Commands.UpdatePricingParameter;

public sealed class UpdatePricingParameterValidator : AbstractValidator<UpdatePricingParameterCommand>
{
    public UpdatePricingParameterValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Ідентифікатор параметра має бути більшим за 0.");

        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Запит на зміну параметра не може бути порожнім.");

        When(x => x.Request is not null, () =>
            RuleFor(x => x.Request).SetValidator(new PricingParameterFieldsValidator()));
    }
}
