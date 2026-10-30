using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Documents.EmailTemplates;
using Crm.Application.Quotes.BuildIntroduction;
using Crm.Application.Quotes.Introductions;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Documents.EmailTemplates;
using Crm.Domain.Quotes.Introductions;
using Crm.Infrastructure.Persistence.Documents.EmailTemplates;
using Crm.Infrastructure.Persistence.Introductions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Npgsql;

namespace IntegrationTests.Crm.Documents.EmailTemplates;
[NonParallelizable]
public sealed class EmailTemplateRegistryPersistenceTests : CrmDatabaseFixture
{
    private static readonly CrmActor Admin = new(7, true);
    private static readonly CrmActor Manager = new(17, false);
    [SetUp]
    public async Task ClearOwnRows()
    {
        await using var db = Context();
        await db.Database.ExecuteSqlRawAsync("TRUNCATE crm.\"EmailTemplateMutations\", crm.\"EmailTemplateState\", crm.\"EmailTemplateVersions\", crm.\"IntroductionTemplateMutationRequests\", crm.\"IntroductionTemplateVersions\", crm.\"IntroductionTemplates\", crm.\"IntroductionFeaturedMappingState\", crm.\"IntroductionFeaturedMappingVersions\" RESTART IDENTITY CASCADE");
    }
    [Test]
    public async Task Admin_versions_activation_and_replay_are_persisted_without_defaults()
    {
        await using var db = Context();
        var store = new EmailTemplateStore(db);
        var service = new EmailTemplateService(store);
        Assert.That((await service.ActiveAsync(default)).Active, Is.Null);
        var request = Guid.NewGuid();
        var created = await service.CreateAsync(Admin, request, "Знайомство", "КП {quoteNumber}", "Вітаємо {recipientName}", default);
        var replay = await service.CreateAsync(Admin, request, "Знайомство", "КП {quoteNumber}", "Вітаємо {recipientName}", default);
        Assert.That(replay.Value.VersionId, Is.EqualTo(created.Value.VersionId));
        Assert.That((await service.CreateAsync(Admin, request, "Different", "КП", "Текст", default)).Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That((await service.CreateAsync(Manager, Guid.NewGuid(), "No", "КП", "Текст", default)).Status, Is.EqualTo(ResultStatus.Forbidden));
        var activation = Guid.NewGuid();
        var active = await service.ActivateAsync(Admin, activation, 1, created.Value.VersionId, default);
        Assert.That(active.Value.Revision, Is.EqualTo(2));
        Assert.That((await service.ActivateAsync(Admin, activation, 1, created.Value.VersionId, default)).Value.Revision, Is.EqualTo(2));
        Assert.That((await service.ActivateAsync(Admin, Guid.NewGuid(), 1, created.Value.VersionId, default)).Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That((await service.ActivateAsync(Manager, Guid.NewGuid(), 2, created.Value.VersionId, default)).Status, Is.EqualTo(ResultStatus.Forbidden));
        var next = await service.CreateAsync(Admin, Guid.NewGuid(), "Нова версія", "Інша тема", "Інший текст", default);
        Assert.That(next.Value.Number, Is.EqualTo(2));
        await service.ActivateAsync(Admin, Guid.NewGuid(), 2, next.Value.VersionId, default);
        Assert.That((await service.ReadAsync(created.Value.VersionId, default))!.Body, Is.EqualTo("Вітаємо {recipientName}"));
        var page = await service.ListAsync(1, 1, default);
        Assert.That(page.Value.Single().VersionId, Is.EqualTo(next.Value.VersionId));
        Assert.That(page.PagedInfo.TotalRecords, Is.EqualTo(2));
        Assert.That(await db.Set<EmailTemplateMutationRecord>().CountAsync(), Is.EqualTo(4));
    }

    [Test]
    public async Task Database_rejects_update_or_delete_of_an_email_version()
    {
        await using var db = Context();
        var v = await new EmailTemplateStore(db).CreateAsync(Admin, Guid.NewGuid(), "Тест", "Тема", "Текст", default);
        var ex = Assert.ThrowsAsync<PostgresException>(async () => await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE crm.\"EmailTemplateVersions\" SET \"Body\"='wrong' WHERE \"PublicId\"={v.Value.VersionId}"));
        Assert.That(ex!.SqlState, Is.EqualTo(PostgresErrorCodes.RaiseException));
        ex = Assert.ThrowsAsync<PostgresException>(async () => await db.Database.ExecuteSqlRawAsync("DELETE FROM crm.\"EmailTemplateVersions\""));
        Assert.That(ex!.SqlState, Is.EqualTo(PostgresErrorCodes.RaiseException));
    }

    [Test]
    public async Task New_or_changed_selection_reference_must_exist_but_unchanged_legacy_reference_is_preserved()
    {
        await using var db = Context();
        var registry = new EmailTemplateStore(db);
        var version = await registry.CreateAsync(Admin, Guid.NewGuid(), "Тест", "КП", "Текст", default);
        var mappings = new IntroductionFeaturedMappingStore(db);
        var mapping = await mappings.ActivateAsync(IntroductionFeaturedMappingVersion.Create(new string('a', 64), "MOD-166", "MOD-169", 115, 118, new CrmUserId(7)), default);
        var selections = Enumerable.Range(0, 4).Select(index => new IntroductionSelection(new IntroductionSlot(IntroductionSeason.Summer, index), index == 0 ? 115 : 200 + index, 20 + index)).ToArray();
        var templates = new IntroductionTemplateStore(db);
        var service = new IntroductionTemplateService(templates, mappings, new IntroductionDraftBuilder(Mock.Of<IIntroductionCatalogReader>()), registry);
        var missing = await service.CreateAsync(Manager, Guid.NewGuid(), "Missing", IntroductionLayout.Summer, Guid.NewGuid(), selections, default);
        Assert.That(missing.Status, Is.EqualTo(ResultStatus.Invalid));
        var valid = await service.CreateAsync(Manager, Guid.NewGuid(), "Known", IntroductionLayout.Summer, version.Value.VersionId, selections, default);
        Assert.That(valid.IsSuccess, Is.True);
        var changed = await service.ReviseAsync(Manager, Guid.NewGuid(), valid.Value.TemplatePublicId, valid.Value.Revision, "Changed", IntroductionLayout.Summer, Guid.NewGuid(), selections, default);
        Assert.That(changed.Status, Is.EqualTo(ResultStatus.Invalid));
        var legacyId = Guid.NewGuid();
        var legacy = await templates.CreateAsync(Manager, Guid.NewGuid(), "Legacy", new IntroductionTemplatePayload(mapping, IntroductionLayout.Summer, legacyId, selections), default);
        var retained = await service.ReviseAsync(Manager, Guid.NewGuid(), legacy.Value.TemplatePublicId, legacy.Value.Revision, "Renamed", IntroductionLayout.Summer, legacyId, selections, default);
        Assert.That(retained.IsSuccess, Is.True);
        Assert.That(retained.Value.Snapshot.EmailTemplateVersionId, Is.EqualTo(legacyId));
        var stranger = await service.ReviseAsync(new CrmActor(18, false), Guid.NewGuid(), legacy.Value.TemplatePublicId, retained.Value.Revision, "Foreign", IntroductionLayout.Summer, legacyId, selections, default);
        Assert.That(stranger.Status, Is.EqualTo(ResultStatus.NotFound));
    }
}
