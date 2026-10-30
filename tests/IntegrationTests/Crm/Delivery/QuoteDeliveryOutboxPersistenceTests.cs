using System.Security.Cryptography;
using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Delivery.EnqueueQuote;
using Crm.Application.Delivery.Outbox;
using Crm.Application.Delivery.Processing;
using Crm.Application.Delivery.Provider;
using Crm.Application.Delivery.Readiness;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Delivery;
using Crm.Domain.Documents.EmailTemplates;
using Crm.Domain.Documents.Issuer;
using Crm.Domain.Documents.Templates;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Issues;
using Crm.Domain.Quotes.Versions;
using Crm.Infrastructure.Persistence.Delivery;
using Crm.Infrastructure.Persistence.Documents.Artifacts;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace IntegrationTests.Crm.Delivery;

[NonParallelizable]
public sealed class QuoteDeliveryOutboxPersistenceTests : CrmDatabaseFixture
{
    private static readonly CrmActor Owner = new(17, false);
    private static readonly CrmActor Stranger = new(18, false);
    private static readonly Identity Settings = new();
    private static readonly Ready Readiness = new();

    [Test]
    public async Task Enqueue_replays_one_pin_conflicts_on_changed_intent_and_rechecks_current_acl()
    {
        var request = await SeedAsync();
        await using var db = Context();
        var store = new QuoteDeliveryStore(db);
        var first = await store.EnqueueAsync(request, Owner, Settings, Readiness, default);
        Assert.That(first.IsSuccess, Is.True, string.Join(',', first.Errors));
        Assert.That((await store.EnqueueAsync(request, Owner, Settings, Readiness, default)).Value.DeliveryId, Is.EqualTo(first.Value.DeliveryId));
        Assert.That((await store.EnqueueAsync(request with { RecipientAddress = "other@example.test" }, Owner, Settings, Readiness, default)).Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(await db.Set<QuoteDelivery>().CountAsync(), Is.EqualTo(1));
        var pin = await db.Set<QuoteDelivery>().SingleAsync();
        Assert.That(pin.Subject, Is.EqualTo("КП 10/10/2026"));
        Assert.That(pin.PlainText, Is.EqualTo("Вітаємо Recipient. Manager"));
        Assert.That(pin.PdfSha256, Is.EqualTo(Convert.ToHexString(SHA256.HashData(pin.PdfBytes))));
        var quote = await db.Quotes.SingleAsync();
        quote.Transfer(quote.Revision, new CrmUserId(Stranger.UserId), Owner.UserId, false, "Transfer");
        await db.SaveChangesAsync();
        Assert.That((await store.EnqueueAsync(request with { ManagerName = "Changed" }, Owner, Settings, Readiness, default)).Status, Is.EqualTo(ResultStatus.NotFound));
        Assert.That((await store.GetAsync(request.QuoteId, first.Value.DeliveryId, Owner, default)).Status, Is.EqualTo(ResultStatus.NotFound));
        Assert.That((await store.ListAsync(request.QuoteId, Stranger, 1, 10, default)).Value.Value, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Confirmed_provider_success_is_atomic_once_and_provider_io_is_outside_transaction()
    {
        var request = await SeedAsync();
        await using var db = Context();
        var store = new QuoteDeliveryStore(db);
        await store.EnqueueAsync(request, Owner, Settings, Readiness, default);
        var provider = new Provider(() => Assert.That(db.Database.CurrentTransaction, Is.Null));
        var processor = new QuoteDeliveryProcessor(store, provider, Settings, Readiness);
        Assert.That(await processor.ProcessOneAsync(default), Is.True);
        Assert.That(await processor.ProcessOneAsync(default), Is.False);
        Assert.That(provider.Submits, Is.EqualTo(1));
        Assert.That(provider.Lookups, Is.Zero);
        Assert.That((await db.Quotes.SingleAsync()).State, Is.EqualTo("sent"));
        Assert.That(await db.Set<QuoteDeliveryEvent>().CountAsync(x => x.Kind == "succeeded"), Is.EqualTo(1));
        Assert.That((await db.Set<QuoteDeliveryState>().SingleAsync()).Status, Is.EqualTo("succeeded"));
    }

    [Test]
    public async Task Expired_attempt_reconciles_only_late_finalizer_cannot_mark_sent_and_unknown_backs_off()
    {
        var request = await SeedAsync();
        var clock = new Clock();
        await using var db = Context();
        var store = new QuoteDeliveryStore(db, clock);
        await store.EnqueueAsync(request, Owner, Settings, Readiness, default);
        var first = await store.ClaimAsync(Settings, _ => true, default);
        Assert.That(first, Is.Not.Null);
        clock.Advance(TimeSpan.FromSeconds(121));
        var second = await store.ClaimAsync(Settings, _ => true, default);
        Assert.That(second!.Reconcile, Is.True);
        Assert.That(await store.CompleteAsync(first!.DeliveryId, first.LeaseToken, Success, default), Is.False);
        Assert.That(await store.CompleteAsync(second.DeliveryId, second.LeaseToken,
            new QuoteDeliveryProviderResult(QuoteDeliveryProviderOutcome.Unknown, null, "lookup_empty"), default), Is.True);
        Assert.That(await store.ClaimAsync(Settings, _ => true, default), Is.Null);
        Assert.That((await db.Quotes.SingleAsync()).State, Is.EqualTo("ready"));
        clock.Advance(TimeSpan.FromMinutes(6));
        var provider = new Provider();
        Assert.That(await new QuoteDeliveryProcessor(store, provider, Settings, Readiness).ProcessOneAsync(default), Is.True);
        Assert.That(provider.Submits, Is.Zero);
        Assert.That(provider.Lookups, Is.EqualTo(1));
    }

    [Test]
    public async Task Parallel_claims_submit_once_and_preserve_newer_draft_on_success()
    {
        var request = await SeedAsync();
        await using var db = Context();
        var store = new QuoteDeliveryStore(db);
        await store.EnqueueAsync(request, Owner, Settings, Readiness, default);
        async Task<QuoteDeliveryClaim?> Claim() { await using var own = Context(); return await new QuoteDeliveryStore(own).ClaimAsync(Settings, _ => true, default); }
        var claims = await Task.WhenAll(Claim(), Claim());
        Assert.That(claims.Count(x => x is not null), Is.EqualTo(1));
        var claim = claims.Single(x => x is not null)!;
        var quote = await db.Quotes.SingleAsync();
        var next = QuoteVersion.Create(quote.Id, 2, quote.CurrentVersionId, "edited", "New recipient", null, null, Owner.UserId, []);
        db.QuoteVersions.Add(next);
        await db.SaveChangesAsync();
        quote.AppendVersion(quote.Revision, next.Id, Owner.UserId);
        await db.SaveChangesAsync();
        Assert.That(await store.CompleteAsync(claim.DeliveryId, claim.LeaseToken, Success, default), Is.True);
        Assert.That((await db.Quotes.SingleAsync()).State, Is.EqualTo("draft"));
        Assert.That(await db.Set<QuoteDeliveryEvent>().CountAsync(x => x.Kind == "succeeded"), Is.EqualTo(1));
    }

    [Test]
    public async Task Archive_cancels_queued_without_http_and_identity_or_readiness_change_cannot_claim()
    {
        var request = await SeedAsync();
        await using var db = Context();
        var store = new QuoteDeliveryStore(db);
        await store.EnqueueAsync(request, Owner, Settings, Readiness, default);
        Assert.That(await store.ClaimAsync(new Identity("different@example.test"), _ => true, default), Is.Null);
        Assert.That(await store.ClaimAsync(Settings, _ => false, default), Is.Null);
        var quote = await db.Quotes.SingleAsync();
        quote.SetArchived(quote.Revision, true, Owner.UserId, false, "Archive");
        await db.SaveChangesAsync();
        var provider = new Provider();
        Assert.That(await new QuoteDeliveryProcessor(store, provider, Settings, Readiness).ProcessOneAsync(default), Is.False);
        Assert.That(provider.Submits, Is.Zero);
        Assert.That((await db.Set<QuoteDeliveryState>().SingleAsync()).ErrorCode, Is.EqualTo("delivery_quote_archived"));
        Assert.That((await store.EnqueueAsync(request, Owner, Settings, Readiness, default)).IsSuccess, Is.True);
        Assert.That((await store.EnqueueAsync(request with { RequestId = Guid.NewGuid() }, Owner, Settings, Readiness, default)).Status, Is.EqualTo(ResultStatus.Conflict));
    }

    [Test]
    public async Task Missing_readiness_or_foreign_pins_do_not_insert_and_immutable_evidence_rejects_sql_updates()
    {
        var request = await SeedAsync();
        await using var db = Context();
        var store = new QuoteDeliveryStore(db);
        Assert.That((await store.EnqueueAsync(request, Owner, Settings, new Ready(false), default)).Status, Is.EqualTo(ResultStatus.Unavailable));
        Assert.That((await store.EnqueueAsync(request with { ArtifactId = Guid.NewGuid() }, Owner, Settings, Readiness, default)).Status, Is.EqualTo(ResultStatus.NotFound));
        Assert.That(await db.Set<QuoteDelivery>().CountAsync(), Is.Zero);
        await store.EnqueueAsync(request, Owner, Settings, Readiness, default);
        // Fixed test-only identifier allowlist; no user-controlled SQL identifiers.
        foreach (var sql in new[] {
            "UPDATE crm.\"QuoteDeliveries\" SET \"CreatedAt\"=\"CreatedAt\"",
            "UPDATE crm.\"QuoteDeliveryEvents\" SET \"CreatedAt\"=\"CreatedAt\"",
            "UPDATE crm.\"QuoteDeliveryRequests\" SET \"CreatedAt\"=\"CreatedAt\"" })
        {
            var exception = Assert.ThrowsAsync<PostgresException>(async () =>
                await db.Database.ExecuteSqlRawAsync(sql));
            Assert.That(exception!.SqlState, Is.EqualTo("P0001"));
        }
    }

    [Test]
    public async Task New_request_key_cannot_resend_pending_or_unknown_recipient_until_confirmed_outcome()
    {
        var request = await SeedAsync();
        var clock = new Clock();
        await using var db = Context();
        var store = new QuoteDeliveryStore(db, clock);
        var first = await store.EnqueueAsync(request, Owner, Settings, Readiness, default);
        Assert.That(first.IsSuccess, Is.True);
        var changedKey = request with { RequestId = Guid.NewGuid(), RecipientAddress = request.RecipientAddress.ToUpperInvariant() };
        var duplicateQueued = await store.EnqueueAsync(changedKey, Owner, Settings, Readiness, default);
        Assert.That(duplicateQueued.Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(duplicateQueued.Errors, Does.Contain("delivery_unresolved_attempt_exists"));
        var claim = await store.ClaimAsync(Settings, _ => true, default);
        Assert.That(await store.CompleteAsync(claim!.DeliveryId, claim.LeaseToken,
            new QuoteDeliveryProviderResult(QuoteDeliveryProviderOutcome.Unknown, null, "lookup_empty"), default), Is.True);
        Assert.That((await store.EnqueueAsync(changedKey, Owner, Settings, Readiness, default)).Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That((await store.EnqueueAsync(request, Owner, Settings, Readiness, default)).Value.DeliveryId, Is.EqualTo(first.Value.DeliveryId));
        var otherRecipient = await store.EnqueueAsync(request with { RequestId = Guid.NewGuid(), RecipientAddress = "another@example.test" }, Owner, Settings, Readiness, default);
        Assert.That(otherRecipient.IsSuccess, Is.True);
        clock.Advance(TimeSpan.FromMinutes(6));
        var lookup = await store.ClaimAsync(Settings, _ => true, default);
        Assert.That(lookup!.DeliveryId, Is.EqualTo(first.Value.DeliveryId));
        Assert.That(lookup.Reconcile, Is.True);
        Assert.That(await store.CompleteAsync(lookup.DeliveryId, lookup.LeaseToken, Success, default), Is.True);
        var revision = (await db.Quotes.SingleAsync()).Revision;
        var explicitResend = await store.EnqueueAsync(changedKey with { ExpectedRevision = revision }, Owner, Settings, Readiness, default);
        Assert.That(explicitResend.IsSuccess, Is.True);
        Assert.That(explicitResend.Value.DeliveryId, Is.Not.EqualTo(first.Value.DeliveryId));
    }

    [Test]
    public async Task Sandbox_recipient_after_twenty_ineligible_messages_is_still_claimed()
    {
        var request = await SeedAsync();
        await using var db = Context();
        var clock = new Clock();
        var store = new QuoteDeliveryStore(db, clock);
        for (var index = 0; index < 21; index++)
        {
            var recipient = index == 20 ? "allowed@example.test" : $"blocked{index}@example.test";
            var queued = await store.EnqueueAsync(request with { RequestId = Guid.NewGuid(), RecipientAddress = recipient },
                Owner, Settings, Readiness, default);
            Assert.That(queued.IsSuccess, Is.True);
            clock.Advance(TimeSpan.FromSeconds(1));
        }
        var options = new global::Crm.Infrastructure.Delivery.Readiness.QuoteDeliveryProviderOptions
        {
            Enabled = true, ProviderKey = Settings.ProviderKey, SenderAddress = Settings.SenderAddress,
            SandboxOnly = true, TestRecipientAddress = "ALLOWED@example.test"
        };
        var identity = new global::Crm.Infrastructure.Delivery.Readiness.ConfiguredQuoteDeliveryIdentity(options);
        var readiness = new global::Crm.Infrastructure.Delivery.Readiness.QuoteDeliveryReadinessPolicy(options,
            new QuoteDeliveryProviderCapabilities("gmail", true, true), credentialConfigured: true);
        var claim = await store.ClaimAsync(identity, recipient => readiness.Assess(recipient).CanSubmit, default);
        Assert.That(claim, Is.Not.Null, "Older blocked recipients must not starve an eligible sandbox message.");
        Assert.That(claim!.Submission.RecipientAddress, Is.EqualTo("allowed@example.test"));
        Assert.That(await db.Set<QuoteDeliveryState>().CountAsync(x => x.Status == "queued"), Is.EqualTo(20));
    }

    private static readonly QuoteDeliveryProviderResult Success = new(QuoteDeliveryProviderOutcome.ConfirmedSuccess, "msg1", null);
    private async Task<EnqueueQuoteDeliveryCommand> SeedAsync()
    {
        await using var db = Context();
        var actor = new CrmUserId(Owner.UserId);
        var quote = Quote.Create(null, null, "Recipient", actor);
        db.Quotes.Add(quote); await db.SaveChangesAsync();
        var version = QuoteVersion.Create(quote.Id, 1, null, "created", "Recipient", null, null, Owner.UserId, []);
        db.QuoteVersions.Add(version); await db.SaveChangesAsync();
        quote.SetInitialVersion(version.Id);
        var issuer = IssuerSettingsVersion.Create(1, "Seller", "12345678", "1234567890", "UA123456789012345678901234567", "Kyiv", "+380501234567", "seller@example.test", "https://brevi.com.ua", null, null, null, actor);
        var document = DocumentTemplateVersion.Create(1, "standard", "КП", "Ціни в грн", "Вітаємо", "Контакт", "Напишіть", "{managerName}", actor);
        var emailNumber = (await db.EmailTemplateVersions.MaxAsync(x => (int?)x.Number) ?? 0) + 1;
        var email = EmailTemplateVersion.Create(emailNumber, "Template", "КП {quoteNumber}", "Вітаємо {recipientName}. {managerName}", actor);
        db.IssuerSettingsVersions.Add(issuer); db.DocumentTemplateVersions.Add(document); db.EmailTemplateVersions.Add(email);
        await db.SaveChangesAsync();
        quote.PrepareReady(quote.Revision, "10/10/2026", Owner.UserId, false);
        db.QuoteIssues.Add(QuoteIssue.Create(quote.Id, version.Id, issuer.Id, document.Id, quote.Number!, DateTimeOffset.UtcNow));
        var pdf = QuotePdfArtifact.Queue(quote.Id, version.Id, issuer.Id, document.Id, "fixture", DateTimeOffset.UtcNow);
        var token = pdf.Claim(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(2));
        pdf.Complete(token, "%PDF-1.7\nfixture"u8.ToArray(), DateTimeOffset.UtcNow);
        db.Set<QuotePdfArtifact>().Add(pdf); await db.SaveChangesAsync();
        return new EnqueueQuoteDeliveryCommand(quote.PublicId, Guid.NewGuid(), quote.Revision, version.PublicId,
            pdf.PublicId, email.PublicId, "sandbox@example.test", "Manager");
    }

    private sealed class Identity(string sender = "sender@example.test") : IQuoteDeliveryIdentity
    { public string ProviderKey => "gmail"; public string SenderAddress => sender; public int LeaseSeconds => 120; }
    private sealed class Ready(bool ready = true) : IQuoteDeliveryReadiness
    { public QuoteDeliveryReadiness Assess(string recipientAddress) => new(ready, ready ? [] : ["fixture_disabled"]); }
    private sealed class Clock : TimeProvider
    {
        private DateTimeOffset now = new(2026, 10, 30, 12, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => now;
        public void Advance(TimeSpan value) => now = now.Add(value);
    }
    private sealed class Provider(Action? onCall = null) : IQuoteDeliveryProvider
    {
        public int Submits { get; private set; }
        public int Lookups { get; private set; }
        public QuoteDeliveryProviderCapabilities Capabilities => new("gmail", true, true);
        public Task<QuoteDeliveryProviderResult> SubmitAsync(QuoteDeliverySubmission submission, CancellationToken ct)
        { onCall?.Invoke(); Submits++; return Task.FromResult(Success); }
        public Task<QuoteDeliveryProviderResult> ReconcileAsync(Guid deliveryId, CancellationToken ct)
        { onCall?.Invoke(); Lookups++; return Task.FromResult(Success); }
    }
}
