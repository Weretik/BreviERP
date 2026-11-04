using Crm.Application.Embroidery.Access;
using Crm.Application.Embroidery.Contracts;
using Crm.Application.Embroidery.Mapping;
using Crm.Application.Embroidery.Persistence;
using Crm.Domain.Embroidery.Settings;

namespace Crm.Application.Embroidery.GetForm;

/// <summary>Hoops, extra costs and price names for the «Розрахунок» form; no rates for a Manager.</summary>
public sealed record GetEmbroideryFormQuery(EmbroideryActor Actor) : IQuery<Result<EmbroideryFormResponse>>;

public sealed class GetEmbroideryFormQueryHandler(IEmbroideryStore store)
    : IQueryHandler<GetEmbroideryFormQuery, Result<EmbroideryFormResponse>>
{
    public async ValueTask<Result<EmbroideryFormResponse>> Handle(GetEmbroideryFormQuery query,
        CancellationToken cancellationToken)
    {
        var saved = await store.FindSettingsAsync(tracking: false, cancellationToken);
        var content = saved?.Content ?? EmbroiderySettingsDefaults.Content;
        return Result.Success(content.ToForm(query.Actor.SeesCost, saved?.UpdatedAt));
    }
}
