
using Media.Contracts.Imports;
using Catalog.Application.Abstractions.Media;

namespace Catalog.Infrastructure.Adapters.Media;

/// <summary>Photo imports of Catalog (CAT-016) download pictures into the Media library once per source key.</summary>
internal sealed class ProductPhotoSource(IMediaUrlImporter importer) : IProductPhotoSource
{
    public Task<IReadOnlyDictionary<string, int>> FindImportedAsync(IReadOnlyCollection<string> sourceKeys,
        CancellationToken cancellationToken) =>
        importer.FindImportedAsync(sourceKeys, cancellationToken);

    public async Task<ProductPhotoDownload> DownloadAsync(Uri url, string sourceKey, IReadOnlyList<string> folderPath,
        CancellationToken cancellationToken)
    {
        var import = await importer.ImportAsync(url, sourceKey, folderPath, cancellationToken);
        return new ProductPhotoDownload(import.MediaFileId, import.Error);
    }
}
