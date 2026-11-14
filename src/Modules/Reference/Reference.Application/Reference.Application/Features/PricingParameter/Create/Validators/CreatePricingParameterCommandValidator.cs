using Reference.Application.Features.PricingParameter.Shared.Validators;
using PricingParameterEntity = Reference.Domain.PricingParameters.PricingParameter;

namespace Reference.Application.Features.PricingParameter.Create.Validators;

public sealed class CreatePricingParameterCommandValidator : AbstractValidator<CreatePricingParameterCommand>
{
    public CreatePricingParameterCommandValidator()
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
