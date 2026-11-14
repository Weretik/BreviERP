using Content.Application.Features.SiteGalleries.Commands.SaveSiteGallery;

namespace Content.Api.SiteGalleries;

public sealed record SaveSiteGalleryRequest(int Revision, IReadOnlyList<SiteGalleryItemInput> Items);
