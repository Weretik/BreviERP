using Content.Domain.ProductPages;

namespace Content.Application.Features.Catalog.Shared;

/// <summary>An address for the sitemap. <paramref name="HasRussian"/> is false when the Russian page shows Ukrainian texts.</summary>
public sealed record PublicCatalogSlug(string Slug, DateTimeOffset UpdatedAtUtc, bool HasRussian);
