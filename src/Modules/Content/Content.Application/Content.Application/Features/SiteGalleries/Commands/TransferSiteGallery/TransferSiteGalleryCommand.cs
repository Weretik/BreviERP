using Content.Application.Abstractions;
using Content.Domain.SiteGalleries;
using Content.Application.Features.SiteGalleries.Shared;

namespace Content.Application.Features.SiteGalleries.Commands.TransferSiteGallery;

/// <summary>
/// «Перенести у бібліотеку фото» (CNT-011 SC-03): downloads the next pictures the site still keeps as its own files into
/// the «Фото» library and puts the library photos in their place. One request moves up to
/// <see cref="SiteGalleryFiles.TransferBatch"/> pictures; the Admin repeats it while some are left. A picture already in
/// the library is not downloaded again.
/// </summary>
public sealed record TransferSiteGalleryCommand(string Key, string UpdatedBy) : ICommand<Result<SiteGalleryTransferResponse>>;
