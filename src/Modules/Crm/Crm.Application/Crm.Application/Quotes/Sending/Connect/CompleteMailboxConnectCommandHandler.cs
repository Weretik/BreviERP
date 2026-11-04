using Crm.Application.Quotes.Persistence;
using Crm.Application.Quotes.Sending.Gmail;
using Crm.Domain.Quotes.Sending;
using Microsoft.Extensions.Logging;

namespace Crm.Application.Quotes.Sending.Connect;

public sealed class CompleteMailboxConnectCommandHandler(IQuoteStore store, IGmailClient gmail,
    IMailboxProtector protector, QuoteMailOptions options, TimeProvider clock,
    ILogger<CompleteMailboxConnectCommandHandler> logger) : ICommandHandler<CompleteMailboxConnectCommand, Result<string>>
{
    public async ValueTask<Result<string>> Handle(CompleteMailboxConnectCommand command,
        CancellationToken cancellationToken)
    {
        var state = string.IsNullOrEmpty(command.State) ? null : protector.UnprotectState(command.State);
        var returnPath = state is not null && MailboxReturnPath.IsAllowed(state.ReturnPath)
            ? state.ReturnPath
            : MailboxReturnPath.Fallback;
        if (state is null) return Back(returnPath, "error", "expired");
        if (!string.IsNullOrEmpty(command.Error) || string.IsNullOrEmpty(command.Code))
            return Back(returnPath, "error", "denied");

        var grant = await gmail.ExchangeCodeAsync(command.Code, cancellationToken);
        if (!grant.IsSuccess)
        {
            logger.LogWarning("Gmail consent for user {UserId} failed: {Reason}", state.UserId,
                string.Join("; ", grant.Errors));
            return Back(returnPath, "error", "google");
        }

        var now = clock.GetUtcNow();
        var token = protector.ProtectToken(grant.Value.RefreshToken);
        var mailbox = await store.StaffMailboxes.SingleOrDefaultAsync(x => x.UserId == state.UserId, cancellationToken);
        if (mailbox is null)
            store.StaffMailboxes.Add(new StaffMailbox(state.UserId, grant.Value.Email, token, state.ConnectedByName, now));
        else
            mailbox.Connect(grant.Value.Email, token, state.ConnectedByName, now);

        try
        {
            await store.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (store.IsDuplicateMailbox(exception))
        {
            return Back(returnPath, "error", "twice");
        }

        logger.LogInformation("Gmail connected for user {UserId}", state.UserId);
        return Back(returnPath, "connected", null);
    }

    private Result<string> Back(string path, string outcome, string? reason)
    {
        var query = reason is null ? $"gmail={outcome}" : $"gmail={outcome}&reason={reason}";
        var separator = path.Contains('?', StringComparison.Ordinal) ? '&' : '?';
        return Result.Success($"{options.AdminUrl.TrimEnd('/')}{path}{separator}{query}");
    }
}
