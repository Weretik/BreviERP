using Catalog.Application.Features.Product.OldSitePhotoImport;

namespace Catalog.Api.Contracts.Products.Photos;

/// <summary>The file «old-site-photo-import.json», or a part of it for a real run (CAT-016).</summary>
public sealed record ImportOldSitePhotosRequest(bool DryRun, string BaseUrl, IReadOnlyList<OldSiteProduct> Products);
