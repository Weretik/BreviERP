using Content.Application.Abstractions;
using Content.Application.Contracts;
using Content.Application.SiteSettings.Shared;

namespace Content.Application.SiteSettings.Get;

/// <summary>The settings, empty with revision 0 before the first save (CNT-006 SC-01).</summary>
public sealed record GetSiteSeoSettingsQuery : IQuery<Result<SiteSeoSettingsResponse>>;

public sealed class GetSiteSeoSettingsQueryHandler(IContentDbContext db, IContentMediaReader media)
    : IQueryHandler<GetSiteSeoSettingsQuery, Result<SiteSeoSettingsResponse>>
{
    public async ValueTask<Result<SiteSeoSettingsResponse>> Handle(GetSiteSeoSettingsQuery query,
        CancellationToken cancellationToken)
    {
        var settings = await db.SiteSeoSettings.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        return SiteSettingsReads.Response(settings,
            await SiteSettingsReads.PictureAsync(media, settings?.DefaultOgPhotoId, cancellationToken));
    }
}
