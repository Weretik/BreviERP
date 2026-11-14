using Crm.Domain.Embroidery.Saved;
using Crm.Application.Abstractions.Embroidery;

namespace Crm.Application.Features.Embroidery.Queries.GetEmbroideryMockupUsages;

public sealed class GetEmbroideryMockupUsagesHandler(IEmbroideryStore store)
    : IQueryHandler<GetEmbroideryMockupUsagesQuery, Result<IReadOnlyList<EmbroideryMockupUsage>>>
{
    public async ValueTask<Result<IReadOnlyList<EmbroideryMockupUsage>>> Handle(GetEmbroideryMockupUsagesQuery query,
        CancellationToken cancellationToken)
    {
        var ids = query.MediaFileIds.Distinct().ToArray();
        var rows = await store.EmbroideryCalculations.AsNoTracking()
            .Where(x => x.MockupMediaFileId != null && ids.Contains(x.MockupMediaFileId.Value))
            .Select(x => new { x.Id, x.Name, FileId = x.MockupMediaFileId!.Value })
            .ToListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<EmbroideryMockupUsage>>(rows.Select(x => new EmbroideryMockupUsage(x.FileId,
            x.Id, (EmbroiderySavedCalculation.NumberOf(x.Id) + " " + x.Name).Trim())).ToArray());
    }
}
