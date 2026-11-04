namespace Crm.Application.Quotes.SetResponsible;

public sealed class SetQuoteResponsibleCommandValidator : AbstractValidator<SetQuoteResponsibleCommand>
{
    public SetQuoteResponsibleCommandValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.UserId).GreaterThan(0).WithMessage("Оберіть менеджера.");
    }
}
