using Content.Application.Features.SiteGalleries.Shared;
using Content.Application.Features.SiteGalleries.Shared.Admin;
namespace Content.Application.Features.SiteGalleries.Commands.SaveSiteGallery;

/// <summary>
/// One picture as staff send it (CNT-011): a photo of the «Фото» library, or a file the site still keeps
/// (<paramref name="LegacyPath"/>, only one the gallery already has).
/// </summary>
public sealed record SiteGalleryItemInput(
    int? PhotoId,
    string? LegacyPath,
    GalleryText? Title,
    GalleryText? Note,
    IReadOnlyList<string>? Standards);
