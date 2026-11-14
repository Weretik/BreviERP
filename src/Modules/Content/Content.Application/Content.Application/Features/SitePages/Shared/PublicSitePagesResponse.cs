using Content.Domain.ProductPages;
using Content.Domain.SitePages;

namespace Content.Application.Features.SitePages.Shared;

public sealed record PublicSitePagesResponse(IReadOnlyList<PublicSitePage> Items);
