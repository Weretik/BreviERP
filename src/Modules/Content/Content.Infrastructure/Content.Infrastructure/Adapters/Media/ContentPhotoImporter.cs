using Content.Application.Abstractions;

using Media.Contracts.Imports;

namespace Content.Infrastructure.Adapters.Media;

/// <summary>
/// Brings old-site pictures of articles into the «Фото» library through the same importer as the photo transfer of
/// products, so both share the source keys and no picture is stored twice.
/// </summary>
internal sealed class ContentPhotoImporter(IMediaUrlImporter importer) : IContentPhotoImporter
{
    public Task<IReadOnlyDictionary<string, int>> FindImportedAsync(IReadOnlyCollection<string> sourceKeys,
        CancellationToken cancellationToken) =>
        importer.FindImportedAsync(sourceKeys, cancellationToken);

    public async Task<ContentPhotoDownload> DownloadAsync(Uri url, string sourceKey, IReadOnlyList<string> folderPath,
        CancellationToken cancellationToken)
    {
        var import = await importer.ImportAsync(url, sourceKey, folderPath, cancellationToken);
        return new ContentPhotoDownload(import.MediaFileId, import.Error);
    }
}
