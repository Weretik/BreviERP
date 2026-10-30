using System.Net.Mail;
using System.Security.Cryptography;
using System.Text.Json;
using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Delivery.EnqueueQuote;
using Crm.Application.Delivery.Outbox;
using Crm.Application.Delivery.Readiness;
using Crm.Application.Documents.EmailTemplates;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Delivery;
using Crm.Domain.Documents.EmailTemplates;
using Crm.Domain.Quotes;
using Crm.Infrastructure.Persistence.Documents.Artifacts;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence.Delivery;

public sealed partial class QuoteDeliveryStore(CrmDbContext db, TimeProvider? clock = null) : IQuoteDeliveryStore
{
    private readonly TimeProvider time = clock ?? TimeProvider.System;
    public async Task<Result<QuoteDeliveryDetail>> EnqueueAsync(EnqueueQuoteDeliveryCommand command, CrmActor actor,
        IQuoteDeliveryIdentity identity, IQuoteDeliveryReadiness readiness, CancellationToken ct)
    {
        if (actor.UserId <= 0) return Result.Forbidden();
        if (command is null || command.QuoteId == Guid.Empty || command.RequestId == Guid.Empty ||
            command.VersionId == Guid.Empty || command.ArtifactId == Guid.Empty || command.EmailTemplateVersionId == Guid.Empty ||
            command.ExpectedRevision <= 0 || !ValidAddress(command.RecipientAddress) ||
            string.IsNullOrWhiteSpace(command.ManagerName) || command.ManagerName.Length > 200 || command.ManagerName.Any(c => c is '\r' or '\n' or '\0'))
            return Invalid("delivery_request_invalid");
        var normalized = command with { ManagerName = command.ManagerName.Trim() };
        var hash = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(normalized)));
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var committed = false;
        try
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({actor.UserId}, {command.RequestId.GetHashCode()})", ct);
            var old = await db.Set<QuoteDeliveryRequest>().AsNoTracking().SingleOrDefaultAsync(x => x.ActorId == actor.UserId && x.RequestId == command.RequestId, ct);
            var target = old?.QuoteId;
            var quote = target is { } id
                ? await db.Quotes.FromSqlInterpolated($"SELECT * FROM crm.\"Quotes\" WHERE \"Id\"={id.Value} FOR UPDATE").SingleOrDefaultAsync(ct)
                : await db.Quotes.FromSqlInterpolated($"SELECT * FROM crm.\"Quotes\" WHERE \"PublicId\"={command.QuoteId} FOR UPDATE").SingleOrDefaultAsync(ct);
            if (!CanAccess(quote, actor)) return Result.NotFound();
            if (old is not null)
                return old.IntentHash != hash ? Result.Conflict("request_id_payload_conflict") :
                    Result.Success(await DetailAsync(old.DeliveryId, quote!.PublicId, ct));
            if (quote!.Archived || quote.Revision != command.ExpectedRevision || quote.State is not ("ready" or "sent"))
                return Result.Conflict("delivery_quote_not_ready_or_revision_conflict");
            if (!ValidAddress(identity.SenderAddress) || string.IsNullOrWhiteSpace(identity.ProviderKey) || identity.ProviderKey.Length > 80)
                return Result.Unavailable("delivery_provider_unavailable");
            if (!readiness.Assess(command.RecipientAddress).CanSubmit) return Result.Unavailable("delivery_not_ready");
            var version = await db.QuoteVersions.AsNoTracking().SingleOrDefaultAsync(x => x.QuoteId == quote.Id && x.PublicId == command.VersionId, ct);
            if (version is null) return Result.NotFound();
            if (quote.CurrentVersionId != version.Id || quote.IssuedVersionId != version.Id)
                return Result.Conflict("delivery_current_issued_version_required");
            var recipientKey = command.RecipientAddress.ToLowerInvariant();
            var unresolved = await (from pendingDelivery in db.Set<QuoteDelivery>().AsNoTracking()
                join state in db.Set<QuoteDeliveryState>().AsNoTracking() on pendingDelivery.Id equals state.DeliveryId
                where pendingDelivery.QuoteId == quote.Id && pendingDelivery.QuoteVersionId == version.Id &&
                    pendingDelivery.RecipientAddress.ToLower() == recipientKey &&
                    (state.Status == "queued" || state.Status == "sending" || state.Status == "reconciling" || state.Status == "unknown")
                select pendingDelivery.Id).AnyAsync(ct);
            if (unresolved) return Result.Conflict("delivery_unresolved_attempt_exists");
            var issue = await db.QuoteIssues.AsNoTracking().SingleOrDefaultAsync(x => x.QuoteId == quote.Id && x.QuoteVersionId == version.Id, ct);
            var pdf = await db.Set<QuotePdfArtifact>().AsNoTracking().SingleOrDefaultAsync(x => x.PublicId == command.ArtifactId && x.QuoteId == quote.Id && x.QuoteVersionId == version.Id, ct);
            if (issue is null || pdf is null) return Result.NotFound();
            if (pdf.IssuerVersionId != issue.IssuerVersionId || pdf.TemplateVersionId != issue.TemplateVersionId || !pdf.HasIntactContent() ||
                pdf.PdfBytes is not { Length: <= QuotePdfArtifact.MaximumPdfBytes } || !pdf.PdfBytes.AsSpan().StartsWith("%PDF-"u8))
                return Result.Conflict("delivery_pdf_not_ready_or_corrupt");
            var template = await db.Set<EmailTemplateVersion>().AsNoTracking().SingleOrDefaultAsync(x => x.PublicId == command.EmailTemplateVersionId, ct);
            if (template is null) return Result.NotFound();
            RenderedEmailTemplate rendered;
            try { rendered = EmailTemplateTextRenderer.Render(new EmailTemplateDetail(template.PublicId, template.Number, template.Name, template.Subject, template.Body, template.CreatedAt), version.RecipientName, normalized.ManagerName, issue.Number); }
            catch (ArgumentException) { return Invalid("delivery_template_render_invalid"); }
            var now = time.GetUtcNow();
            var delivery = new QuoteDelivery { QuoteId = quote.Id, QuoteVersionId = version.Id, ArtifactId = pdf.Id,
                EmailTemplateVersionId = template.Id, ActorId = actor.UserId, ProviderKey = identity.ProviderKey,
                SenderAddress = identity.SenderAddress, RecipientAddress = command.RecipientAddress,
                Subject = rendered.Subject, PlainText = rendered.Body, PdfBytes = pdf.PdfBytes.ToArray(), PdfSha256 = pdf.Sha256!, CreatedAt = now };
            db.Set<QuoteDelivery>().Add(delivery);
            await db.SaveChangesAsync(ct);
            db.Set<QuoteDeliveryState>().Add(QuoteDeliveryState.Queue(delivery.Id, now));
            db.Set<QuoteDeliveryRequest>().Add(new QuoteDeliveryRequest { ActorId = actor.UserId, RequestId = command.RequestId,
                QuoteId = quote.Id, DeliveryId = delivery.Id, IntentHash = hash, CreatedAt = now });
            Event(delivery, "queued", 0, now);
            await db.SaveChangesAsync(ct);
            var result = await DetailAsync(delivery.Id, quote.PublicId, ct);
            await tx.CommitAsync(ct);
            committed = true;
            return Result.Success(result);
        }
        finally
        {
            if (!committed) await tx.RollbackAsync(CancellationToken.None);
            db.ChangeTracker.Clear();
        }
    }

    private static bool ValidAddress(string? value) => !string.IsNullOrWhiteSpace(value) && value.Length <= 254 &&
        value.All(c => c <= 127 && !char.IsWhiteSpace(c)) && MailAddress.TryCreate(value, out var address) &&
        address.DisplayName.Length == 0 && address.Host.Contains('.') && string.Equals(value, address.Address, StringComparison.OrdinalIgnoreCase);
    private static bool CanAccess(Quote? quote, CrmActor actor) => quote is not null && actor.UserId > 0 &&
        (actor.IsAdmin || quote.OwnerId == new CrmUserId(actor.UserId));
    private static Result<QuoteDeliveryDetail> Invalid(string code) => Result<QuoteDeliveryDetail>.Invalid(new ValidationError("delivery", code));
    private void Event(QuoteDelivery delivery, string kind, int attempt, DateTimeOffset now, string? error = null, string? messageId = null)
        => db.Set<QuoteDeliveryEvent>().Add(new QuoteDeliveryEvent { DeliveryId = delivery.Id, QuoteId = delivery.QuoteId,
            QuoteVersionId = delivery.QuoteVersionId, ActorId = delivery.ActorId, Kind = kind, Attempt = attempt,
            CreatedAt = now, ErrorCode = error, ProviderMessageId = messageId });
}
