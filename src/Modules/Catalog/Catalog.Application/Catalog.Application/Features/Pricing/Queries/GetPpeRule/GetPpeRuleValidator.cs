namespace Catalog.Application.Features.Pricing.Queries.GetPpeRule;

public sealed class GetPpeRuleValidator : AbstractValidator<GetPpeRuleQuery>
{
    public GetPpeRuleValidator() =>
        RuleFor(x => x.Code).NotEmpty().MaximumLength(200).WithMessage("Вкажіть код правила до 200 символів.");
}
