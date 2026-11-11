using Content.Application.Contracts;

namespace Content.Api.SiteGalleries;

public sealed record SaveSiteGalleryRequest(int Revision, IReadOnlyList<SiteGalleryItemInput> Items);
