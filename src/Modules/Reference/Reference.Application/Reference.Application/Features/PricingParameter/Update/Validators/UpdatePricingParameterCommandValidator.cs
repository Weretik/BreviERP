using Reference.Application.Features.PricingParameter.Shared.Validators;

namespace Reference.Application.Features.PricingParameter.Update.Validators;

public sealed class UpdatePricingParameterCommandValidator : AbstractValidator<UpdatePricingParameterCommand>
{
    public UpdatePricingParameterCommandValidator()
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
