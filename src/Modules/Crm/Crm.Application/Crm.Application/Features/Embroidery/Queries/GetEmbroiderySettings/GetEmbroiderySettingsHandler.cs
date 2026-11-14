using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;
using Crm.Application.Features.Embroidery.Shared.Mapping;
using Crm.Application.Abstractions.Embroidery;

namespace Crm.Application.Features.Embroidery.Queries.GetEmbroiderySettings;

public sealed class GetEmbroiderySettingsHandler(IEmbroideryStore store, IEmbroideryStitchRateReader stitchRate)
    : IQueryHandler<GetEmbroiderySettingsQuery, Result<EmbroiderySettingsResponse>>
{
    public async ValueTask<Result<EmbroiderySettingsResponse>> Handle(GetEmbroiderySettingsQuery query,
        CancellationToken cancellationToken)
    {
        if (!query.Actor.SeesCost) return Result.Forbidden(EmbroideryMessages.Forbidden);
        var saved = await store.FindSettingsAsync(tracking: false, cancellationToken);
        return Result.Success(saved.ToResponse(await stitchRate.ReadAsync(cancellationToken)));
    }
}
