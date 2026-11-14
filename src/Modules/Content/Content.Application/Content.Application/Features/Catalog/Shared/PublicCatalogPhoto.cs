using Content.Domain.ProductPages;

namespace Content.Application.Features.Catalog.Shared;

public sealed record PublicCatalogPhoto(string Url, string? ThumbnailUrl, bool IsMain);
