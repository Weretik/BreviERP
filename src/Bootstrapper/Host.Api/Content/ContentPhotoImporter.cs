using Catalog.Application.Contracts.Media;
using Content.Application.Abstractions;

namespace Host.Api.Content;

/// <summary>
/// Brings old-site pictures of articles into the «Фото» library through the same downloader as the photo transfer of
/// products, so both share the source keys and no picture is stored twice.
/// </summary>
internal sealed class ContentPhotoImporter(IProductPhotoSource source) : IContentPhotoImporter
{
    public Task<IReadOnlyDictionary<string, int>> FindImportedAsync(IReadOnlyCollection<string> sourceKeys,
        CancellationToken cancellationToken) => source.FindImportedAsync(sourceKeys, cancellationToken);

    public async Task<ContentPhotoDownload> DownloadAsync(Uri url, string sourceKey, IReadOnlyList<string> folderPath,
        CancellationToken cancellationToken)
    {
        var download = await source.DownloadAsync(url, sourceKey, folderPath, cancellationToken);
        return new ContentPhotoDownload(download.MediaFileId, download.Error);
    }
}
