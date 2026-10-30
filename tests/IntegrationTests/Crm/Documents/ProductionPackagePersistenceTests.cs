using System.IO.Compression;
using System.Text.Json;
using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Production.Export;
using Crm.Application.CustomSewing.Production.Persistence;
using Crm.Application.CustomSewing.Production.Projection;
using Crm.Application.Quotes.Pricing;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.CustomSewing.Composition;
using Crm.Domain.CustomSewing.Drafts;
using Crm.Domain.CustomSewing.Pricing;
using Crm.Domain.CustomSewing.Versions;
using Crm.Domain.Documents.Issuer;
using Crm.Domain.Documents.Templates;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Items;
using Crm.Domain.Quotes.Issues;
using Crm.Domain.Quotes.Pricing;
using Crm.Domain.Quotes.Variants;
using Crm.Domain.Quotes.Versions;
using Crm.Infrastructure.Persistence;
using Crm.Infrastructure.Persistence.Documents.Production;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace IntegrationTests.Crm.Documents;

[NonParallelizable]
public sealed class ProductionPackagePersistenceTests : CrmDatabaseFixture
{
    private static readonly CrmActor Owner = new(17, false);
    private static readonly CrmActor Stranger = new(18, false);
    private static readonly CrmActor Admin = new(7, true);
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    [Test]
    public async Task Export_replay_and_historical_read_reuse_one_pinned_private_artifact()
    {
        var fixture = await SeedAsync();
        await using var db = Context();
        var store = new ProductionPackageStore(db);
        var request = Request(fixture);
        var builds = 0;
        Task<Result<byte[]>> Build(ProductionExportSource source, CancellationToken _) { builds++; return Archive(source, request.VariantIds); }
        var first = await store.ExportAsync(request, Owner, Build, default);
        Assert.That(first.IsSuccess, Is.True, string.Join(',', first.ValidationErrors.Select(x => x.ErrorMessage)));
        var replay = await store.ExportAsync(request, Owner, Build, default);
        var nextRequest = await store.ExportAsync(request with { RequestId = Guid.NewGuid() }, Owner,
            (_, _) => throw new AssertionException("Cached package must not read current inputs or build again."), default);
        Assert.Multiple(() =>
        {
            Assert.That(replay.Value.PackageId, Is.EqualTo(first.Value.PackageId));
            Assert.That(nextRequest.Value.Sha256, Is.EqualTo(first.Value.Sha256));
            Assert.That(builds, Is.EqualTo(1));
            Assert.That(first.Value.ContentType, Is.EqualTo("application/zip"));
        });
        var draft = await db.CustomDrafts.SingleAsync();
        draft.Revise(draft.Revision, draft.Configuration with { Name = "CURRENT_MASTER_CHANGED", Fabrics = [new CustomMaterial(999, 99m, "шт")] });
        await db.SaveChangesAsync();
        var read = await store.ReadAsync(fixture.QuoteId, first.Value.PackageId, Owner, default);
        Assert.That(read.IsSuccess, Is.True);
        using var archive = new ZipArchive(new MemoryStream(read.Value.Bytes), ZipArchiveMode.Read);
        using var text = new StreamReader(archive.GetEntry("manifest.json")!.Open());
        var manifest = await text.ReadToEndAsync();
        using var parsed = JsonDocument.Parse(manifest);
        Assert.That(parsed.RootElement.GetProperty("options")[0].GetProperty("name").GetString(), Is.EqualTo("Куртка"));
        Assert.That(manifest, Does.Not.Contain("CURRENT_MASTER_CHANGED"));
        Assert.That(manifest, Does.Not.Contain("PRIVATE_CUSTOM_NOTE"));
        Assert.That(await db.Set<ProductionPackageArtifact>().CountAsync(), Is.EqualTo(1));
        Assert.That(await db.Set<ProductionPackageMutationRecord>().CountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task Changed_payload_conflicts_and_current_quote_access_precedes_replay_hash()
    {
        var fixture = await SeedAsync();
        await using var db = Context();
        var store = new ProductionPackageStore(db);
        var request = Request(fixture);
        var first = await store.ExportAsync(request, Owner, (source, _) => Archive(source, request.VariantIds), default);
        Assert.That(first.IsSuccess, Is.True);
        Assert.That((await store.ExportAsync(request with { VariantIds = [Guid.NewGuid()] }, Owner,
            (_, _) => throw new AssertionException("Builder must not run."), default)).Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That((await store.ReadAsync(fixture.QuoteId, first.Value.PackageId, Stranger, default)).Status,
            Is.EqualTo(ResultStatus.NotFound));
        Assert.That((await store.ReadAsync(fixture.QuoteId, first.Value.PackageId, Admin, default)).IsSuccess, Is.True);
        var quote = await db.Quotes.SingleAsync();
        quote.Transfer(quote.Revision, new CrmUserId(Stranger.UserId), Owner.UserId, false, "Owner changed");
        await db.SaveChangesAsync();
        Assert.That((await store.ExportAsync(request with { VariantIds = [Guid.NewGuid()] }, Owner,
            (_, _) => throw new AssertionException("Builder must not run."), default)).Status, Is.EqualTo(ResultStatus.NotFound));
        Assert.That((await store.GetAsync(fixture.QuoteId, first.Value.PackageId, Owner, default)).Status,
            Is.EqualTo(ResultStatus.NotFound));
        Assert.That((await store.GetAsync(fixture.QuoteId, first.Value.PackageId, Stranger, default)).IsSuccess, Is.True);
    }

    [Test]
    public async Task Unissued_and_mismatched_custom_snapshot_never_create_artifact_or_receipt()
    {
        var unissued = await SeedAsync(issued: false);
        await using var db = Context();
        var store = new ProductionPackageStore(db);
        var noReady = await store.ExportAsync(Request(unissued), Owner, (_, _) => throw new AssertionException("Builder must not run."), default);
        Assert.That(noReady.Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(noReady.Errors, Does.Contain("production_issued_version_required"));
        var mismatch = await SeedAsync(tamperSnapshot: true);
        var bad = await store.ExportAsync(Request(mismatch), Owner, (_, _) => throw new AssertionException("Builder must not run."), default);
        Assert.That(bad.Status, Is.EqualTo(ResultStatus.Invalid));
        Assert.That(bad.ValidationErrors.Select(x => x.ErrorMessage), Does.Contain("production_quote_custom_snapshot_mismatch"));
        Assert.That(await db.Set<ProductionPackageArtifact>().CountAsync(), Is.Zero);
        Assert.That(await db.Set<ProductionPackageMutationRecord>().CountAsync(), Is.Zero);
    }

    [Test]
    public async Task Concurrent_requests_for_same_version_and_selection_build_exactly_one_artifact()
    {
        var fixture = await SeedAsync();
        var request = Request(fixture);
        var builds = 0;
        async Task<Result<byte[]>> Build(ProductionExportSource source, CancellationToken _) {
            Interlocked.Increment(ref builds);
            await Task.Delay(60);
            return await Archive(source, request.VariantIds);
        }
        async Task<Guid> Export(Guid requestId) {
            await using var ownDb = Context();
            var result = await new ProductionPackageStore(ownDb).ExportAsync(request with { RequestId = requestId }, Owner, Build, default);
            Assert.That(result.IsSuccess, Is.True, string.Join(',', result.Errors));
            return result.Value.PackageId;
        }
        var ids = await Task.WhenAll(Export(Guid.NewGuid()), Export(Guid.NewGuid()));
        Assert.That(ids.Distinct().Count(), Is.EqualTo(1));
        Assert.That(builds, Is.EqualTo(1));
        await using var db = Context();
        Assert.That(await db.Set<ProductionPackageArtifact>().CountAsync(), Is.EqualTo(1));
        Assert.That(await db.Set<ProductionPackageMutationRecord>().CountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task Production_artifact_and_receipt_rows_are_append_only_in_postgresql()
    {
        var fixture = await SeedAsync();
        await using var db = Context();
        var request = Request(fixture);
        var first = await new ProductionPackageStore(db).ExportAsync(request, Owner,
            (source, _) => Archive(source, request.VariantIds), default);
        Assert.That(first.IsSuccess, Is.True);
        var artifactUpdate = Assert.ThrowsAsync<PostgresException>(async () =>
            await db.Database.ExecuteSqlRawAsync("UPDATE crm.\"ProductionPackageArtifacts\" SET \"SchemaVersion\"=1"));
        Assert.That(artifactUpdate!.SqlState, Is.EqualTo(PostgresErrorCodes.RaiseException));
        var receiptDelete = Assert.ThrowsAsync<PostgresException>(async () =>
            await db.Database.ExecuteSqlRawAsync("DELETE FROM crm.\"ProductionPackageRequests\""));
        Assert.That(receiptDelete!.SqlState, Is.EqualTo(PostgresErrorCodes.RaiseException));
    }

    [Test]
    public async Task Archived_quote_blocks_new_export_requests_but_preserves_replay_and_historical_reads()
    {
        var fixture = await SeedAsync();
        await using var db = Context();
        var store = new ProductionPackageStore(db);
        var request = Request(fixture);
        var first = await store.ExportAsync(request, Owner, (source, _) => Archive(source, request.VariantIds), default);
        Assert.That(first.IsSuccess, Is.True);
        var quote = await db.Quotes.SingleAsync();
        quote.SetArchived(quote.Revision, true, Owner.UserId, false, "Archive after production export");
        await db.SaveChangesAsync();
        Task<Result<byte[]>> MustNotBuild(ProductionExportSource _, CancellationToken __) =>
            throw new AssertionException("Archived quote must not build a new artifact.");
        var blocked = await store.ExportAsync(request with { RequestId = Guid.NewGuid() }, Owner, MustNotBuild, default);
        Assert.That(blocked.Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(blocked.Errors, Does.Contain("production_quote_archived"));
        var replay = await store.ExportAsync(request, Owner, MustNotBuild, default);
        Assert.That(replay.IsSuccess, Is.True);
        Assert.That(replay.Value.PackageId, Is.EqualTo(first.Value.PackageId));
        Assert.That((await store.ReadAsync(fixture.QuoteId, first.Value.PackageId, Owner, default)).IsSuccess, Is.True);
        Assert.That(await db.Set<ProductionPackageArtifact>().CountAsync(), Is.EqualTo(1));
        Assert.That(await db.Set<ProductionPackageMutationRecord>().CountAsync(), Is.EqualTo(1));
        var neverExported = await SeedAsync(issued: false);
        var anotherQuote = await db.Quotes.SingleAsync(x => x.PublicId == neverExported.QuoteId);
        anotherQuote.SetArchived(anotherQuote.Revision, true, Owner.UserId, false, "Archive before production export");
        await db.SaveChangesAsync();
        var noArtifact = await store.ExportAsync(Request(neverExported), Owner, MustNotBuild, default);
        Assert.That(noArtifact.Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(noArtifact.Errors, Does.Contain("production_quote_archived"));
        Assert.That(await db.Set<ProductionPackageArtifact>().CountAsync(), Is.EqualTo(1));
        Assert.That(await db.Set<ProductionPackageMutationRecord>().CountAsync(), Is.EqualTo(1));
    }
    private static ProductionPackageRequest Request(Fixture fixture) =>
        new(fixture.QuoteId, fixture.VersionId, Guid.NewGuid(), [fixture.VariantId]);

    private static Task<Result<byte[]>> Archive(ProductionExportSource source, IReadOnlyList<Guid> selected)
    {
        var result = ProductionPackageProjectionBuilder.Build(source.QuotePublicId, source.QuoteNumber,
            source.Version, selected, source.CustomVersions);
        Assert.That(result.IsSuccess, Is.True, string.Join(',', result.ValidationErrors.Select(x => x.ErrorMessage)));
        using var memory = new MemoryStream();
        using (var zip = new ZipArchive(memory, ZipArchiveMode.Create, leaveOpen: true))
        using (var writer = new StreamWriter(zip.CreateEntry("manifest.json").Open()))
            writer.Write(JsonSerializer.Serialize(result.Value, Json));
        return Task.FromResult(Result.Success(memory.ToArray()));
    }

    private async Task<Fixture> SeedAsync(bool issued = true, bool tamperSnapshot = false)
    {
        await using var db = Context();
        var owner = new CrmUserId(Owner.UserId);
        var config = new CustomConfiguration("Куртка", "jacket", "summer", "PRIVATE_DESCRIPTION", "PRIVATE_CONTACT",
            "PRIVATE_CUSTOM_NOTE", [new CustomMaterial(11, 2.5m, "м")], [], [new CustomOperation(3, 2m, null)], [], 5m, 6m);
        var basis = new CustomPriceBasis(Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMinutes(15), "v1", 41,
            [new CustomReferenceBasis("Operation:3", 91, 12m, "Minutes", "SECRET_SUPPLIER_CODE")],
            new CustomCostBreakdown(10m, 20m, 24m, 3m, 72m, 4m, 5m, 888m, 7m, 888m, 5m, 6m, 0m, 0m),
            100m, 150m, 140m, 130m, []);
        var draft = CustomSewingDraft.Create(owner, null, null, config);
        db.CustomDrafts.Add(draft);
        await db.SaveChangesAsync();
        var custom = CustomSewingVersion.Create(draft.Id, 1, null, config, basis, owner);
        db.CustomVersions.Add(custom);
        await db.SaveChangesAsync();
        draft.SetVersion(draft.Revision, custom);
        var snapshot = new QuoteCustomVersionSnapshot(draft.PublicId, custom.PublicId, custom.Number, custom.Configuration, custom.Basis);
        if (tamperSnapshot) snapshot = snapshot with { Configuration = snapshot.Configuration with { Name = "WRONG_VERSION_CONTENT" } };
        var preview = new QuotePricePreview(Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(15),
            new QuotePriceIntent("customSewing", null, null, null, null, custom.PublicId), null, 150m, 140m, 130m,
            [], "UAH", "custom_no_extra_vat", snapshot);
        var variant = QuoteVariant.Create(Guid.NewGuid(), 0, "Варіант", "customSewing", null, null, true,
            preview.BasisToken, JsonSerializer.Serialize(preview, Json), [QuoteTier.Create("retail", 150m, true),
                QuoteTier.Create("wholesale", 140m, true), QuoteTier.Create("largeWholesale", 130m, true)], custom.Id);
        var quote = Quote.Create(null, null, "PRIVATE_RECIPIENT", owner);
        db.Quotes.Add(quote);
        await db.SaveChangesAsync();
        var version = QuoteVersion.Create(quote.Id, 1, null, "created", quote.RecipientName, null, null, owner.Value,
            [QuoteItem.Create(Guid.NewGuid(), 0, "PRIVATE_LABEL", null, null, "PRIVATE_NOTE", true, [variant])]);
        db.QuoteVersions.Add(version);
        await db.SaveChangesAsync();
        quote.SetInitialVersion(version.Id);
        if (issued)
        {
            var issuer = IssuerSettingsVersion.Create(1, "Seller", "12345678", "1234567890", "UA123456789012345678901234567",
                "Kyiv", "+380501234567", "seller@example.com", "https://brevi.com.ua", null, null, null, owner);
            var template = DocumentTemplateVersion.Create(1, "standard", "КП", "Ціни в грн", "Вітаємо", "Контакт", "Напишіть", "{managerName}", owner);
            db.IssuerSettingsVersions.Add(issuer); db.DocumentTemplateVersions.Add(template);
            await db.SaveChangesAsync();
            quote.PrepareReady(quote.Revision, "10/10/2026", owner.Value, false);
            db.QuoteIssues.Add(QuoteIssue.Create(quote.Id, version.Id, issuer.Id, template.Id, quote.Number!, DateTimeOffset.UtcNow));
        }
        await db.SaveChangesAsync();
        return new Fixture(quote.PublicId, version.PublicId, variant.PublicId);
    }

    private sealed record Fixture(Guid QuoteId, Guid VersionId, Guid VariantId);
}
