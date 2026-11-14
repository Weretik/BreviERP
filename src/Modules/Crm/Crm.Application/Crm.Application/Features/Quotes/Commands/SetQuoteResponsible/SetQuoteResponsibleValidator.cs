namespace Crm.Application.Features.Quotes.Commands.SetQuoteResponsible;

public sealed class SetQuoteResponsibleValidator : AbstractValidator<SetQuoteResponsibleCommand>
{
    public SetQuoteResponsibleValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.UserId).GreaterThan(0).WithMessage("Оберіть менеджера.");
    }
}
