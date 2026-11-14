using Content.Application.Features.SiteSettings.Shared;

namespace Content.Application.Features.SiteSettings.Queries.GetPublicSiteSettings;

public sealed class GetPublicSiteSettingsHandler(IPublicSiteSettingsSource source)
    : IQueryHandler<GetPublicSiteSettingsQuery, Result<PublicSiteSettingsResponse>>
{
    public async ValueTask<Result<PublicSiteSettingsResponse>> Handle(GetPublicSiteSettingsQuery query,
        CancellationToken cancellationToken) =>
        await source.GetAsync(cancellationToken);
}
