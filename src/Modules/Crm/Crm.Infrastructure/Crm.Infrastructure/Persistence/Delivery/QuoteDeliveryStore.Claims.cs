using System.Security.Cryptography;
using Crm.Application.Delivery.Outbox;
using Crm.Application.Delivery.Provider;
using Crm.Domain.Delivery;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence.Delivery;

public sealed partial class QuoteDeliveryStore
{
    public async Task<QuoteDeliveryClaim?> ClaimAsync(IQuoteDeliveryIdentity identity, Func<string, bool> canProcessRecipient, CancellationToken ct)
    {
        if (identity.LeaseSeconds is < 30 or > 300) return null;
        var now = time.GetUtcNow();
        var recipientRestriction = identity.RecipientRestriction?.ToLowerInvariant();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        try
        {
            var candidates = await db.Set<QuoteDeliveryState>().FromSqlInterpolated($"""
                SELECT s.* FROM crm."QuoteDeliveryStates" s
                JOIN crm."QuoteDeliveries" d ON d."Id" = s."DeliveryId"
                WHERE d."ProviderKey" = {identity.ProviderKey} AND d."SenderAddress" = {identity.SenderAddress}
                  AND (CAST({recipientRestriction} AS text) IS NULL OR lower(d."RecipientAddress") = {recipientRestriction})
                  AND (s."Status" = 'queued' OR (s."Status" = 'unknown' AND s."NextCheckAt" <= {now})
                       OR (s."Status" IN ('sending','reconciling') AND s."LeaseUntil" <= {now}))
                ORDER BY s."UpdatedAt", s."DeliveryId" LIMIT 20 FOR UPDATE OF s SKIP LOCKED
                """).ToArrayAsync(ct);
            foreach (var state in candidates)
            {
                var delivery = await db.Set<QuoteDelivery>().AsNoTracking().SingleAsync(x => x.Id == state.DeliveryId, ct);
                if (!canProcessRecipient(delivery.RecipientAddress)) continue;
                if (state.Status == "queued")
                {
                    var current = await db.Quotes.FromSqlInterpolated($"SELECT * FROM crm.\"Quotes\" WHERE \"Id\"={delivery.QuoteId.Value} FOR UPDATE").SingleAsync(ct);
                    var archived = current.Archived;
                    if (archived)
                    {
                        state.CancelQueued(now);
                        Event(delivery, "cancelled", state.Attempt, now, state.ErrorCode);
                        continue;
                    }
                }
                if (delivery.PdfBytes.Length is < 5 or > 20971520 ||
                    !delivery.PdfBytes.AsSpan().StartsWith("%PDF-"u8) ||
                    delivery.PdfSha256 != Convert.ToHexString(SHA256.HashData(delivery.PdfBytes)))
                    throw new InvalidDataException("Pinned delivery content is corrupt.");
                var claim = state.Claim(now, TimeSpan.FromSeconds(identity.LeaseSeconds));
                Event(delivery, claim.Reconcile ? "reconcile_started" : "submit_started", state.Attempt, now);
                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return new QuoteDeliveryClaim(delivery.PublicId, claim.Token, claim.Reconcile,
                    new QuoteDeliverySubmission(delivery.PublicId, delivery.CreatedAt, delivery.RecipientAddress,
                        delivery.Subject, delivery.PlainText, delivery.PdfBytes));
            }
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return null;
        }
        finally { db.ChangeTracker.Clear(); }
    }
}
