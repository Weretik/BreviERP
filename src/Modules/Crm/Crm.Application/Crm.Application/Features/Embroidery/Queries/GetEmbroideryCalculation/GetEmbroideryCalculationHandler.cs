using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;
using Crm.Application.Features.Embroidery.Shared.Mapping;
using Crm.Application.Abstractions.Embroidery;

namespace Crm.Application.Features.Embroidery.Queries.GetEmbroideryCalculation;

public sealed class GetEmbroideryCalculationHandler(IEmbroideryStore store, IEmbroideryMediaReader media)
    : IQueryHandler<GetEmbroideryCalculationQuery, Result<EmbroiderySavedCalculationResponse>>
{
    public async ValueTask<Result<EmbroiderySavedCalculationResponse>> Handle(GetEmbroideryCalculationQuery query,
        CancellationToken cancellationToken)
    {
        var saved = await store.EmbroideryCalculations.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == query.Id, cancellationToken);
        if (saved is null) return Result.NotFound(EmbroideryMessages.CalculationNotFound);
        var mockups = await media.ReadForAsync([saved.MockupMediaFileId], cancellationToken);
        return Result.Success(saved.ToResponse(query.Actor.SeesCost, mockups.Of(saved.MockupMediaFileId)));
    }
}
