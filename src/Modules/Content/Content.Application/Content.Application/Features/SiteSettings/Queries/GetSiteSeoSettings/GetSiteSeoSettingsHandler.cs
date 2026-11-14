using Content.Application.Abstractions;
using Content.Application.Features.SiteSettings.Shared;

namespace Content.Application.Features.SiteSettings.Queries.GetSiteSeoSettings;

public sealed class GetSiteSeoSettingsHandler(IContentDbContext db, IContentMediaReader media)
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
