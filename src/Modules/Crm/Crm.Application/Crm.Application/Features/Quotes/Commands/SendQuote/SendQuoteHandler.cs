using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Sending;
using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Features.Quotes.Shared.Mapping;
using Crm.Application.Features.Quotes.Shared.Pdf.Composing;
using Crm.Application.Features.Quotes.Shared.Persistence;
using Crm.Application.Features.Quotes.Shared.Sending;
using Crm.Application.Features.Quotes.Shared.Sending.Gmail;
using Crm.Application.Abstractions.Quotes;
using Crm.Domain.Quotes.ValueObjects;

namespace Crm.Application.Features.Quotes.Commands.SendQuote;

public sealed class SendQuoteHandler(IQuoteStore store, QuotePdfComposer composer, IGmailClient gmail,
    IMailboxProtector protector, TimeProvider clock, ILogger<SendQuoteHandler> logger)
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
        quote.Record(QuoteEventKind.Sent, new QuotePerson(member.UserId, member.Name), $"Кому: {string.Join(", ", recipients)}",
            now);
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
