using Crm.Domain.Quotes.ValueObjects;

namespace Crm.Domain.Quotes.Sending;

/// <summary>One letter with a КП that Gmail accepted: who sent it, from which address, to whom and when.</summary>
public sealed class QuoteSending
{
    private QuoteSending() { }

    public QuoteSending(long quoteId, int quoteVersion, QuotePerson sender, string fromEmail, string to, string subject,
        string? providerMessageId, DateTimeOffset sentAt)
    {
        QuoteId = quoteId;
        QuoteVersion = quoteVersion;
        SenderUserId = sender.UserId;
        SenderName = sender.Name;
        FromEmail = fromEmail;
        To = to;
        Subject = subject;
        ProviderMessageId = providerMessageId;
        SentAt = sentAt;
    }

    public long Id { get; private set; }
    public long QuoteId { get; private set; }

    /// <summary>The КП version whose PDF went out.</summary>
    public int QuoteVersion { get; private set; }

    public int SenderUserId { get; private set; }
    public string SenderName { get; private set; } = null!;
    public string FromEmail { get; private set; } = null!;

    /// <summary>Recipients as typed, comma separated.</summary>
    public string To { get; private set; } = null!;

    public string Subject { get; private set; } = null!;
    public string? ProviderMessageId { get; private set; }
    public DateTimeOffset SentAt { get; private set; }
}
