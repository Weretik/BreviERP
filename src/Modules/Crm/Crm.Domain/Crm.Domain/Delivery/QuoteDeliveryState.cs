namespace Crm.Domain.Delivery;

/// <summary>Only the first queued claim may submit; every uncertain claim is read-only reconciliation.</summary>
public sealed class QuoteDeliveryState
{
    public QuoteDeliveryId DeliveryId { get; private set; }
    public string Status { get; private set; } = "queued";
    public int Attempt { get; private set; }
    public Guid? LeaseToken { get; private set; }
    public DateTimeOffset? LeaseUntil { get; private set; }
    public DateTimeOffset? NextCheckAt { get; private set; }
    public string? ErrorCode { get; private set; }
    public string? ProviderMessageId { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? ConfirmedAt { get; private set; }

    private QuoteDeliveryState() { }
    public static QuoteDeliveryState Queue(QuoteDeliveryId id, DateTimeOffset now)
        => id.Value <= 0 ? throw new ArgumentException("invalid_delivery_id") : new() { DeliveryId = id, UpdatedAt = now };

    public bool IsClaimable(DateTimeOffset now) => Status == "queued" ||
        (Status == "unknown" && NextCheckAt <= now) ||
        (Status is "sending" or "reconciling" && LeaseUntil <= now);

    public (Guid Token, bool Reconcile) Claim(DateTimeOffset now, TimeSpan lease)
    {
        if (!IsClaimable(now) || lease < TimeSpan.FromSeconds(30) || lease > TimeSpan.FromSeconds(300))
            throw new InvalidOperationException("delivery_not_claimable");
        var reconcile = Status != "queued";
        Status = reconcile ? "reconciling" : "sending";
        LeaseToken = Guid.NewGuid();
        LeaseUntil = now.Add(lease);
        NextCheckAt = null;
        Attempt = checked(Attempt + 1);
        UpdatedAt = now;
        return (LeaseToken.Value, reconcile);
    }

    public bool Owns(Guid token, DateTimeOffset now) => token != Guid.Empty && LeaseToken == token &&
        Status is "sending" or "reconciling" && LeaseUntil > now;

    public bool Complete(Guid token, string outcome, string? errorCode, string? providerMessageId, DateTimeOffset now)
    {
        if (!Owns(token, now)) return false;
        if (outcome is not ("succeeded" or "failed" or "unknown")) throw new ArgumentException("invalid_delivery_outcome");
        if (errorCode is not null && (errorCode.Length > 80 || errorCode.Any(c => !char.IsAsciiLetterOrDigit(c) && c != '_')))
            throw new ArgumentException("invalid_delivery_error");
        if (providerMessageId is not null && (providerMessageId.Length > 200 || providerMessageId.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not ('-' or '_'))))
            throw new ArgumentException("invalid_provider_id");
        Status = outcome;
        ErrorCode = errorCode;
        ProviderMessageId = providerMessageId;
        LeaseToken = null;
        LeaseUntil = null;
        NextCheckAt = outcome == "unknown" ? now.AddMinutes(5) : null;
        ConfirmedAt = outcome == "succeeded" ? now : null;
        UpdatedAt = now;
        return true;
    }

    public void CancelQueued(DateTimeOffset now)
    {
        if (Status != "queued") throw new InvalidOperationException("delivery_already_attempted");
        Status = "failed";
        ErrorCode = "delivery_quote_archived";
        UpdatedAt = now;
    }
}
