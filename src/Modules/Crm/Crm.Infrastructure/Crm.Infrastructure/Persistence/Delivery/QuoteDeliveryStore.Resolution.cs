using System.Security.Cryptography;
using System.Text.Json;
using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Delivery.Outbox;
using Crm.Application.Delivery.ResolveOutcome;
using Crm.Domain.Delivery;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence.Delivery;

public sealed partial class QuoteDeliveryStore
{
    public async Task<Result<QuoteDeliveryDetail>> ResolveAsync(ResolveQuoteDeliveryCommand command, CrmActor actor, CancellationToken ct)
    {
        if (actor.UserId <= 0) return Result.Forbidden();
        if (command is null || !new ResolveQuoteDeliveryValidator().Validate(command).IsValid)
            return Invalid("delivery_resolution_invalid");
        var normalized = command with { EvidenceNote = command.EvidenceNote.Trim() };
        var hash = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(
            new { Operation = "manual_delivery_resolution", Command = normalized })));
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        try
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({actor.UserId}, {command.RequestId.GetHashCode()})", ct);
            var old = await db.Set<QuoteDeliveryRequest>().AsNoTracking()
                .SingleOrDefaultAsync(x => x.ActorId == actor.UserId && x.RequestId == command.RequestId, ct);
            if (old is not null)
            {
                var previousQuote = await db.Quotes.FromSqlInterpolated($"SELECT * FROM crm.\"Quotes\" WHERE \"Id\"={old.QuoteId.Value} FOR UPDATE").SingleOrDefaultAsync(ct);
                if (!CanAccess(previousQuote, actor)) return Result.NotFound();
                return old.IntentHash != hash ? Result.Conflict("request_id_payload_conflict") :
                    Result.Success(await DetailAsync(old.DeliveryId, previousQuote!.PublicId, ct));
            }
            var visibleQuote = await db.Quotes.AsNoTracking().SingleOrDefaultAsync(x => x.PublicId == command.QuoteId, ct);
            if (!CanAccess(visibleQuote, actor)) return Result.NotFound();
            var delivery = await db.Set<QuoteDelivery>().AsNoTracking()
                .SingleOrDefaultAsync(x => x.QuoteId == visibleQuote!.Id && x.PublicId == command.DeliveryId, ct);
            if (delivery is null) return Result.NotFound();
            // Match worker completion order: state -> quote. Recheck current ownership under the quote lock.
            var state = await db.Set<QuoteDeliveryState>().FromSqlInterpolated($"SELECT * FROM crm.\"QuoteDeliveryStates\" WHERE \"DeliveryId\"={delivery.Id.Value} FOR UPDATE").SingleAsync(ct);
            var quote = await db.Quotes.FromSqlInterpolated($"SELECT * FROM crm.\"Quotes\" WHERE \"Id\"={delivery.QuoteId.Value} FOR UPDATE").SingleAsync(ct);
            if (!CanAccess(quote, actor)) return Result.NotFound();
            var now = time.GetUtcNow();
            if (!state.ResolveManually(command.ExpectedAttempt, command.Outcome, now))
                return Result.Conflict("delivery_resolution_state_or_attempt_conflict");
            db.Set<QuoteDeliveryResolution>().Add(new QuoteDeliveryResolution {
                DeliveryId = delivery.Id, QuoteId = delivery.QuoteId, ActorId = actor.UserId,
                Attempt = command.ExpectedAttempt, Outcome = command.Outcome, EvidenceNote = normalized.EvidenceNote, CreatedAt = now
            });
            db.Set<QuoteDeliveryRequest>().Add(new QuoteDeliveryRequest { ActorId = actor.UserId,
                RequestId = command.RequestId, QuoteId = quote.Id, DeliveryId = delivery.Id, IntentHash = hash, CreatedAt = now });
            db.Set<QuoteDeliveryEvent>().Add(new QuoteDeliveryEvent { DeliveryId = delivery.Id, QuoteId = quote.Id,
                QuoteVersionId = delivery.QuoteVersionId, ActorId = actor.UserId, Attempt = state.Attempt,
                Kind = command.Outcome == "sent" ? "manual_succeeded" : "manual_failed", ErrorCode = state.ErrorCode, CreatedAt = now });
            if (command.Outcome == "sent") quote.MarkDeliverySent(delivery.QuoteVersionId);
            await db.SaveChangesAsync(ct);
            var detail = await DetailAsync(delivery.Id, quote.PublicId, ct);
            await tx.CommitAsync(ct);
            return Result.Success(detail);
        }
        finally { db.ChangeTracker.Clear(); }
    }
}
