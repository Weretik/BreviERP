using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.BuildIntroduction;
using Crm.Application.Quotes.Introductions;
using Crm.Application.Quotes.Pricing;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Quotes.Introductions;
using Crm.Infrastructure.Persistence.Introductions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Npgsql;

namespace IntegrationTests.Crm.Introductions;

[NonParallelizable]
public sealed class IntroductionTemplatePersistenceTests : CrmDatabaseFixture
{
    private static readonly CrmActor Owner = new(17, false);
    private static readonly CrmActor Stranger = new(18, false);
    private static readonly CrmActor Admin = new(19, true);

    [SetUp]
    public async Task ClearIntroductionRows()
    {
        await using var db = Context();
        await db.Database.ExecuteSqlRawAsync("""
            TRUNCATE crm."IntroductionTemplateMutationRequests",
                     crm."IntroductionTemplateVersions", crm."IntroductionTemplates",
                     crm."IntroductionFeaturedMappingState", crm."IntroductionFeaturedMappingVersions"
            RESTART IDENTITY CASCADE
            """);
    }

    [Test]
    public async Task Create_revision_archive_and_request_replay_preserve_owned_immutable_versions()
    {
        await using var db = Context();
        var store = new IntroductionTemplateStore(db);
        var mapping = new FeaturedModelMapping(Guid.NewGuid(), 115, 118);
        var payload = Payload(mapping);
        var requestId = Guid.NewGuid();

        var created = await store.CreateAsync(Owner, requestId, "Знайомство", payload, default);
        var replay = await store.CreateAsync(Owner, requestId, "Знайомство", payload, default);
        Assert.Multiple(() =>
        {
            Assert.That(created.IsSuccess, Is.True);
            Assert.That(replay.Value.TemplatePublicId, Is.EqualTo(created.Value.TemplatePublicId));
            Assert.That(replay.Value.VersionPublicId, Is.EqualTo(created.Value.VersionPublicId));
            Assert.That(created.Value.VersionNumber, Is.EqualTo(1));
        });
        Assert.That((await store.CreateAsync(Owner, requestId, "Different", payload, default)).Status,
            Is.EqualTo(ResultStatus.Conflict));
        Assert.That(await store.ReadAsync(Stranger, created.Value.TemplatePublicId, null, default), Is.Null);
        Assert.That(await store.ReadAsync(Admin, created.Value.TemplatePublicId, null, default), Is.Not.Null);
        Assert.That((await store.ListAsync(Stranger, false, 1, 10, default)).Value, Is.Empty);
        var ownerPage = await store.ListAsync(Owner, false, 1, 10, default);
        Assert.That(ownerPage.Value, Has.Count.EqualTo(1));
        Assert.That(ownerPage.PagedInfo.TotalRecords, Is.EqualTo(1));

        var revised = await store.ReviseAsync(Owner, Guid.NewGuid(), created.Value.TemplatePublicId,
            created.Value.Revision, "Новое имя", payload, default);
        Assert.That(revised.IsSuccess, Is.True);
        Assert.That(revised.Value.VersionNumber, Is.EqualTo(2));
        Assert.That((await store.ReviseAsync(Stranger, Guid.NewGuid(), created.Value.TemplatePublicId,
            revised.Value.Revision, "Foreign", payload, default)).Status, Is.EqualTo(ResultStatus.NotFound));
        Assert.That((await store.ReviseAsync(Owner, Guid.NewGuid(), created.Value.TemplatePublicId,
            created.Value.Revision, "Stale", payload, default)).Status, Is.EqualTo(ResultStatus.Conflict));
        var old = await store.ReadAsync(Owner, created.Value.TemplatePublicId, created.Value.VersionPublicId, default);
        Assert.That(old!.Name, Is.EqualTo("Знайомство"));
        var archiveId = Guid.NewGuid();
        var archived = await store.SetArchivedAsync(Owner, archiveId, created.Value.TemplatePublicId,
            revised.Value.Revision, true, default);
        var archiveReplay = await store.SetArchivedAsync(Owner, archiveId, created.Value.TemplatePublicId,
            revised.Value.Revision, true, default);
        Assert.Multiple(() =>
        {
            Assert.That(archived.IsSuccess, Is.True);
            Assert.That(archived.Value.Archived, Is.True);
            Assert.That(archiveReplay.Value.Revision, Is.EqualTo(archived.Value.Revision));
            Assert.That(old.Snapshot.Selections, Has.Count.EqualTo(4));
        });
        Assert.That((await store.ReviseAsync(Owner, Guid.NewGuid(), created.Value.TemplatePublicId,
            archived.Value.Revision, "After archive", payload, default)).Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(await db.IntroductionTemplateVersions.CountAsync(), Is.EqualTo(2));
        Assert.That(await db.Set<IntroductionTemplateMutationRecord>().CountAsync(), Is.EqualTo(3));
    }

    [Test]
    public async Task Active_mapping_and_template_apply_use_current_price_and_block_after_mapping_change()
    {
        await using var db = Context();
        var mappings = new IntroductionFeaturedMappingStore(db);
        var first = await mappings.ActivateAsync(IntroductionFeaturedMappingVersion.Create(new string('a', 64),
            "MOD-166", "MOD-169", 115, 118, new CrmUserId(Admin.UserId)), default);
        var firstReplay = await mappings.ActivateAsync(IntroductionFeaturedMappingVersion.Create(new string('a', 64),
            "MOD-166", "MOD-169", 115, 118, new CrmUserId(Admin.UserId)), default);
        Assert.That(firstReplay.VersionId, Is.EqualTo(first.VersionId));
        Assert.That(await db.IntroductionFeaturedMappingVersions.CountAsync(), Is.EqualTo(1));
        var store = new IntroductionTemplateStore(db);
        var created = await store.CreateAsync(Owner, Guid.NewGuid(), "Знайомство", Payload(first), default);
        var currentLarge = 120m;
        var catalog = new Mock<IIntroductionCatalogReader>();
        catalog.Setup(x => x.ReadAsync(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int productId, long articleId, CancellationToken _) =>
                new IntroductionCatalogOffer(IntroductionSeason.Summer, "made", true, true,
                    "Модель", "Тканина", "60% бавовна", "235", "https://brevi.com.ua/model",
                    new QuoteCatalogPrice("sewing", productId, articleId, Guid.NewGuid(), null, null,
                        1, 1, [], "v1", 200m, 150m, currentLarge, [])));
        var service = new IntroductionTemplateService(store, mappings, new IntroductionDraftBuilder(catalog.Object));
        var firstApply = await service.ApplyAsync(Owner, created.Value.TemplatePublicId, null, default);
        currentLarge = 140m;
        var secondApply = await service.ApplyAsync(Owner, created.Value.TemplatePublicId, null, default);
        Assert.Multiple(() =>
        {
            Assert.That(firstApply.IsSuccess, Is.True);
            Assert.That(firstApply.Value.Slots[0].LargeWholesale, Is.EqualTo(120m));
            Assert.That(secondApply.Value.Slots[0].LargeWholesale, Is.EqualTo(140m));
        });
        await mappings.ActivateAsync(IntroductionFeaturedMappingVersion.Create(new string('b', 64),
            "MOD-166", "MOD-169", 115, 118, new CrmUserId(Admin.UserId)), default);
        Assert.That((await service.ApplyAsync(Owner, created.Value.TemplatePublicId, null, default)).Status,
            Is.EqualTo(ResultStatus.Conflict));
        Assert.That((await service.ApplyAsync(Stranger, created.Value.TemplatePublicId, null, default)).Status,
            Is.EqualTo(ResultStatus.NotFound));
    }

    [Test]
    public async Task Database_rejects_rewrite_of_template_and_mapping_versions()
    {
        await using var db = Context();
        var mapping = await new IntroductionFeaturedMappingStore(db).ActivateAsync(
            IntroductionFeaturedMappingVersion.Create(new string('c', 64), "MOD-166", "MOD-169",
                115, 118, new CrmUserId(Admin.UserId)), default);
        var created = await new IntroductionTemplateStore(db).CreateAsync(Owner, Guid.NewGuid(),
            "Знайомство", Payload(mapping), default);
        var templateRewrite = Assert.ThrowsAsync<PostgresException>(async () =>
            await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE crm.\"IntroductionTemplateVersions\" SET \"Name\"='Wrong' WHERE \"PublicId\"={created.Value.VersionPublicId}"));
        Assert.That(templateRewrite!.SqlState, Is.EqualTo(PostgresErrorCodes.RaiseException));
        var mappingRewrite = Assert.ThrowsAsync<PostgresException>(async () =>
            await db.Database.ExecuteSqlRawAsync("DELETE FROM crm.\"IntroductionFeaturedMappingVersions\""));
        Assert.That(mappingRewrite!.SqlState, Is.EqualTo(PostgresErrorCodes.RaiseException));
    }

    private static IntroductionTemplatePayload Payload(FeaturedModelMapping mapping) => new(mapping,
        IntroductionLayout.Summer, Guid.NewGuid(),
        Enumerable.Range(0, 4).Select(index => new IntroductionSelection(
            new IntroductionSlot(IntroductionSeason.Summer, index),
            index == 0 ? mapping.SummerProductId : 200 + index, 20 + index)).ToArray());
}
