using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Sending.Gmail;
using Crm.Application.Quotes.Staff;

namespace Crm.Application.Quotes.Sending.Connect;

public sealed class StartMailboxConnectCommandHandler(IQuoteStaffReader staff, IGmailClient gmail,
    IMailboxProtector protector) : ICommandHandler<StartMailboxConnectCommand, Result<MailboxConnectResponse>>
{
    /// <summary>Google must call back within this time.</summary>
    private static readonly TimeSpan StateLifetime = TimeSpan.FromMinutes(15);

    public async ValueTask<Result<MailboxConnectResponse>> Handle(StartMailboxConnectCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.Actor.CanSeeAll && command.Actor.UserId != command.UserId)
            return Result.Forbidden(QuoteSendingMessages.ConnectForbidden);
        if (!gmail.IsConfigured) return Result.Conflict(QuoteSendingMessages.NotConfigured);

        var actor = await staff.FindAsync(command.Actor.UserId, cancellationToken);
        if (actor is null) return Result.Forbidden(QuoteMessages.ActorUnknown);
        var member = await staff.FindAsync(command.UserId, cancellationToken);
        if (member is null) return Result.NotFound(QuoteMessages.StaffNotFound);

        var state = protector.ProtectState(
            new MailboxConnectState(member.UserId, actor.Name, command.ReturnPath), StateLifetime);
        return Result.Success(new MailboxConnectResponse(gmail.ConsentUrl(state, member.Email).AbsoluteUri));
    }
}
