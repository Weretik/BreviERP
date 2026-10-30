using System.Text.Json;
using Ardalis.Result;
using Crm.Application.Abstractions.Documents;
using Crm.Application.Customers.Access;
using Crm.Application.Documents.Contracts;
using Crm.Application.Quotes.PrepareQuote;
using Crm.Application.Quotes.Pricing;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Items;
using Crm.Domain.Quotes.Pricing;
using Crm.Domain.Quotes.Variants;
using Crm.Domain.Quotes.Versions;
using Crm.Infrastructure.Persistence.Documents;
using Crm.Infrastructure.Persistence;
using Crm.Infrastructure.Persistence.Quotes.Writing;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace IntegrationTests.Crm.Quotes;

[NonParallelizable]
public sealed class QuoteReadyPersistenceTests : CrmDatabaseFixture
{
    private static readonly CrmActor Admin = new(7, true);
    private sealed class NoLogo : IQuoteBrandAssetResolver
    {
        public Task<PublicDocumentImage?> ResolveAsync(string assetKey, CancellationToken ct)
            => Task.FromResult<PublicDocumentImage?>(null);
    }

    [Test]
    public async Task Ready_requires_active_settings_then_pins_one_issue_and_replays_without_a_second_number()
    {
        await using var db = Context();
        var (quote, version) = await CreateQuoteAndVersionAsync(db);
        var handler = new PrepareQuoteCommandHandler(new QuoteMutationStore(db),
            new DocumentNumberAllocator(db), new NoLogo());
        var command = new PrepareQuoteCommand(quote.PublicId, version.PublicId, 1, Guid.NewGuid(), Admin);
        var missing = await handler.Handle(command, default);
        Assert.That(missing.Status, Is.EqualTo(ResultStatus.Invalid));
        Assert.That(await db.QuoteIssues.CountAsync(), Is.Zero);
        Assert.That(await db.DocumentNumberSequences.CountAsync(), Is.Zero);

        var settings = new IssuerSettingsStore(db);
        var created = await settings.CreateVersionAsync(Guid.NewGuid(),
            new IssuerSettingsInput("Synthetic Seller", "12345678", "1234567890", "UA123456789012345678901234567",
                "Kyiv", "+380501234567", "seller@example.com", "https://example.com", null, null, null),
            new DocumentTemplateInput("standard", "Offer", "Усі ціни вказані в грн з ПДВ.", "Intro",
                "CTA", "Text", "Contact"), Admin, default);
        Assert.That(created.IsSuccess, Is.True, string.Join(",", created.Errors));
        var activated = await settings.ActivateAsync(Guid.NewGuid(), 1, created.Value.IssuerVersionId,
            created.Value.TemplateVersionId, Admin, default);
        Assert.That(activated.IsSuccess, Is.True);

        var ready = await handler.Handle(command, default);
        Assert.That(ready.IsSuccess, Is.True, string.Join(",", ready.Errors));
        Assert.That(ready.Value.State, Is.EqualTo("ready"));
        Assert.That(ready.Value.Number, Does.Match(@"^10/\d{1,2}/\d{4}$"));
        var replay = await handler.Handle(command, default);
        Assert.That(replay.IsSuccess, Is.True);
        Assert.That(replay.Value.Number, Is.EqualTo(ready.Value.Number));
        Assert.That(await db.QuoteIssues.CountAsync(), Is.EqualTo(1));
        var issue = await db.QuoteIssues.SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(issue.QuoteVersionId, Is.EqualTo(version.Id));
            Assert.That(issue.IssuerVersionId, Is.EqualTo(db.IssuerSettingsVersions.Single().Id));
            Assert.That(issue.TemplateVersionId, Is.EqualTo(db.DocumentTemplateVersions.Single().Id));
            Assert.That(issue.Number, Is.EqualTo(ready.Value.Number));
        });
        var rewrite = Assert.ThrowsAsync<PostgresException>(async () =>
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE crm.\"QuoteIssues\" SET \"Number\" = 'changed' WHERE \"Id\" = {issue.Id}"));
        Assert.That(rewrite!.SqlState, Is.EqualTo("55000"));
    }

    [Test]
    public async Task Foreign_archived_and_wrong_current_version_cannot_prepare_ready()
    {
        await using var db = Context();
        var (quote, version) = await CreateQuoteAndVersionAsync(db);
        var handler = new PrepareQuoteCommandHandler(new QuoteMutationStore(db),
            new DocumentNumberAllocator(db), new NoLogo());
        var foreign = await handler.Handle(new PrepareQuoteCommand(quote.PublicId, version.PublicId, 1,
            Guid.NewGuid(), new CrmActor(8, false)), default);
        Assert.That(foreign.Status, Is.EqualTo(ResultStatus.NotFound));

        var tracked = await db.Quotes.SingleAsync(x => x.PublicId == quote.PublicId);
        tracked.SetArchived(1, true, Admin.UserId, true, "Synthetic archive");
        await db.SaveChangesAsync();
        var archived = await handler.Handle(new PrepareQuoteCommand(quote.PublicId, version.PublicId, 2,
            Guid.NewGuid(), Admin), default);
        Assert.That(archived.Status, Is.EqualTo(ResultStatus.Conflict));

        tracked = await db.Quotes.SingleAsync(x => x.PublicId == quote.PublicId);
        tracked.SetArchived(2, false, Admin.UserId, true, "Synthetic restore");
        await db.SaveChangesAsync();
        var wrongVersion = await handler.Handle(new PrepareQuoteCommand(quote.PublicId, Guid.NewGuid(), 3,
            Guid.NewGuid(), Admin), default);
        Assert.That(wrongVersion.Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(await db.QuoteIssues.CountAsync(), Is.Zero);
        Assert.That(await db.DocumentNumberSequences.CountAsync(), Is.Zero);
    }

    [Test]
    public async Task Selected_but_unprovisioned_logo_blocks_ready_without_number()
    {
        await using var db = Context();
        var (quote, version) = await CreateQuoteAndVersionAsync(db);
        var settings = new IssuerSettingsStore(db);
        var created = await settings.CreateVersionAsync(Guid.NewGuid(),
            new IssuerSettingsInput("Synthetic Seller", "12345678", "1234567890", "UA123456789012345678901234567",
                "Kyiv", "+380501234567", "seller@example.com", "https://example.com", null, null, "brevi-wordmark"),
            new DocumentTemplateInput("standard", "Offer", "Усі ціни вказані в грн з ПДВ.", "Intro",
                "CTA", "Text", "Contact"), Admin, default);
        Assert.That(created.IsSuccess, Is.True);
        Assert.That((await settings.ActivateAsync(Guid.NewGuid(), 1, created.Value.IssuerVersionId,
            created.Value.TemplateVersionId, Admin, default)).IsSuccess, Is.True);
        var ready = await new PrepareQuoteCommandHandler(new QuoteMutationStore(db),
            new DocumentNumberAllocator(db), new NoLogo()).Handle(new PrepareQuoteCommand(
                quote.PublicId, version.PublicId, 1, Guid.NewGuid(), Admin), default);
        Assert.That(ready.Status, Is.EqualTo(ResultStatus.Invalid));
        Assert.That(ready.ValidationErrors.Select(x => x.ErrorMessage), Does.Contain("logo_asset_unavailable"));
        Assert.That(await db.QuoteIssues.CountAsync(), Is.Zero);
        Assert.That(await db.DocumentNumberSequences.CountAsync(), Is.Zero);
    }

    [Test]
    public async Task Oversized_public_projection_cannot_make_quote_ready_or_allocate_number()
    {
        await using var db = Context();
        var (quote, version) = await CreateQuoteAndVersionAsync(db, 5, 30, true);
        var settings = new IssuerSettingsStore(db);
        var created = await settings.CreateVersionAsync(Guid.NewGuid(),
            new IssuerSettingsInput("Synthetic Seller", "12345678", "1234567890", "UA123456789012345678901234567",
                "Kyiv", "+380501234567", "seller@example.com", "https://example.com", null, null, null),
            new DocumentTemplateInput("standard", "Offer", "Усі ціни вказані в грн з ПДВ.", "Intro",
                "CTA", "Text", "Contact"), Admin, default);
        Assert.That(created.IsSuccess, Is.True);
        Assert.That((await settings.ActivateAsync(Guid.NewGuid(), 1, created.Value.IssuerVersionId,
            created.Value.TemplateVersionId, Admin, default)).IsSuccess, Is.True);

        var result = await new PrepareQuoteCommandHandler(new QuoteMutationStore(db),
            new DocumentNumberAllocator(db), new NoLogo()).Handle(new PrepareQuoteCommand(
                quote.PublicId, version.PublicId, 1, Guid.NewGuid(), Admin), default);
        Assert.That(result.Status, Is.EqualTo(ResultStatus.Invalid));
        Assert.That(result.ValidationErrors.Select(x => x.ErrorMessage), Does.Contain("public_document_too_large"));
        Assert.That(await db.QuoteIssues.CountAsync(), Is.Zero);
        Assert.That(await db.DocumentNumberSequences.CountAsync(), Is.Zero);
        Assert.That((await db.Quotes.AsNoTracking().SingleAsync(x => x.PublicId == quote.PublicId)).State,
            Is.EqualTo("draft"));
    }

    private static async Task<(Quote Quote, QuoteVersion Version)> CreateQuoteAndVersionAsync(
        CrmDbContext db, int itemCount = 1, int variantsPerItem = 1, bool allTiersVisible = false)
    {
        var quote = Quote.Create(null, null, "Synthetic buyer", new CrmUserId(Admin.UserId));
        db.Quotes.Add(quote);
        await db.SaveChangesAsync();
        var basis = new QuotePricePreview(Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(5),
            new("oneOffPpe", null, null, null, new(100, 90, 80, "Synthetic")), null,
            100, 90, 80, [], "UAH", "manual_no_extra_vat");
        var basisJson = JsonSerializer.Serialize(basis, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var items = Enumerable.Range(0, itemCount).Select(itemOrder => QuoteItem.Create(
            Guid.NewGuid(), itemOrder, "Offer", null, null, "Private", true,
            Enumerable.Range(0, variantsPerItem).Select(variantOrder => QuoteVariant.Create(
                Guid.NewGuid(), variantOrder, "Variant", "oneOffPpe", null, null, true,
                basis.BasisToken, basisJson,
                [QuoteTier.Create("retail", 100, true),
                    QuoteTier.Create("wholesale", 90, allTiersVisible),
                    QuoteTier.Create("largeWholesale", 80, allTiersVisible)])).ToArray())).ToArray();
        var version = QuoteVersion.Create(quote.Id, 1, null, "created", "Synthetic buyer", null, null,
            Admin.UserId, items);
        db.QuoteVersions.Add(version);
        await db.SaveChangesAsync();
        quote.SetInitialVersion(version.Id);
        await db.SaveChangesAsync();
        return (quote, version);
    }
}
