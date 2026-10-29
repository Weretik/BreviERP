using Crm.Application.Documents.Contracts;
using Crm.Domain.Documents;
using Crm.Domain.Documents.Issuer;
using Crm.Domain.Documents.Templates;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence.Documents;

internal sealed class DocumentSettingsReadStore(CrmDbContext db)
{
    public async Task<ActiveIssuerSettings> GetActiveAsync(CancellationToken ct)
    {
        var state = await db.Set<DocumentSettingsState>().AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == new DocumentSettingsStateId(1), ct);
        if (state?.ActiveIssuerVersionId is not { } issuerId || state.ActiveTemplateVersionId is not { } templateId)
            return new ActiveIssuerSettings(state?.Revision ?? 1, null);
        var issuer = await db.Set<IssuerSettingsVersion>().AsNoTracking().SingleAsync(x => x.Id == issuerId, ct);
        var template = await db.Set<DocumentTemplateVersion>().AsNoTracking().SingleAsync(x => x.Id == templateId, ct);
        return new ActiveIssuerSettings(state.Revision, DocumentSettingsMapper.Detail(state.Revision, true, issuer, template));
    }
}
