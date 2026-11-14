
namespace Crm.Api.Endpoints.Mailboxes.ConnectCrmMailbox;

/// <param name="ReturnPath">The Admin page to come back to after Google, for example /crm/quote-settings.</param>
public sealed record MailboxConnectRequest(string ReturnPath);
