using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.Mapping;
using Crm.Application.Quotes.Pdf.Composing;
using Crm.Application.Quotes.Persistence;
using Crm.Application.Quotes.Sending.Gmail;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Sending;

namespace Crm.Application.Quotes.Sending.Send;

public sealed class SendQuoteCommandHandler(IQuoteStore store, QuotePdfComposer composer, IGmailClient gmail,
    IMailboxProtector protector, TimeProvider clock, ILogger<SendQuoteCommandHandler> logger)
    : ICommandHandler<SendQuoteCommand, Result<QuoteResponse>>
{
    public async ValueTask<Result<QuoteResponse>> Handle(SendQuoteCommand command, CancellationToken cancellationToken)
    {
        var quote = await store.LoadAsync(command.Id, cancellationToken);
        if (quote is null || !command.Actor.CanAccess(quote)) return Result.NotFound(QuoteMessages.NotFound);
        if (quote.Version != command.Input.Version) return Result.Conflict(QuoteMessages.Stale);
        if (!gmail.IsConfigured) return Result.Conflict(QuoteSendingMessages.NotConfigured);

        var mailbox = await store.StaffMailboxes.AsNoTracking()
            .SingleOrDefaultAsync(x => x.UserId == command.Actor.UserId, cancellationToken);
        if (mailbox is null) return Result.Conflict(QuoteSendingMessages.NoMailbox);
        var refreshToken = protector.UnprotectToken(mailbox.ProtectedRefreshToken);
        if (refreshToken is null) return Result.Conflict(QuoteSendingMessages.MailboxExpired);

        var context = await composer.LoadContextAsync(cancellationToken);
        var member = await composer.MemberAsync(command.Actor.UserId, "", cancellationToken);
        if (string.IsNullOrEmpty(member.Name)) return Result.Forbidden(QuoteMessages.ActorUnknown);
        var pdf = await composer.ComposeAsync(quote, context, cancellationToken);

        var recipients = QuoteRecipients.Parse(command.Input.To);
        var subject = command.Input.Subject.Trim();
        var letter = new GmailLetter(mailbox.Email, context.ContactOf(member).Name, recipients, subject,
            command.Input.Body.Trim(), new GmailAttachment(pdf.FileName, "application/pdf", pdf.Content));
        var sent = await gmail.SendAsync(refreshToken, letter, cancellationToken);
        if (!sent.IsSuccess) return Result.Conflict(sent.Errors.ToArray());

        var now = clock.GetUtcNow();
        store.QuoteSendings.Add(new QuoteSending(quote.Id, quote.Version, new QuotePerson(member.UserId, member.Name),
            mailbox.Email, string.Join(", ", recipients), subject, sent.Value, now));
        quote.MarkSent(now);
        try
        {
            await store.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // The letter is already out; only the mark failed because someone saved the КП meanwhile.
            logger.LogWarning("КП {QuoteId} was sent as Gmail {MessageId} but changed meanwhile", quote.Id, sent.Value);
            return Result.Conflict(QuoteSendingMessages.SentButChanged);
        }

        logger.LogInformation("КП {QuoteId} sent from user {UserId}", quote.Id, member.UserId);
        return Result.Success(quote.ToResponse());
    }
}
