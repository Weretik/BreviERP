using Content.Application.Features.SiteGalleries.Shared;
using Content.Application.Features.SiteGalleries.Shared.Public;

namespace Content.Application.Features.SiteGalleries.Queries.GetPublicSiteGalleries;

/// <summary>Pictures of «Наші роботи» and «Сертифікати» for the public site (CNT-011 SC-04).</summary>
public sealed record GetPublicSiteGalleriesQuery : IQuery<Result<PublicSiteGalleriesResponse>>;
