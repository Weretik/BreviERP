using Crm.Domain.Embroidery.Saved;
using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;
using Crm.Application.Features.Embroidery.Shared.Mapping;
using Crm.Application.Abstractions.Embroidery;

namespace Crm.Application.Features.Embroidery.Queries.GetEmbroideryCalculations;

public sealed class GetEmbroideryCalculationsHandler(IEmbroideryStore store, IEmbroideryMediaReader media)
    : IQueryHandler<GetEmbroideryCalculationsQuery, Result<EmbroiderySavedPageResponse>>
{
    public async ValueTask<Result<EmbroiderySavedPageResponse>> Handle(GetEmbroideryCalculationsQuery query,
        CancellationToken cancellationToken)
    {
        var rows = store.EmbroideryCalculations.AsNoTracking();
        if (query.CustomerPublicId is { } customer)
            rows = rows.Where(x => x.CustomerPublicId == customer);
        if (query.From is { } from)
        {
            var start = LeadClock.StartOf(from);
            rows = rows.Where(x => x.CreatedAt >= start);
        }
        if (query.To is { } to)
        {
            var end = LeadClock.StartOf(to.AddDays(1));
            rows = rows.Where(x => x.CreatedAt < end);
        }
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var number = EmbroideryNumberSearch.Parse(query.Search.Trim().ToLower());
            rows = rows.WhereEveryWord(query.Search, pattern => x => x.Id == new EmbroiderySavedCalculationId(number) || EF.Functions.Like(EF.Property<string>(x, SearchWords.Column), pattern, SearchWords.Escape));
        }

        var total = await rows.CountAsync(cancellationToken);
        var page = await rows.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(cancellationToken);
        var mockups = await media.ReadForAsync(page.Select(x => x.MockupMediaFileId), cancellationToken);
        return Result.Success(new EmbroiderySavedPageResponse(
            [.. page.Select(x => x.ToSummary(mockups.Of(x.MockupMediaFileId)))], query.Page,
            query.PageSize, total));
    }
}
