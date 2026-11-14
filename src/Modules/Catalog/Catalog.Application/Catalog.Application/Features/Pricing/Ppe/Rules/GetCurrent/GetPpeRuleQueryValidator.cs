namespace Catalog.Application.Features.Pricing.Ppe.Rules.GetCurrent;

public sealed class GetPpeRuleQueryValidator : AbstractValidator<GetPpeRuleQuery>
{
    public GetPpeRuleQueryValidator() =>
        RuleFor(x => x.Code).NotEmpty().MaximumLength(200).WithMessage("Вкажіть код правила до 200 символів.");
}
