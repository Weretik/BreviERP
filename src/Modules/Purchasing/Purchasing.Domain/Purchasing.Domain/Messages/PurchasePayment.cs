namespace Purchasing.Domain.Messages;

/// <summary>
/// «Оплата» of a purchase invoice: an expense of «Бухгалтерія» linked by hand (Accounting operation id, no database link
/// across modules). A removed link stays with who removed it and when; linking again adds a new row.
/// </summary>
public sealed class PurchasePayment
{
    private PurchasePayment() { }

    internal PurchasePayment(string messageId, Guid transactionId, string by, DateTimeOffset at)
    {
        MessageId = messageId;
        TransactionId = transactionId;
        LinkedBy = by;
        LinkedAt = at;
    }

    public long Id { get; private set; }
    public string MessageId { get; private set; } = null!;

    /// <summary>The expense in «Бухгалтерія».</summary>
    public Guid TransactionId { get; private set; }

    public string LinkedBy { get; private set; } = null!;
    public DateTimeOffset LinkedAt { get; private set; }
    public string? UnlinkedBy { get; private set; }
    public DateTimeOffset? UnlinkedAt { get; private set; }

    public bool Active => UnlinkedAt is null;

    internal void Unlink(string by, DateTimeOffset at)
    {
        UnlinkedBy = by;
        UnlinkedAt = at;
    }
}
