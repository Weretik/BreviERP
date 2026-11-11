using Content.Application.Contracts;

namespace Content.Application.PublicSiteSettings;

/// <summary>Analytics ids, verification codes and the default sharing picture of the public site (CNT-006 SC-03).</summary>
public sealed record GetPublicSiteSettingsQuery : IQuery<Result<PublicSiteSettingsResponse>>;

public sealed class GetPublicSiteSettingsQueryHandler(IPublicSiteSettingsSource source)
    : IQueryHandler<GetPublicSiteSettingsQuery, Result<PublicSiteSettingsResponse>>
{
    public async ValueTask<Result<PublicSiteSettingsResponse>> Handle(GetPublicSiteSettingsQuery query,
        CancellationToken cancellationToken) =>
        await source.GetAsync(cancellationToken);
}
