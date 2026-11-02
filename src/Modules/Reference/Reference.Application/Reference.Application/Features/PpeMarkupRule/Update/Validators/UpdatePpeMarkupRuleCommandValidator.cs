using Reference.Application.Features.PpeMarkupRule.Shared.Validators;

namespace Reference.Application.Features.PpeMarkupRule.Update.Validators;

public sealed class UpdatePpeMarkupRuleCommandValidator : AbstractValidator<UpdatePpeMarkupRuleCommand>
{
    public UpdatePpeMarkupRuleCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Ідентифікатор правила має бути більшим за 0.");

        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Запит на зміну правила не може бути порожнім.");

        When(x => x.Request is not null, () =>
            RuleFor(x => x.Request).SetValidator(new PpeMarkupRuleFieldsValidator()));
    }
}
