using Crm.Application.Delivery.Provider;
using Crm.Domain.Delivery;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence.Delivery;

public sealed partial class QuoteDeliveryStore
{
    public async Task<bool> CompleteAsync(Guid deliveryId, Guid leaseToken, QuoteDeliveryProviderResult outcome, CancellationToken ct)
    {
        if (deliveryId == Guid.Empty || leaseToken == Guid.Empty) return false;
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        try
        {
            var delivery = await db.Set<QuoteDelivery>().AsNoTracking().SingleOrDefaultAsync(x => x.PublicId == deliveryId, ct);
            if (delivery is null) return false;
            var state = await db.Set<QuoteDeliveryState>().FromSqlInterpolated($"SELECT * FROM crm.\"QuoteDeliveryStates\" WHERE \"DeliveryId\"={delivery.Id.Value} FOR UPDATE").SingleAsync(ct);
            var now = time.GetUtcNow();
            if (!state.Owns(leaseToken, now)) return false;
            var status = outcome.Outcome switch {
                QuoteDeliveryProviderOutcome.ConfirmedSuccess => "succeeded",
                QuoteDeliveryProviderOutcome.ConfirmedFailure => "failed",
                _ => "unknown" };
            var error = SafeCode(outcome.ErrorCode);
            var providerId = SafeProviderId(outcome.ProviderMessageId);
            if (status == "succeeded" && providerId is null) { status = "unknown"; error = "delivery_confirmation_invalid"; }
            state.Complete(leaseToken, status, error, providerId, now);
            Event(delivery, status, state.Attempt, now, error, providerId);
            if (status == "succeeded")
            {
                var quote = await db.Quotes.FromSqlInterpolated($"SELECT * FROM crm.\"Quotes\" WHERE \"Id\"={delivery.QuoteId.Value} FOR UPDATE").SingleAsync(ct);
                quote.MarkDeliverySent(delivery.QuoteVersionId);
            }
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return true;
        }
        finally { db.ChangeTracker.Clear(); }
    }

    private static string? SafeCode(string? value) => value is null ? null : value.Length is > 0 and <= 80 &&
        value.All(c => char.IsAsciiLetterOrDigit(c) || c == '_') ? value : "delivery_provider_error";
    private static string? SafeProviderId(string? value) => value is { Length: > 0 and <= 200 } &&
        value.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_') ? value : null;
}
