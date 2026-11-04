namespace Catalog.Application.Contracts.Media;

/// <summary>
/// Brings photos from outside into the Media library for product photo imports (CAT-016). Each photo is marked with a
/// source key, so a repeated import finds it instead of downloading it again.
/// </summary>
public interface IProductPhotoSource
{
    /// <summary>Library photo ids of the source keys already imported; unknown keys are left out.</summary>
    Task<IReadOnlyDictionary<string, int>> FindImportedAsync(IReadOnlyCollection<string> sourceKeys, CancellationToken cancellationToken);

    /// <summary>Downloads the photo and adds it to the library folder at <paramref name="folderPath"/> (names from the root).</summary>
    Task<ProductPhotoDownload> DownloadAsync(Uri url, string sourceKey, IReadOnlyList<string> folderPath, CancellationToken cancellationToken);
}

/// <param name="MediaFileId">The library photo; null when it failed, then <paramref name="Error"/> says why.</param>
public sealed record ProductPhotoDownload(int? MediaFileId, string? Error);
