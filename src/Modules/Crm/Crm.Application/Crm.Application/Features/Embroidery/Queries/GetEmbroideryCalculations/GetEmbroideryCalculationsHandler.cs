using Crm.Domain.Embroidery.Saved;
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
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            var number = EmbroideryNumberSearch.Parse(search);
            rows = rows.Where(x => x.Id == number || x.Name.ToLower().Contains(search) ||
                (x.CustomerName != null && x.CustomerName.ToLower().Contains(search)));
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
