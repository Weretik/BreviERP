using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Documents.EmailTemplates;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Documents.EmailTemplates;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence.Documents.EmailTemplates;

public sealed partial class EmailTemplateStore(CrmDbContext db) : IEmailTemplateStore
{
    public Task<Result<EmailTemplateDetail>> CreateAsync(CrmActor actor, Guid requestId,
        string name, string subject, string body, CancellationToken ct)
        => ExecuteAsync("CreateEmailTemplate", actor, requestId, new { name, subject, body }, async token =>
        {
            var number = (await db.Set<EmailTemplateVersion>().MaxAsync(x => (int?)x.Number, token) ?? 0) + 1;
            var version = EmailTemplateVersion.Create(number, name, subject, body, new CrmUserId(actor.UserId));
            db.Set<EmailTemplateVersion>().Add(version);
            await db.SaveChangesAsync(token);
            return Result.Success(Detail(version));
        }, ct);

    public Task<Result<EmailTemplateSettings>> ActivateAsync(CrmActor actor, Guid requestId,
        long expectedRevision, Guid versionId, CancellationToken ct)
        => ExecuteAsync("ActivateEmailTemplate", actor, requestId, new { expectedRevision, versionId }, async token =>
        {
            var version = await db.Set<EmailTemplateVersion>().AsNoTracking().SingleOrDefaultAsync(x => x.PublicId == versionId, token);
            if (version is null) return Result<EmailTemplateSettings>.NotFound();
            var state = await db.Set<EmailTemplateState>().SingleOrDefaultAsync(x => x.Id == new EmailTemplateStateId(1), token);
            if (state is null) { state = EmailTemplateState.Create(); db.Set<EmailTemplateState>().Add(state); }
            state.Activate(expectedRevision, version.Id);
            await db.SaveChangesAsync(token);
            return Result.Success(new EmailTemplateSettings(state.Revision, Detail(version)));
        }, ct);

    private static EmailTemplateDetail Detail(EmailTemplateVersion v) => new(v.PublicId, v.Number, v.Name, v.Subject, v.Body, v.CreatedAt);
}
