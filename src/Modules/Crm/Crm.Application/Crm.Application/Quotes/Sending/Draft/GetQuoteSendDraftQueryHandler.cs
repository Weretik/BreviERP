using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Pdf.Composing;
using Crm.Application.Quotes.Persistence;
using Crm.Application.Quotes.Sending.Gmail;

namespace Crm.Application.Quotes.Sending.Draft;

/// <summary>Letter texts use the sender's КП contact, because the client answers the person who wrote.</summary>
public sealed class GetQuoteSendDraftQueryHandler(IQuoteStore store, QuotePdfComposer composer, IGmailClient gmail)
    : IQueryHandler<GetQuoteSendDraftQuery, Result<QuoteSendDraftResponse>>
{
    public async ValueTask<Result<QuoteSendDraftResponse>> Handle(GetQuoteSendDraftQuery query,
        CancellationToken cancellationToken)
    {
        var quote = await store.Quotes.AsNoTracking().SingleOrDefaultAsync(x => x.Id == query.Id, cancellationToken);
        if (quote is null || !query.Actor.CanAccess(quote)) return Result.NotFound(QuoteMessages.NotFound);

        var context = await composer.LoadContextAsync(cancellationToken);
        var sender = context.ContactOf(await composer.MemberAsync(query.Actor.UserId, "", cancellationToken));
        var mailbox = await store.StaffMailboxes.AsNoTracking()
            .Where(x => x.UserId == query.Actor.UserId).Select(x => x.Email).SingleOrDefaultAsync(cancellationToken);
        var site = context.Settings.Company.Site;
        string Fill(string text) => QuoteMarks.Fill(text, sender, site, quote.Number);

        var letters = new List<QuoteLetterResponse>
        {
            new("Стандартний лист", Fill(context.Settings.Email.Subject), Fill(context.Settings.Email.Body)),
        };
        letters.AddRange(context.Settings.LetterVariants.Select(x => new QuoteLetterResponse(x.Name, Fill(x.Subject),
            Fill(x.Body))));
        return Result.Success(new QuoteSendDraftResponse(mailbox, sender.Name, gmail.IsConfigured,
            QuotePdfComposer.FileName(quote), letters));
    }
}
