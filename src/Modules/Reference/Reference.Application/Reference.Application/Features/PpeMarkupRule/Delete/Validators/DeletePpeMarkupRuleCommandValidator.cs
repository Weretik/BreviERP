namespace Reference.Application.Features.PpeMarkupRule.Delete.Validators;

public sealed class DeletePpeMarkupRuleCommandValidator : AbstractValidator<DeletePpeMarkupRuleCommand>
{
    public DeletePpeMarkupRuleCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Ідентифікатор правила має бути більшим за 0.");
    }
}
