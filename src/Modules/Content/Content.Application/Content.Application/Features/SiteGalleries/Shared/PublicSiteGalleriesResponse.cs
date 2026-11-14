namespace Content.Application.Features.SiteGalleries.Shared;

public sealed record PublicSiteGalleriesResponse(
    IReadOnlyList<PublicGalleryItem> Works,
    IReadOnlyList<PublicGalleryItem> Certificates);
