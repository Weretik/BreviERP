using System.Text.Json;
using Crm.Domain.CustomSewing.Drafts;
using Crm.Domain.CustomSewing.Identifiers;
using Crm.Domain.CustomSewing.Pricing;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence.CustomSewing.Pricing;

internal sealed class CustomCalculationStore(CrmDbContext db)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    internal void Add(CustomSewingDraft draft, int actorId, CustomPriceBasis basis)
        => db.CustomCalculations.Add(new CustomCalculationRecord { DraftId = draft.Id, Revision = draft.Revision,
            ActorId = actorId, Token = basis.Token, ExpiresAt = basis.ExpiresAt, BasisJson = JsonSerializer.Serialize(basis, Json) });

    internal async Task<CustomPriceBasis?> FindAsync(Guid token, CustomDraftId draftId, long revision, int actorId, CancellationToken ct)
    {
        var row = await db.CustomCalculations.AsNoTracking().SingleOrDefaultAsync(x => x.Token == token
            && x.DraftId == draftId && x.Revision == revision && x.ActorId == actorId && x.ExpiresAt > DateTimeOffset.UtcNow, ct);
        return row is null ? null : JsonSerializer.Deserialize<CustomPriceBasis>(row.BasisJson, Json);
    }
}
