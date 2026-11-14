using Content.Application.Abstractions;
using Content.Domain.SiteGalleries;
using Content.Application.Features.SiteGalleries.Shared;

namespace Content.Application.Features.SiteGalleries.Queries.GetSiteGallery;

/// <summary>The pictures of «Наші роботи» or «Сертифікати» with their addresses (CNT-011 SC-01).</summary>
public sealed record GetSiteGalleryQuery(string Key) : IQuery<Result<SiteGalleryResponse>>;
