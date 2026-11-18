namespace Content.Application.Features.SiteGalleries.Shared.Public;

/// <remarks>Cached as is by <see cref="PublicSiteCache"/>, so it is immutable.</remarks>
[System.ComponentModel.ImmutableObject(true)]
public sealed record PublicSiteGalleriesResponse(
    IReadOnlyList<PublicGalleryItem> Works,
    IReadOnlyList<PublicGalleryItem> Certificates);
