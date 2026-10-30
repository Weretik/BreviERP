using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Contracts.Inputs;
using Crm.Application.Quotes.CreateQuote;
using Crm.Application.Quotes.Introductions;
using Crm.Application.Quotes.Mapping;
using Crm.Application.Quotes.Pricing;
using Crm.Infrastructure.Persistence.Introductions;
using Crm.Infrastructure.Persistence.Quotes.Pricing;
using Crm.Infrastructure.Persistence.Quotes.Writing;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace IntegrationTests.Crm.Introductions;

[NonParallelizable]
public sealed class IntroductionQuoteReplayTests : CrmDatabaseFixture
{
    private static readonly CrmActor Owner = new(17, false);
    private static IntroductionQuoteApplicationIntent Intent() => new(Guid.NewGuid(), null, null, null, "Synthetic");

    private async Task<global::Crm.Application.Quotes.Contracts.Responses.QuoteDetail> Create(Guid requestId, IntroductionQuoteApplicationIntent intent)
    {
        await using var db = Context();
        var builder = new QuoteVersionBuilder(new QuotePriceBasisResolver(new QuotePriceBasisStore(db), Mock.Of<IQuoteCatalogPriceReader>()));
        var result = await new CreateQuoteCommandHandler(new QuoteMutationStore(db), builder)
            .Handle(new(null, null, "Synthetic", new QuoteDraftContent([]), requestId, Owner, null, intent), default);
        Assert.That(result.IsSuccess, Is.True, string.Join(",", result.Errors));
        return result.Value;
    }

    [Test]
    public async Task Successful_apply_replays_without_template_or_catalog_even_after_quote_archive()
    {
        var id = Guid.NewGuid(); var intent = Intent(); var saved = await Create(id, intent);
        await using var db = Context();
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE crm.\"Quotes\" SET \"Archived\" = TRUE WHERE \"PublicId\" = {saved.PublicId}");
        var creator = new IntroductionQuoteCreator(null!, null!, null!, new IntroductionQuoteReplayReader(db));
        var replay = await creator.CreateAsync(Owner, id, intent.TemplatePublicId, null, null, null, intent.RecipientName, default);
        Assert.That(replay.IsSuccess, Is.True);
        Assert.That(replay.Value.PublicId, Is.EqualTo(saved.PublicId));
        Assert.That(await db.Quotes.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task Changed_payload_conflicts_and_foreign_actor_cannot_replay()
    {
        var id = Guid.NewGuid(); var intent = Intent(); await Create(id, intent);
        await using var db = Context(); var reader = new IntroductionQuoteReplayReader(db);
        var changed = await reader.ReadAsync(id, Owner, intent with { RecipientName = "Changed" }, default);
        Assert.That(changed!.Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(await reader.ReadAsync(id, new CrmActor(18, false), intent, default), Is.Null);
    }

    [Test]
    public async Task Former_owner_replay_is_denied_after_transfer()
    {
        var id = Guid.NewGuid(); var intent = Intent(); var saved = await Create(id, intent);
        await using var db = Context();
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE crm.\"Quotes\" SET \"OwnerId\" = 18 WHERE \"PublicId\" = {saved.PublicId}");
        var replay = await new IntroductionQuoteReplayReader(db).ReadAsync(id, Owner, intent, default);
        Assert.That(replay!.Status, Is.EqualTo(ResultStatus.NotFound));
        var changed = await new IntroductionQuoteReplayReader(db).ReadAsync(id, Owner,
            intent with { RecipientName = "Changed" }, default);
        Assert.That(changed!.Status, Is.EqualTo(ResultStatus.NotFound));
    }

    [Test]
    public async Task Concurrent_same_intent_commits_one_quote_and_one_receipt()
    {
        var id = Guid.NewGuid(); var intent = Intent();
        var results = await Task.WhenAll(Create(id, intent), Create(id, intent));
        Assert.That(results[0].PublicId, Is.EqualTo(results[1].PublicId));
        await using var db = Context();
        Assert.That(await db.Quotes.CountAsync(), Is.EqualTo(1));
        Assert.That(await db.QuoteMutationRequests.CountAsync(), Is.EqualTo(1));
    }
}
