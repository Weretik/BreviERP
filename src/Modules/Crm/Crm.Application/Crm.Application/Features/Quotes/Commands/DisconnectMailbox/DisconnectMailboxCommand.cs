using Crm.Application.Features.Quotes.Shared.Access;

namespace Crm.Application.Features.Quotes.Commands.DisconnectMailbox;

/// <summary>«Відключити»: forgets the Gmail of a staff member (the Director or Admin for anyone, a manager for themselves).</summary>
public sealed record DisconnectMailboxCommand(QuoteActor Actor, int UserId) : ICommand<Result>;

public sealed class DisconnectMailboxValidator : AbstractValidator<DisconnectMailboxCommand>
{
    public DisconnectMailboxValidator() => RuleFor(x => x.UserId).GreaterThan(0);
}
