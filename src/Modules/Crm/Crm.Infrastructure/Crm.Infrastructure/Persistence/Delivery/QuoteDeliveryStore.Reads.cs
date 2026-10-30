using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Delivery.Outbox;
using Crm.Domain.Delivery;
using Crm.Domain.Documents.EmailTemplates;
using Crm.Infrastructure.Persistence.Documents.Artifacts;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence.Delivery;

public sealed partial class QuoteDeliveryStore
{
    public async Task<Result<PagedResult<IReadOnlyList<QuoteDeliveryDetail>>>> ListAsync(Guid quoteId, CrmActor actor, int page, int pageSize, CancellationToken ct)
    {
        var quote = await db.Quotes.AsNoTracking().SingleOrDefaultAsync(x => x.PublicId == quoteId, ct);
        if (!CanAccess(quote, actor)) return Result.NotFound();
        if (page is < 1 or > 1000000 || pageSize is < 1 or > 100)
            return Result<PagedResult<IReadOnlyList<QuoteDeliveryDetail>>>.Invalid(new ValidationError("page", "invalid_pagination"));
        var query = db.Set<QuoteDelivery>().AsNoTracking().Where(x => x.QuoteId == quote!.Id);
        var count = await query.CountAsync(ct);
        // Page source columns before constructing positional records: EF cannot translate ordering on record constructor properties.
        var pageQuery = query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.PublicId)
            .Skip((page - 1) * pageSize).Take(pageSize);
        var rows = await Project(pageQuery, quoteId).ToArrayAsync(ct);
        var ordered = rows.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.DeliveryId).ToArray();
        return Result.Success(new PagedResult<IReadOnlyList<QuoteDeliveryDetail>>(new PagedInfo(page, pageSize, (long)Math.Ceiling(count / (double)pageSize), count), ordered));
    }

    public async Task<Result<QuoteDeliveryDetail>> GetAsync(Guid quoteId, Guid deliveryId, CrmActor actor, CancellationToken ct)
    {
        var quote = await db.Quotes.AsNoTracking().SingleOrDefaultAsync(x => x.PublicId == quoteId, ct);
        if (!CanAccess(quote, actor)) return Result.NotFound();
        var id = await db.Set<QuoteDelivery>().AsNoTracking().Where(x => x.QuoteId == quote!.Id && x.PublicId == deliveryId).Select(x => (QuoteDeliveryId?)x.Id).SingleOrDefaultAsync(ct);
        return id is null ? Result.NotFound() : Result.Success(await DetailAsync(id.Value, quoteId, ct));
    }

    private Task<QuoteDeliveryDetail> DetailAsync(QuoteDeliveryId id, Guid quoteId, CancellationToken ct)
        => Project(db.Set<QuoteDelivery>().AsNoTracking().Where(x => x.Id == id), quoteId).SingleAsync(ct);

    private IQueryable<QuoteDeliveryDetail> Project(IQueryable<QuoteDelivery> deliveries, Guid quoteId)
        => from delivery in deliveries
            join state in db.Set<QuoteDeliveryState>().AsNoTracking() on delivery.Id equals state.DeliveryId
            join version in db.QuoteVersions.AsNoTracking() on delivery.QuoteVersionId equals version.Id
            join pdf in db.Set<QuotePdfArtifact>().AsNoTracking() on delivery.ArtifactId equals pdf.Id
            join template in db.Set<EmailTemplateVersion>().AsNoTracking() on delivery.EmailTemplateVersionId equals template.Id
            select new QuoteDeliveryDetail(delivery.PublicId, quoteId, version.PublicId, pdf.PublicId, template.PublicId,
                delivery.RecipientAddress, state.Status, state.Attempt, state.ErrorCode, delivery.CreatedAt, state.UpdatedAt, state.ConfirmedAt);
}
