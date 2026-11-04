using Crm.Application.Quotes.Access;

namespace Crm.Application.Quotes.Sending.Disconnect;

/// <summary>«Відключити»: forgets the Gmail of a staff member (the Director or Admin for anyone, a manager for themselves).</summary>
public sealed record DisconnectMailboxCommand(QuoteActor Actor, int UserId) : ICommand<Result>;

public sealed class DisconnectMailboxCommandValidator : AbstractValidator<DisconnectMailboxCommand>
{
    public DisconnectMailboxCommandValidator() => RuleFor(x => x.UserId).GreaterThan(0);
}
