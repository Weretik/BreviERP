using Crm.Domain.Embroidery.Settings;
using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;
using Crm.Application.Features.Embroidery.Shared.Mapping;
using Crm.Application.Abstractions.Embroidery;

namespace Crm.Application.Features.Embroidery.Queries.GetEmbroideryForm;

public sealed class GetEmbroideryFormHandler(IEmbroideryStore store)
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
