namespace Crm.Application.Features.Quotes.Commands.CompleteMailboxConnect;

/// <summary>
/// The Google callback: stores the address and the encrypted refresh token, then returns the Admin address to
/// redirect to, with <c>gmail=connected</c> or <c>gmail=error</c>. It always succeeds so the browser lands back in
/// the Admin.
/// </summary>
public sealed record CompleteMailboxConnectCommand(string? Code, string? State, string? Error)
    : ICommand<Result<string>>;
