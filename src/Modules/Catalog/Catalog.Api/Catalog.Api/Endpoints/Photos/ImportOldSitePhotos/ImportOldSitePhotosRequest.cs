using Catalog.Application.Features.Product.Commands.ImportOldSitePhotos;

namespace Catalog.Api.Endpoints.Photos.ImportOldSitePhotos;

/// <summary>The file «old-site-photo-import.json», or a part of it for a real run (CAT-016).</summary>
public sealed record ImportOldSitePhotosRequest(bool DryRun, string BaseUrl, IReadOnlyList<OldSiteProduct> Products);
