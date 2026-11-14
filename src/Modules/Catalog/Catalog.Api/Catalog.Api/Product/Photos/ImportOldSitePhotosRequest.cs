using Catalog.Application.Features.Product.Commands.ImportOldSitePhotos;

namespace Catalog.Api.Product.Photos;

/// <summary>The file «old-site-photo-import.json», or a part of it for a real run (CAT-016).</summary>
public sealed record ImportOldSitePhotosRequest(bool DryRun, string BaseUrl, IReadOnlyList<OldSiteProduct> Products);
