using Crm.Application.Embroidery.Access;
using Crm.Application.Embroidery.Contracts;
using Crm.Application.Embroidery.Mapping;
using Crm.Application.Embroidery.Persistence;
using Crm.Application.Embroidery.StitchRate;

namespace Crm.Application.Embroidery.GetSettings;

/// <summary>«Налаштування» with the embroiderer's rate from «Параметри ціноутворення»; Director and Admin only.</summary>
public sealed record GetEmbroiderySettingsQuery(EmbroideryActor Actor) : IQuery<Result<EmbroiderySettingsResponse>>;

public sealed class GetEmbroiderySettingsQueryHandler(IEmbroideryStore store, IEmbroideryStitchRateReader stitchRate)
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
