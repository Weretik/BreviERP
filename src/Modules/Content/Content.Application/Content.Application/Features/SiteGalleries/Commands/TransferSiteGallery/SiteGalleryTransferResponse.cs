using Content.Application.Features.SiteGalleries.Shared;
using Content.Application.Features.SiteGalleries.Shared.Admin;
namespace Content.Application.Features.SiteGalleries.Commands.TransferSiteGallery;

/// <summary>One step of the transfer: how many pictures moved now, how many still wait, and why some failed.</summary>
public sealed record SiteGalleryTransferResponse(
    int Moved,
    int Left,
    IReadOnlyList<string> Problems,
    SiteGalleryResponse Gallery);
