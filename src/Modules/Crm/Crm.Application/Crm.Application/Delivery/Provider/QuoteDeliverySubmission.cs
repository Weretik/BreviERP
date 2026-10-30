namespace Crm.Application.Delivery.Provider;

/// <summary>Internal pinned public message input; From always comes from host configuration.</summary>
public sealed record QuoteDeliverySubmission(Guid DeliveryId, DateTimeOffset RequestedAt,
    string RecipientAddress, string Subject, string PlainText, ReadOnlyMemory<byte> PdfBytes);
