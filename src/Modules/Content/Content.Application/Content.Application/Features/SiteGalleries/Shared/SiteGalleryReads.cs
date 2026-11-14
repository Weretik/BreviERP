using Content.Application.Abstractions;
using Content.Domain.SiteCompany;
using Content.Domain.SiteGalleries;
using Content.Application.Features.SiteGalleries.Shared.Admin;
using Content.Application.Features.SiteGalleries.Shared.Public;

namespace Content.Application.Features.SiteGalleries.Shared;

/// <summary>Builds the Admin and public answers of a gallery with the addresses of its library photos.</summary>
internal static class SiteGalleryReads
{
    public static async Task<SiteGalleryResponse> ResponseAsync(string key, SiteGallery? gallery,
        IContentMediaReader media, CancellationToken cancellationToken)
    {
        var items = gallery?.Items ?? [];
        var photos = await PhotosAsync(items, media, cancellationToken);
        return new SiteGalleryResponse(key, items.Select(item => Item(item, photos)).ToArray(),
            items.Count(x => x.PhotoId is null && x.Legacy is not null), gallery?.Revision ?? 0, gallery?.UpdatedAtUtc,
            gallery?.UpdatedBy);
    }

    /// <summary>Pictures for the site: a photo the library no longer has is left out.</summary>
    public static IReadOnlyList<PublicGalleryItem> Public(SiteGallery? gallery,
        IReadOnlyDictionary<int, ContentMediaPhoto> photos) =>
        (gallery?.Items ?? []).Select(item =>
        {
            if (item.Legacy is { } legacy)
                return new PublicGalleryItem(legacy.Path, legacy.ThumbPath, legacy.Width, legacy.Height, Text(item.Title),
                    Text(item.Note), item.Standards ?? []);
            if (item.PhotoId is { } id && photos.TryGetValue(id, out var photo))
                return new PublicGalleryItem(photo.Url, photo.ThumbnailUrl ?? photo.Url, photo.Width, photo.Height,
                    Text(item.Title), Text(item.Note), item.Standards ?? []);
            return null;
        }).Where(x => x is not null).Select(x => x!).ToArray();

    public static async Task<IReadOnlyDictionary<int, ContentMediaPhoto>> PhotosAsync(
        IEnumerable<SiteGalleryItem> items, IContentMediaReader media, CancellationToken cancellationToken)
    {
        var ids = items.Where(x => x.PhotoId is not null).Select(x => x.PhotoId!.Value).Distinct().ToArray();
        if (ids.Length == 0) return new Dictionary<int, ContentMediaPhoto>();
        return (await media.GetAsync(ids, cancellationToken)).ToDictionary(x => x.Id);
    }

    public static GalleryText Text(SiteText? text) => new(text?.Uk ?? string.Empty, text?.Ru ?? string.Empty);

    private static SiteGalleryItemResponse Item(SiteGalleryItem item, IReadOnlyDictionary<int, ContentMediaPhoto> photos)
    {
        if (item.Legacy is { } legacy)
            return new SiteGalleryItemResponse(null, legacy.Path, SiteGalleryFiles.SiteOrigin + legacy.Path,
                SiteGalleryFiles.SiteOrigin + legacy.ThumbPath, legacy.Width, legacy.Height, false, Text(item.Title),
                Text(item.Note), item.Standards ?? []);
        var photo = item.PhotoId is { } id ? photos.GetValueOrDefault(id) : null;
        return new SiteGalleryItemResponse(item.PhotoId, null, photo?.Url, photo?.ThumbnailUrl ?? photo?.Url,
            photo?.Width, photo?.Height, photo is null, Text(item.Title), Text(item.Note), item.Standards ?? []);
    }
}
