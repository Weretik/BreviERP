namespace Reference.Application.Features.PpeMarkupRule.Commands.DeletePpeMarkupRule;

public sealed class DeletePpeMarkupRuleValidator : AbstractValidator<DeletePpeMarkupRuleCommand>
{
    public DeletePpeMarkupRuleValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Ідентифікатор правила має бути більшим за 0.");
    }
}
