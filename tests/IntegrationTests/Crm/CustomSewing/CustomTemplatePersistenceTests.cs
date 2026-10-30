using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.ApplyTemplate;
using Crm.Application.CustomSewing.Calculate;
using Crm.Application.CustomSewing.ArchiveTemplate;
using Crm.Application.CustomSewing.CreateTemplate;
using Crm.Application.CustomSewing.ReviseTemplate;
using Crm.Domain.CustomSewing.Templates;
using Crm.Infrastructure.Persistence;
using Crm.Infrastructure.Persistence.CustomSewing.Writing;
using Crm.Infrastructure.Persistence.Templates.Reading;
using Crm.Infrastructure.Persistence.Templates.Writing;
using Microsoft.EntityFrameworkCore;
using Npgsql;
namespace IntegrationTests.Crm.CustomSewing;

[NonParallelizable]
public sealed class CustomTemplatePersistenceTests : CrmDatabaseFixture
{
    private static readonly CrmActor Manager = new(17, false);
    private static TemplateSelections Selections => new("Куртка", "Куртка", "winter", "Public", "Comment",
        [new(1, 2, "Meters")], [], [new(2, 1, null)], [new("print", "Front")]);
    private static CreateTemplateCommandHandler Creator(CrmDbContext db) => new(new TemplateMutationStore(db));
    private static ApplyTemplateCommandHandler Applier(CrmDbContext db) => new(new CustomMutationStore(db), new TemplateReadStore(db), new TemplateApplySource(db));

    [Test]
    public async Task Versions_remain_immutable_and_old_version_applies_to_new_private_draft_with_persisted_origin()
    {
        await using var db = Context();
        var initial = (await Creator(db).Handle(new(Guid.NewGuid(), "Original intro", Selections, Manager), default)).Value;
        var revised = (await new ReviseTemplateCommandHandler(new TemplateMutationStore(db)).Handle(new(initial.PublicId,
            initial.Revision, Guid.NewGuid(), "New intro", Selections with { Fabrics = [new(1, 3, "Meters")] }, Manager), default)).Value;
        Assert.That(revised.VersionNumber, Is.EqualTo(2));
        var old = await new TemplateReadStore(db).DetailAsync(initial.PublicId, initial.VersionId, Manager, default);
        Assert.That(old!.Selections.Fabrics.Single().Quantity, Is.EqualTo(2));
        var command = new ApplyTemplateCommand(initial.PublicId, initial.VersionId, revised.Revision, Guid.NewGuid(), null, null, Manager);
        var applied = await Applier(db).Handle(command, default);
        Assert.That(applied.IsSuccess, Is.True, string.Join(",", applied.Errors));
        var replay = await Applier(db).Handle(command, default);
        Assert.That(replay.Value.Draft.PublicId, Is.EqualTo(applied.Value.Draft.PublicId));
        Assert.That(replay.Value.Introduction, Is.EqualTo("Original intro"));
        var differentRequest = await Applier(db).Handle(command with { RequestId = Guid.NewGuid() }, default);
        Assert.That(differentRequest.Value.Draft.PublicId, Is.Not.EqualTo(applied.Value.Draft.PublicId));
        var persisted = await db.CustomDrafts.AsNoTracking().SingleAsync(x => x.PublicId == applied.Value.Draft.PublicId);
        Assert.Multiple(() => {
            Assert.That(persisted.OwnerId.Value, Is.EqualTo(17));
            Assert.That(persisted.CurrentVersionId, Is.Null);
            Assert.That(persisted.TemplateOrigin!.TemplatePublicId, Is.EqualTo(initial.PublicId));
            Assert.That(persisted.TemplateOrigin.VersionPublicId, Is.EqualTo(initial.VersionId));
            Assert.That(persisted.TemplateOrigin.Introduction, Is.EqualTo("Original intro"));
            Assert.That(persisted.Configuration.Fabrics.Single().Quantity, Is.EqualTo(2));
            Assert.That(persisted.Configuration.InternalNote, Is.Empty);
            Assert.That(persisted.Configuration.Applications.Single().SaleApproved, Is.False);
        });
        Assert.That(await db.CustomCalculations.CountAsync(), Is.Zero);
        Assert.That(await db.CustomVersions.CountAsync(), Is.Zero);
        Assert.That((await Applier(db).Handle(command with { VersionId = Guid.NewGuid() }, default)).Status, Is.EqualTo(ResultStatus.Conflict));
        var currentPrices = new CustomSewingTestPrices { SourceVersion = 55 };
        Assert.That(currentPrices.Calls, Is.Zero);
        var calculated = await new CalculateCustomSewingCommandHandler(new CustomMutationStore(db), currentPrices).Handle(
            new(applied.Value.Draft.PublicId, applied.Value.Draft.Revision, Guid.NewGuid(), Manager), default);
        Assert.That(calculated.IsSuccess, Is.True);
        Assert.That(calculated.Value.Preview!.References.Single(x => x.Key == "Fabric:1").VersionId, Is.EqualTo(55));
        Assert.That(currentPrices.Calls, Is.EqualTo(1));
    }

    [Test]
    public async Task Request_replay_scope_payload_revision_archive_and_admin_access_are_enforced()
    {
        await using var db = Context();
        var command = new CreateTemplateCommand(Guid.NewGuid(), "Intro", Selections, Manager);
        var created = await Creator(db).Handle(command, default);
        var repeated = await Creator(db).Handle(command, default);
        Assert.That(repeated.Value.VersionId, Is.EqualTo(created.Value.VersionId));
        Assert.That((await Creator(db).Handle(command with { Introduction = "Changed" }, default)).Status, Is.EqualTo(ResultStatus.Conflict));
        var reader = new TemplateReadStore(db);
        Assert.That(await reader.DetailAsync(created.Value.PublicId, null, new(18, false), default), Is.Null);
        Assert.That(await reader.DetailAsync(created.Value.PublicId, null, new(18, true), default), Is.Not.Null);
        Assert.That((await reader.ListAsync(new(18, false), true, 1, 10, default)).Value, Is.Empty);
        Assert.That((await reader.ListAsync(new(18, true), true, 1, 10, default)).Value, Has.Count.EqualTo(1));
        var adminCommand = command with { RequestId = Guid.NewGuid(), Actor = new(19, true) };
        var adminTemplate = (await Creator(db).Handle(adminCommand, default)).Value;
        var reviseAdmin = new ReviseTemplateCommand(created.Value.PublicId, created.Value.Revision, Guid.NewGuid(), "Admin revision", Selections, new(19, true));
        var changedByAdmin = await new ReviseTemplateCommandHandler(new TemplateMutationStore(db)).Handle(reviseAdmin, default);
        Assert.That(changedByAdmin.IsSuccess, Is.True);
        Assert.That((await new ReviseTemplateCommandHandler(new TemplateMutationStore(db)).Handle(reviseAdmin with { Actor = new(19, false) }, default)).Status,
            Is.EqualTo(ResultStatus.NotFound));
        var stale = await new ReviseTemplateCommandHandler(new TemplateMutationStore(db)).Handle(new(created.Value.PublicId,
            created.Value.Revision, Guid.NewGuid(), "Stale", Selections, Manager), default);
        Assert.That(stale.Status, Is.EqualTo(ResultStatus.Conflict));
        var archived = (await new ArchiveTemplateCommandHandler(new TemplateMutationStore(db)).Handle(new(created.Value.PublicId,
            changedByAdmin.Value.Revision, Guid.NewGuid(), true, Manager), default)).Value;
        Assert.That(archived.CanEdit, Is.False);
        Assert.That((await reader.DetailAsync(archived.PublicId, null, Manager, default))!.CanEdit, Is.False);
        var applyArchived = await Applier(db).Handle(new(archived.PublicId, archived.VersionId, archived.Revision, Guid.NewGuid(), null, null, Manager), default);
        Assert.That(applyArchived.Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That((await reader.ListAsync(Manager, false, 1, 10, default)).Value, Is.Empty);
        Assert.That((await reader.ListAsync(Manager, true, 1, 10, default)).Value, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Database_rejects_template_version_rewrite_and_history_delete()
    {
        await using var db = Context();
        var template = (await Creator(db).Handle(new(Guid.NewGuid(), "Immutable", Selections, Manager), default)).Value;
        var update = Assert.ThrowsAsync<PostgresException>(async () => await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE crm.\"CustomSewingTemplateVersions\" SET \"Introduction\" = 'Wrong' WHERE \"PublicId\" = {template.VersionId}"));
        Assert.That(update!.SqlState, Is.EqualTo(PostgresErrorCodes.RaiseException));
        Assert.ThrowsAsync<PostgresException>(async () => await db.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM crm.\"CustomSewingTemplateVersions\" WHERE \"PublicId\" = {template.VersionId}"));
    }
}
