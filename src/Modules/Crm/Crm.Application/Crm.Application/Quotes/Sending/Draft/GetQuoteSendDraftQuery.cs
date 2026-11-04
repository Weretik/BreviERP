using Crm.Application.Quotes.Access;

namespace Crm.Application.Quotes.Sending.Draft;

/// <summary>What «Надіслати КП» starts from: the sender's Gmail and the letters with marks already filled.</summary>
public sealed record GetQuoteSendDraftQuery(QuoteActor Actor, long Id) : IQuery<Result<QuoteSendDraftResponse>>;

/// <param name="FromEmail">The signed-in person's connected Gmail; null until they connect it.</param>
/// <param name="Letters">For a КП «під запит» the template from «Налаштування КП» first, then the letter variants;
/// for an introduction the chosen variant first, then the other variants and the template (R-18).</param>
/// <param name="To">The recipient saved in an introduction; null for a КП «під запит».</param>
public sealed record QuoteSendDraftResponse(
    string? FromEmail,
    string FromName,
    bool GmailConfigured,
    string FileName,
    IReadOnlyList<QuoteLetterResponse> Letters,
    string? To = null);

public sealed record QuoteLetterResponse(string Name, string Subject, string Body);

public sealed class GetQuoteSendDraftQueryValidator : AbstractValidator<GetQuoteSendDraftQuery>
{
    public GetQuoteSendDraftQueryValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
