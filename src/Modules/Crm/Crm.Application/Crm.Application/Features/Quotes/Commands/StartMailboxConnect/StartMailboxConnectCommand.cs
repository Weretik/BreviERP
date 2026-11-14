using Crm.Application.Features.Quotes.Shared.Access;

namespace Crm.Application.Features.Quotes.Commands.StartMailboxConnect;

/// <summary>
/// «Підключити Gmail»: the Google consent address for a staff member. The Director or Admin starts it for anyone,
/// a manager only for themselves; the person then signs in to their Gmail and allows sending.
/// </summary>
/// <param name="ReturnPath">The Admin page to come back to, for example /crm/quote-settings.</param>
public sealed record StartMailboxConnectCommand(QuoteActor Actor, int UserId, string ReturnPath)
    : ICommand<Result<MailboxConnectResponse>>;

public sealed record MailboxConnectResponse(string ConsentUrl);
