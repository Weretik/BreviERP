using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.ApplyTemplate;
using Crm.Application.CustomSewing.Templates.Contracts;
using Crm.Domain.CustomSewing.Templates;
using Microsoft.EntityFrameworkCore;
namespace Crm.Infrastructure.Persistence.Templates.Reading;

public sealed class TemplateApplySource(CrmDbContext db) : ITemplateApplySource
{
    public async Task<TemplateDetail?> LockAsync(Guid templateId, Guid versionId, CrmActor actor, CancellationToken ct)
    {
        if (db.Database.CurrentTransaction is null) throw new InvalidOperationException("Template apply requires the draft transaction.");
        // Materialize before the regular scope projection; FOR UPDATE must retain its original query shape.
        _ = await db.Set<CustomTemplate>().FromSqlInterpolated($"""SELECT * FROM crm."CustomSewingTemplates" WHERE "PublicId" = {templateId} FOR UPDATE""")
            .AsNoTracking().ToArrayAsync(ct);
        return await new TemplateReadStore(db).DetailAsync(templateId, versionId, actor, ct);
    }
}
