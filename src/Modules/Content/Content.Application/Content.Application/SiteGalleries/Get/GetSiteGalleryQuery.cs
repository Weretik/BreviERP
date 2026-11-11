using Content.Application.Abstractions;
using Content.Application.Contracts;
using Content.Application.SiteGalleries.Shared;
using Content.Domain.SiteGalleries;

namespace Content.Application.SiteGalleries.Get;

/// <summary>The pictures of «Наші роботи» or «Сертифікати» with their addresses (CNT-011 SC-01).</summary>
public sealed record GetSiteGalleryQuery(string Key) : IQuery<Result<SiteGalleryResponse>>;

public sealed class GetSiteGalleryQueryHandler(IContentDbContext db, IContentMediaReader media)
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
