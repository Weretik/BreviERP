using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Contracts.Responses;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Events;
using Crm.Domain.Quotes.Versions;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence.Quotes.Reading;

public sealed partial class QuoteReadRepository
{
    private IQueryable<Quote> ScopedQuotes(Guid quoteId, CrmActor actor)
        => db.Quotes.AsNoTracking().Where(x => x.PublicId == quoteId &&
            (actor.IsAdmin || x.OwnerId == new CrmUserId(actor.UserId)));

    public Task<QuoteVersion?> FindVersionAsync(Guid quoteId, Guid versionId, CrmActor actor, CancellationToken cancellationToken)
        => db.QuoteVersions.AsNoTracking().AsSplitQuery()
            .Where(x => x.PublicId == versionId && ScopedQuotes(quoteId, actor).Any(q => q.Id == x.QuoteId))
            .Include(x => x.Items).ThenInclude(x => x.Variants).ThenInclude(x => x.Tiers)
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<PagedResult<IReadOnlyList<QuoteVersionSummary>>> VersionsAsync(
        Guid quoteId, CrmActor actor, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = db.QuoteVersions.AsNoTracking()
            .Where(x => ScopedQuotes(quoteId, actor).Any(q => q.Id == x.QuoteId));
        var count = await query.CountAsync(cancellationToken);
        var rows = await query.OrderByDescending(x => x.VersionNumber).ThenBy(x => x.PublicId)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new QuoteVersionSummary(x.PublicId, x.VersionNumber, x.Action, x.ActorId, x.CreatedAt,
                db.QuoteVersions.Where(parent => parent.Id == x.ParentVersionId).Select(parent => (Guid?)parent.PublicId).FirstOrDefault()))
            .ToArrayAsync(cancellationToken);
        return new PagedResult<IReadOnlyList<QuoteVersionSummary>>(new PagedInfo(page, pageSize,
            (long)Math.Ceiling(count / (double)pageSize), count), rows);
    }

    public async Task<PagedResult<IReadOnlyList<QuoteEventView>>> EventsAsync(
        Guid quoteId, CrmActor actor, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = db.Set<QuoteLifecycleEvent>().AsNoTracking()
            .Where(x => ScopedQuotes(quoteId, actor).Any(q => q.Id == x.QuoteId));
        var count = await query.CountAsync(cancellationToken);
        var rows = await query.OrderByDescending(x => x.Revision).ThenBy(x => x.PublicId)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new QuoteEventView(x.PublicId, x.Revision, x.VersionNumber, x.Action,
                x.ActorId.Value, x.OwnerId.Value, x.PreviousOwnerId.HasValue ? x.PreviousOwnerId.Value.Value : null,
                x.Reason, x.CreatedAt)).ToArrayAsync(cancellationToken);
        return new PagedResult<IReadOnlyList<QuoteEventView>>(new PagedInfo(page, pageSize,
            (long)Math.Ceiling(count / (double)pageSize), count), rows);
    }
}
