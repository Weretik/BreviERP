using Crm.Application.Quotes.Persistence;

namespace Crm.Application.Quotes.Sending.Disconnect;

public sealed class DisconnectMailboxCommandHandler(IQuoteStore store) : ICommandHandler<DisconnectMailboxCommand, Result>
{
    public async ValueTask<Result> Handle(DisconnectMailboxCommand command, CancellationToken cancellationToken)
    {
        if (!command.Actor.CanSeeAll && command.Actor.UserId != command.UserId)
            return Result.Forbidden(QuoteSendingMessages.ConnectForbidden);
        var mailbox = await store.StaffMailboxes.SingleOrDefaultAsync(x => x.UserId == command.UserId, cancellationToken);
        if (mailbox is null) return Result.NoContent();
        store.StaffMailboxes.Remove(mailbox);
        await store.SaveChangesAsync(cancellationToken);
        return Result.NoContent();
    }
}
