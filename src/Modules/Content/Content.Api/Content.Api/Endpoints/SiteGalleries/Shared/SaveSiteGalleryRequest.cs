using Content.Application.Features.SiteGalleries.Commands.SaveSiteGallery;

namespace Content.Api.Endpoints.SiteGalleries.Shared;

public sealed record SaveSiteGalleryRequest(int Revision, IReadOnlyList<SiteGalleryItemInput> Items);
