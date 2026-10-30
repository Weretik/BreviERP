using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Templates.Contracts;
using Crm.Application.CustomSewing.Templates.Persistence;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.CustomSewing.Templates;
using Crm.Infrastructure.Persistence.Templates.Reading;
using Microsoft.EntityFrameworkCore;
namespace Crm.Infrastructure.Persistence.Templates.Writing;

internal sealed class TemplateWriteSession(CrmDbContext db) : ITemplateWriteSession
{
    public Task<CustomTemplate?> FindAsync(Guid id, CrmActor actor, CancellationToken ct)
        => db.Set<CustomTemplate>().Where(x => actor.UserId > 0 && (actor.IsAdmin || x.OwnerId == new CrmUserId(actor.UserId)))
            .SingleOrDefaultAsync(x => x.PublicId == id, ct);
    public void Add(CustomTemplate template) => db.Set<CustomTemplate>().Add(template);
    public void Add(CustomTemplateVersion version) => db.Set<CustomTemplateVersion>().Add(version);
    public Task FlushAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
    public async Task<TemplateDetail> DetailAsync(CustomTemplate template, CrmActor actor, CancellationToken ct)
        => await new TemplateReadStore(db).DetailAsync(template.PublicId, null, actor, ct)
            ?? throw new InvalidOperationException("Saved template has no current version.");
}
