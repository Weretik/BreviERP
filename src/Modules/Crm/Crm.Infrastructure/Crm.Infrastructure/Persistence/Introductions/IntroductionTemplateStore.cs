using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Introductions;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Quotes.Introductions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Crm.Infrastructure.Persistence.Introductions;

public sealed partial class IntroductionTemplateStore(CrmDbContext db) : IIntroductionTemplateStore
{
    public Task<Result<IntroductionTemplateDetail>> CreateAsync(CrmActor actor, Guid requestId, string name,
        IntroductionTemplatePayload payload, CancellationToken cancellationToken)
        => ExecuteAsync("CreateIntroductionTemplate", requestId, actor, new { name, payload }, async ct =>
        {
            var template = IntroductionTemplate.Create(new CrmUserId(actor.UserId));
            db.IntroductionTemplates.Add(template);
            await db.SaveChangesAsync(ct);
            var version = IntroductionTemplateVersion.Create(template.Id, 1, name, payload);
            template.RecordVersion(template.Revision, version);
            db.IntroductionTemplateVersions.Add(version);
            await db.SaveChangesAsync(ct);
            return (Result.Success(ToDetail(template, version)), version.Payload);
        }, cancellationToken);

    public Task<Result<IntroductionTemplateDetail>> ReviseAsync(CrmActor actor, Guid requestId, Guid templatePublicId,
        long expectedRevision, string name, IntroductionTemplatePayload payload, CancellationToken cancellationToken)
        => ExecuteAsync("ReviseIntroductionTemplate", requestId, actor,
            new { templatePublicId, expectedRevision, name, payload }, async ct =>
        {
            var template = await Accessible(actor).SingleOrDefaultAsync(x => x.PublicId == templatePublicId, ct);
            if (template is null) return (Result<IntroductionTemplateDetail>.NotFound(), null);
            if (template.Revision != expectedRevision) return (Result<IntroductionTemplateDetail>.Conflict("revision_conflict"), null);
            if (template.Archived) return (Result<IntroductionTemplateDetail>.Conflict("template_archived"), null);
            var version = IntroductionTemplateVersion.Create(template.Id, template.VersionNumber + 1, name, payload);
            template.RecordVersion(expectedRevision, version);
            db.IntroductionTemplateVersions.Add(version);
            await db.SaveChangesAsync(ct);
            return (Result.Success(ToDetail(template, version)), version.Payload);
        }, cancellationToken);

    public Task<Result<IntroductionTemplateDetail>> SetArchivedAsync(CrmActor actor, Guid requestId,
        Guid templatePublicId, long expectedRevision, bool archived, CancellationToken cancellationToken)
        => ExecuteAsync("ArchiveIntroductionTemplate", requestId, actor,
            new { templatePublicId, expectedRevision, archived }, async ct =>
        {
            var template = await Accessible(actor).SingleOrDefaultAsync(x => x.PublicId == templatePublicId, ct);
            if (template is null) return (Result<IntroductionTemplateDetail>.NotFound(), null);
            if (template.Revision != expectedRevision) return (Result<IntroductionTemplateDetail>.Conflict("revision_conflict"), null);
            template.SetArchived(expectedRevision, archived);
            await db.SaveChangesAsync(ct);
            var version = await db.IntroductionTemplateVersions.AsNoTracking().SingleAsync(
                x => x.TemplateId == template.Id && x.Number == template.VersionNumber, ct);
            return (Result.Success(ToDetail(template, version)), version.Payload);
        }, cancellationToken);

    private IQueryable<IntroductionTemplate> Accessible(CrmActor actor) =>
        db.IntroductionTemplates.Where(x => actor.UserId > 0 &&
            (actor.IsAdmin || x.OwnerId == new CrmUserId(actor.UserId)));

    private static IntroductionTemplateDetail ToDetail(IntroductionTemplate template, IntroductionTemplateVersion version)
        => new(template.PublicId, template.OwnerId.Value, template.Revision, template.Archived,
            version.PublicId, version.Number, version.Name, version.Payload.ToSnapshot(), template.UpdatedAt);
}
