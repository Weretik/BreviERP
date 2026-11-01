using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Delivery.Outbox;
using Crm.Application.Delivery.ResolveOutcome;
using Crm.Application.Delivery.Provider;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Delivery;
using Crm.Domain.Quotes.Versions;
using Crm.Infrastructure.Persistence.Delivery;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;

namespace IntegrationTests.Crm.Delivery;

public sealed partial class QuoteDeliveryOutboxPersistenceTests
{
    private static ResolveQuoteDeliveryCommand Manual(QuoteDeliveryDetail delivery, string outcome = "sent")
        => new(delivery.QuoteId, delivery.DeliveryId, Guid.NewGuid(), 1, outcome, "Перевірено лист та адресата вручну.");

    private async Task<QuoteDeliveryDetail> UnknownAsync(QuoteDeliveryStore store)
    {
        var request = await SeedAsync();
        var delivery = (await store.EnqueueAsync(request, Owner, Settings, Readiness, default)).Value;
        var claim = await store.ClaimAsync(Settings, _ => true, default);
        await store.CompleteAsync(claim!.DeliveryId, claim.LeaseToken,
            new(QuoteDeliveryProviderOutcome.Unknown, null, "gmail_send_outcome_unknown"), default);
        return delivery;
    }

    [Test]
    public async Task Manual_success_records_actual_actor_replays_and_protects_immutable_evidence()
    {
        await using var db = Context();
        var store = new QuoteDeliveryStore(db);
        var delivery = await UnknownAsync(store);
        var command = Manual(delivery);
        var resolver = new CrmActor(18, true);
        Assert.That((await store.ResolveAsync(command, new CrmActor(18, false), default)).Status, Is.EqualTo(ResultStatus.NotFound));
        var result = await store.ResolveAsync(command, resolver, default);
        Assert.That(result.IsSuccess, Is.True, string.Join(',', result.Errors));
        Assert.That(result.Value.ManualResolution!.ActorId, Is.EqualTo(18));
        Assert.That(result.Value.ManualResolution.Outcome, Is.EqualTo("sent"));
        Assert.That((await store.ResolveAsync(command with { EvidenceNote = " " + command.EvidenceNote + " " }, resolver, default)).IsSuccess, Is.True);
        Assert.That(result.Value.CanResolveManually, Is.False);
        Assert.That((await db.Quotes.SingleAsync()).State, Is.EqualTo("sent"));
        Assert.That((await db.Set<QuoteDeliveryEvent>().SingleAsync(x => x.Kind == "manual_succeeded")).ActorId, Is.EqualTo(18));
        Assert.That((await store.ResolveAsync(command, resolver, default)).Value.ManualResolution, Is.EqualTo(result.Value.ManualResolution));
        Assert.That((await store.ResolveAsync(command with { EvidenceNote = "Changed evidence" }, resolver, default)).Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(await db.Set<QuoteDeliveryResolution>().CountAsync(), Is.EqualTo(1));
        var error = await Assert.ThrowsAsync<PostgresException>(async () =>
            await db.Database.ExecuteSqlRawAsync("UPDATE crm.\"QuoteDeliveryResolutions\" SET \"EvidenceNote\"='changed'"));
        Assert.That(error!.SqlState, Is.EqualTo("P0001"));
        var deletion = await Assert.ThrowsAsync<PostgresException>(async () =>
            await db.Database.ExecuteSqlRawAsync("DELETE FROM crm.\"QuoteDeliveryResolutions\""));
        Assert.That(deletion!.SqlState, Is.EqualTo("P0001"));
    }

    [Test]
    public async Task Manual_success_for_old_version_does_not_mark_newer_draft_sent()
    {
        await using var db = Context();
        var store = new QuoteDeliveryStore(db);
        var delivery = await UnknownAsync(store);
        var quote = await db.Quotes.SingleAsync();
        var next = QuoteVersion.Create(quote.Id, 2, quote.CurrentVersionId, "edited", "New recipient", null, null, Owner.UserId, []);
        db.QuoteVersions.Add(next); await db.SaveChangesAsync();
        quote.AppendVersion(quote.Revision, next.Id, Owner.UserId); await db.SaveChangesAsync();
        Assert.That((await store.ResolveAsync(Manual(delivery), Owner, default)).IsSuccess, Is.True);
        Assert.That((await db.Quotes.SingleAsync()).State, Is.EqualTo("draft"));
    }

    [Test]
    public async Task Manual_failure_allows_only_a_new_explicit_enqueue_without_automatic_dispatch()
    {
        var request = await SeedAsync();
        await using var db = Context();
        var store = new QuoteDeliveryStore(db);
        var delivery = (await store.EnqueueAsync(request, Owner, Settings, Readiness, default)).Value;
        var claim = await store.ClaimAsync(Settings, _ => true, default);
        await store.CompleteAsync(claim!.DeliveryId, claim.LeaseToken,
            new(QuoteDeliveryProviderOutcome.Unknown, null, "gmail_send_outcome_unknown"), default);
        Assert.That((await store.ResolveAsync(Manual(delivery) with { RequestId = request.RequestId }, Owner, default)).Status, Is.EqualTo(ResultStatus.Conflict));
        var result = await store.ResolveAsync(Manual(delivery, "not_sent"), Owner, default);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Status, Is.EqualTo("failed"));
        Assert.That((await db.Quotes.SingleAsync()).State, Is.EqualTo("ready"));
        var next = await store.EnqueueAsync(request with { RequestId = Guid.NewGuid() }, Owner, Settings, Readiness, default);
        Assert.That(next.IsSuccess, Is.True);
        Assert.That(await db.Set<QuoteDelivery>().CountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task Manual_active_attempt_is_rejected_and_expired_attempt_can_be_resolved_without_provider()
    {
        var request = await SeedAsync(); var clock = new Clock();
        await using var db = Context(); var store = new QuoteDeliveryStore(db, clock);
        var delivery = (await store.EnqueueAsync(request, Owner, Settings, Readiness, default)).Value;
        await store.ClaimAsync(Settings, _ => true, default);
        var command = Manual(delivery);
        Assert.That((await store.ResolveAsync(command, Owner, default)).Status, Is.EqualTo(ResultStatus.Conflict));
        clock.Advance(TimeSpan.FromSeconds(121));
        Assert.That((await store.GetAsync(request.QuoteId, delivery.DeliveryId, Owner, default)).Value.CanResolveManually, Is.True);
        Assert.That((await store.ResolveAsync(command with { ExpectedAttempt = 2 }, Owner, default)).Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That((await store.ResolveAsync(command, Owner, default)).IsSuccess, Is.True);
    }

    [Test]
    public async Task Manual_replay_rechecks_current_ownership_before_changed_intent()
    {
        await using var db = Context(); var store = new QuoteDeliveryStore(db);
        var delivery = await UnknownAsync(store); var command = Manual(delivery);
        Assert.That((await store.ResolveAsync(command, Owner, default)).IsSuccess, Is.True);
        var quote = await db.Quotes.SingleAsync();
        quote.Transfer(quote.Revision, new CrmUserId(Stranger.UserId), Owner.UserId, false, "Transfer");
        await db.SaveChangesAsync();
        Assert.That((await store.ResolveAsync(command with { EvidenceNote = "Changed after transfer" }, Owner, default)).Status, Is.EqualTo(ResultStatus.NotFound));
    }

    [Test]
    public async Task Manual_parallel_conflicting_resolutions_commit_exactly_one_result()
    {
        await using var db = Context(); var store = new QuoteDeliveryStore(db);
        var delivery = await UnknownAsync(store);
        await using var otherDb = Context();
        var outcomes = await Task.WhenAll(store.ResolveAsync(Manual(delivery), Owner, default),
            new QuoteDeliveryStore(otherDb).ResolveAsync(Manual(delivery, "not_sent"), Owner, default));
        Assert.That(outcomes.Count(x => x.IsSuccess), Is.EqualTo(1));
        Assert.That(outcomes.Count(x => x.Status == ResultStatus.Conflict), Is.EqualTo(1));
        Assert.That(await db.Set<QuoteDeliveryResolution>().CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task Manual_upgrade_preserves_existing_delivery_and_reapplies_without_data_loss()
    {
        await using var db = Context(); var store = new QuoteDeliveryStore(db);
        var delivery = await UnknownAsync(store); var migrator = db.GetService<IMigrator>();
        await migrator.MigrateAsync("20261030163554_AddQuoteDeliveryOutbox");
        try
        {
            await migrator.MigrateAsync(); await migrator.MigrateAsync();
            var result = await store.GetAsync(delivery.QuoteId, delivery.DeliveryId, Owner, default);
            Assert.That(result.Value.Status, Is.EqualTo("unknown"));
            Assert.That(result.Value.ManualResolution, Is.Null);
            Assert.That((await store.ResolveAsync(Manual(delivery), Owner, default)).IsSuccess, Is.True);
        }
        finally { await migrator.MigrateAsync(); }
    }
}
