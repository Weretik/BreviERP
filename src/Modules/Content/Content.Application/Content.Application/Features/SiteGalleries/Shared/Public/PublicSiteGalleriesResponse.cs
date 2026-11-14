namespace Content.Application.Features.SiteGalleries.Shared.Public;

public sealed record PublicSiteGalleriesResponse(
    IReadOnlyList<PublicGalleryItem> Works,
    IReadOnlyList<PublicGalleryItem> Certificates);
