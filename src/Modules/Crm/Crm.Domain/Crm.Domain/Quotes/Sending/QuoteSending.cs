using BuildingBlocks.Domain;
using BuildingBlocks.Domain.Identifiers.Orders;
using BuildingBlocks.Domain.Identifiers.People;
using Crm.Domain.Quotes;
using BuildingBlocks.Domain.Entity;
using Crm.Domain.Quotes.ValueObjects;

namespace Crm.Domain.Quotes.Sending;

/// <summary>One letter with a КП that Gmail accepted: who sent it, from which address, to whom and when.</summary>
public sealed class QuoteSending : IAggregateRoot
{
    private QuoteSending() { }

    private QuoteSending(QuoteId quoteId, int quoteVersion, QuotePerson sender, string fromEmail, string to, string subject,
        string? providerMessageId, DateTimeOffset sentAt, RequestKey? request = null)
    {
        RequestId = request?.Id;
        RequestHash = request?.Hash;
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

    public static QuoteSending Create(QuoteId quoteId, int quoteVersion, QuotePerson sender, string fromEmail, string to, string subject,
        string? providerMessageId, DateTimeOffset sentAt, RequestKey? request = null) =>
        new(quoteId, quoteVersion, sender, fromEmail, to, subject, providerMessageId, sentAt, request);

    public QuoteSendingId Id { get; private set; }

    /// <summary>The «Надіслати» press; a repeat with the same id sends nothing new (unique).</summary>
    public Guid? RequestId { get; private set; }
    /// <summary>Hash of what the request asked for; a repeat with other data answers 409.</summary>
    public string? RequestHash { get; private set; }
    public QuoteId QuoteId { get; private set; }

    /// <summary>The КП version whose PDF went out.</summary>
    public int QuoteVersion { get; private set; }

    public UserId SenderUserId { get; private set; }
    public string SenderName { get; private set; } = null!;
    public string FromEmail { get; private set; } = null!;

    /// <summary>Recipients as typed, comma separated.</summary>
    public string To { get; private set; } = null!;

    public string Subject { get; private set; } = null!;
    public string? ProviderMessageId { get; private set; }
    public DateTimeOffset SentAt { get; private set; }
}
