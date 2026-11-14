using PricingParameterEntity = Reference.Domain.PricingParameters.PricingParameter;
using Reference.Application.Features.PricingParameter.Shared;

namespace Reference.Application.Features.PricingParameter.Commands.CreatePricingParameter;

public sealed class CreatePricingParameterValidator : AbstractValidator<CreatePricingParameterCommand>
{
    public CreatePricingParameterValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Запит на створення параметра не може бути порожнім.");

        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.Code)
                .Must(code => PricingParameterEntity.IsValidCode(PricingParameterEntity.NormalizeCode(code)))
                .WithMessage("Код: великі латинські літери, цифри та _, до 64 символів (наприклад, SALARY_SEAMSTRESS).");

            RuleFor(x => x.Request).SetValidator(new PricingParameterFieldsValidator());
        });
    }
}
