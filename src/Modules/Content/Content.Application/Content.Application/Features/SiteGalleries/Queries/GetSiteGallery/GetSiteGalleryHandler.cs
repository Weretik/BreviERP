using Content.Application.Abstractions;
using Content.Domain.SiteGalleries;
using Content.Application.Features.SiteGalleries.Shared;
using Content.Application.Features.SiteGalleries.Shared.Admin;

namespace Content.Application.Features.SiteGalleries.Queries.GetSiteGallery;

public sealed class GetSiteGalleryHandler(IContentDbContext db, IContentMediaReader media)
    : IQueryHandler<GetSiteGalleryQuery, Result<SiteGalleryResponse>>
{
    public async ValueTask<Result<SiteGalleryResponse>> Handle(GetSiteGalleryQuery query,
        CancellationToken cancellationToken)
    {
        if (!SiteGallery.IsKey(query.Key)) return Result.NotFound(SiteGalleryMessages.NotFound);
        var gallery = await db.SiteGalleries.AsNoTracking().FirstOrDefaultAsync(x => x.Key == query.Key, cancellationToken);
        return await SiteGalleryReads.ResponseAsync(query.Key, gallery, media, cancellationToken);
    }
}
