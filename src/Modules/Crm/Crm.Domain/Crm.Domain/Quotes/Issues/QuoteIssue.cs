using Crm.Domain.Documents.Issuer;
using Crm.Domain.Documents.Templates;
using Crm.Domain.Quotes.Identifiers;

namespace Crm.Domain.Quotes.Issues;

// One immutable settings/number pin for each customer-ready saved version.
public sealed class QuoteIssue
{
    public long Id { get; private set; }
    public QuoteId QuoteId { get; private set; }
    public long QuoteVersionId { get; private set; }
    public IssuerSettingsVersionId IssuerVersionId { get; private set; }
    public DocumentTemplateVersionId TemplateVersionId { get; private set; }
    public string Number { get; private set; } = null!;
    public DateTimeOffset ReadyAt { get; private set; }

    private QuoteIssue() { }

    public static QuoteIssue Create(QuoteId quoteId, long quoteVersionId,
        IssuerSettingsVersionId issuerVersionId, DocumentTemplateVersionId templateVersionId,
        string number, DateTimeOffset readyAt)
    {
        if (quoteId.Value <= 0 || quoteVersionId <= 0 || issuerVersionId.Value <= 0 ||
            templateVersionId.Value <= 0 || string.IsNullOrWhiteSpace(number) || number.Length > 80 ||
            readyAt.Offset != TimeSpan.Zero)
            throw new QuoteValidationException("invalid_quote_issue");
        return new QuoteIssue
        {
            QuoteId = quoteId,
            QuoteVersionId = quoteVersionId,
            IssuerVersionId = issuerVersionId,
            TemplateVersionId = templateVersionId,
            Number = number,
            ReadyAt = readyAt
        };
    }
}
