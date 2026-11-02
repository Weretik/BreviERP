using Reference.Application.Features.PpeMarkupRule.Shared.Validators;
using PpeMarkupRuleEntity = Reference.Domain.PpeMarkupRules.Entities.PpeMarkupRule;

namespace Reference.Application.Features.PpeMarkupRule.Create.Validators;

public sealed class CreatePpeMarkupRuleCommandValidator : AbstractValidator<CreatePpeMarkupRuleCommand>
{
    public CreatePpeMarkupRuleCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Запит на створення правила не може бути порожнім.");

        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Назва правила є обов'язковою.")
                .MaximumLength(PpeMarkupRuleEntity.NameMaxLength).WithMessage("Назва правила: не більше 200 символів.");

            RuleFor(x => x.Request).SetValidator(new PpeMarkupRuleFieldsValidator());
        });
    }
}
