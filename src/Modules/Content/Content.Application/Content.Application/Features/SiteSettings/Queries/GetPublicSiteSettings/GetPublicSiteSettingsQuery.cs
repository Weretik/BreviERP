using Content.Application.Features.SiteSettings.Shared;

namespace Content.Application.Features.SiteSettings.Queries.GetPublicSiteSettings;

/// <summary>Analytics ids, verification codes and the default sharing picture of the public site (CNT-006 SC-03).</summary>
public sealed record GetPublicSiteSettingsQuery : IQuery<Result<PublicSiteSettingsResponse>>;

public sealed class GetPublicSiteSettingsHandler(IPublicSiteSettingsSource source)
    : IQueryHandler<GetPublicSiteSettingsQuery, Result<PublicSiteSettingsResponse>>
{
    public async ValueTask<Result<PublicSiteSettingsResponse>> Handle(GetPublicSiteSettingsQuery query,
        CancellationToken cancellationToken) =>
        await source.GetAsync(cancellationToken);
}
