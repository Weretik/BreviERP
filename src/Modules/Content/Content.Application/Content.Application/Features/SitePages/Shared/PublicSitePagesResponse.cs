using Content.Domain.ProductPages;
using Content.Domain.SitePages;

namespace Content.Application.Features.SitePages.Shared;

/// <remarks>Cached as is by <see cref="PublicSiteCache"/>, so it is immutable.</remarks>
[System.ComponentModel.ImmutableObject(true)]
public sealed record PublicSitePagesResponse(IReadOnlyList<PublicSitePage> Items);
