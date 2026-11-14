using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Features.Quotes.Shared.Pdf.Composing;
using Crm.Application.Features.Quotes.Shared.Persistence;
using Crm.Application.Features.Quotes.Shared.Sending;
using Crm.Application.Features.Quotes.Shared.Sending.Gmail;
using Crm.Application.Abstractions.Quotes;
using Crm.Application.Abstractions.Quotes.Sending;

namespace Crm.Application.Features.Quotes.Queries.GetQuoteSendDraft;

/// <summary>Letter texts use the sender's КП contact, because the client answers the person who wrote.</summary>
public sealed class GetQuoteSendDraftHandler(IQuoteStore store, QuotePdfComposer composer, IGmailClient gmail)
    : IQueryHandler<GetQuoteSendDraftQuery, Result<QuoteSendDraftResponse>>
{
    public const string StandardLetter = "Стандартний лист";

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
        string Fill(string text) => QuoteMarks.Fill(text, sender, site, quote.Number, quote.RecipientName, quote.CompanyName);

        var standard = new QuoteLetterResponse(StandardLetter, Fill(context.Settings.Email.Subject),
            Fill(context.Settings.Email.Body));
        var introduction = quote.Introduction;
        // The introduction's own subject and text replace its chosen variant's (round 2, item 14).
        var variants = context.Settings.LetterVariants
            .Select(x => x.Name == introduction?.LetterName
                ? new QuoteLetterResponse(x.Name, Fill(introduction.LetterSubject ?? x.Subject),
                    Fill(introduction.LetterBody ?? x.Body))
                : new QuoteLetterResponse(x.Name, Fill(x.Subject), Fill(x.Body)))
            .ToList();
        IReadOnlyList<QuoteLetterResponse> letters = introduction is null
            ? [standard, .. variants]
            : [.. variants.OrderByDescending(x => x.Name == introduction.LetterName), standard];
        return Result.Success(new QuoteSendDraftResponse(mailbox, sender.Name, gmail.IsConfigured,
            QuotePdfComposer.FileName(quote), letters, introduction?.RecipientEmail));
    }
}
