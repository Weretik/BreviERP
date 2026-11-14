using Crm.Application.Features.Quotes.Shared.Persistence;
using Crm.Application.Features.Quotes.Shared.Sending;
using Crm.Application.Abstractions.Quotes;

namespace Crm.Application.Features.Quotes.Commands.DisconnectMailbox;

public sealed class DisconnectMailboxHandler(IQuoteStore store) : ICommandHandler<DisconnectMailboxCommand, Result>
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
