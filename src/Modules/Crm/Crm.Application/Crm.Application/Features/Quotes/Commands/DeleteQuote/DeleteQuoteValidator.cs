namespace Crm.Application.Features.Quotes.Commands.DeleteQuote;

public sealed class DeleteQuoteValidator : AbstractValidator<DeleteQuoteCommand>
{
    public DeleteQuoteValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Version).GreaterThan(0);
    }
}
