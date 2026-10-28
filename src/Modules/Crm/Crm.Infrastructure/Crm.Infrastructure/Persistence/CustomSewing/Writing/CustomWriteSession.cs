using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Access;
using Crm.Application.CustomSewing.Contracts;
using Crm.Application.CustomSewing.Mapping;
using Crm.Application.CustomSewing.Persistence;
using Crm.Domain.Customers;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.CustomSewing.Drafts;
using Crm.Domain.CustomSewing.Identifiers;
using Crm.Domain.CustomSewing.Pricing;
using Crm.Domain.CustomSewing.Versions;
using Crm.Domain.Quotes;
using Crm.Infrastructure.Persistence.CustomSewing.Pricing;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence.CustomSewing.Writing;

internal sealed class CustomWriteSession(CrmDbContext db, CrmActor actor) : ICustomWriteSession
{
    private readonly CustomCalculationStore _calculations = new(db);
    public async Task<CustomSewingDraft?> FindDraftAsync(Guid publicId, CrmActor requestedActor, CancellationToken ct)
    {
        var quoteId = await db.CustomDrafts.AsNoTracking().Where(CustomAccess.For(requestedActor))
            .Where(x => x.PublicId == publicId).Select(x => x.Quote == null ? (Guid?)null : x.Quote.PublicId).FirstOrDefaultAsync(ct);
        if (quoteId is { } id && await FindQuoteAsync(id, requestedActor, ct) is null) return null;
        return await db.CustomDrafts.FromSqlInterpolated(
                $"SELECT * FROM crm.\"CustomSewingDrafts\" WHERE \"PublicId\" = {publicId} FOR UPDATE")
            .Where(CustomAccess.For(requestedActor)).Include(x => x.Quote).FirstOrDefaultAsync(ct);
    }

    public Task<Customer?> FindCustomerAsync(Guid publicId, CrmActor requestedActor, CancellationToken ct)
        => db.Customers.SingleOrDefaultAsync(x => x.PublicId == publicId
            && (requestedActor.IsAdmin || x.OwnerId == new CrmUserId(requestedActor.UserId)), ct);

    public Task<Quote?> FindQuoteAsync(Guid publicId, CrmActor requestedActor, CancellationToken ct)
        => db.Quotes.FromSqlInterpolated($"SELECT * FROM crm.\"Quotes\" WHERE \"PublicId\" = {publicId} FOR UPDATE")
            .SingleOrDefaultAsync(x => requestedActor.IsAdmin || x.OwnerId == new CrmUserId(requestedActor.UserId), ct);

    public void Add(CustomSewingDraft draft) => db.CustomDrafts.Add(draft);
    public void Add(CustomSewingVersion version) => db.CustomVersions.Add(version);
    public void AddBasis(CustomSewingDraft draft, int actorId, CustomPriceBasis basis)
        => _calculations.Add(draft, actorId, basis);

    public Task<CustomPriceBasis?> FindBasisAsync(Guid token, CustomDraftId draftId, long revision, int actorId, CancellationToken ct)
        => _calculations.FindAsync(token, draftId, revision, actorId, ct);

    public Task FlushAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
    public Task PinFilesAsync(CustomSewingVersion version, CrmActor requestedActor, CancellationToken ct)
        => new Crm.Infrastructure.Files.Versions.CustomFileVersionWriter(db).PinAsync(version, requestedActor, ct);
    public async Task<CustomDetail> DetailAsync(CustomSewingDraft draft, CancellationToken ct)
        => await db.CustomDrafts.AsNoTracking().Where(CustomAccess.For(actor)).Where(x => x.PublicId == draft.PublicId)
            .Select(CustomDetailProjection.Value).FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException("Custom mutation result is not accessible.");
}
