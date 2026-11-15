using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Mapping;
using Crm.Application.Abstractions.Embroidery;

namespace Crm.Application.Features.Embroidery.Queries.GetEmbroideryCalculationsById;

/// <summary>The calculations and their mockups in two queries, whatever the number of ids.</summary>
public sealed class GetEmbroideryCalculationsByIdHandler(IEmbroideryStore store, IEmbroideryMediaReader media)
    : IQueryHandler<GetEmbroideryCalculationsByIdQuery, Result<IReadOnlyList<EmbroiderySavedCalculationResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<EmbroiderySavedCalculationResponse>>> Handle(
        GetEmbroideryCalculationsByIdQuery query, CancellationToken cancellationToken)
    {
        var ids = query.Ids.Distinct().ToArray();
        if (ids.Length == 0) return Result.Success<IReadOnlyList<EmbroiderySavedCalculationResponse>>([]);
        var saved = await store.EmbroideryCalculations.AsNoTracking().Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);
        var mockups = await media.ReadForAsync(saved.Select(x => x.MockupMediaFileId), cancellationToken);
        IReadOnlyList<EmbroiderySavedCalculationResponse> found = saved
            .Select(x => x.ToResponse(query.Actor.SeesCost, mockups.Of(x.MockupMediaFileId))).ToList();
        return Result.Success(found);
    }
}
