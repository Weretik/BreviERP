namespace Content.Application.Abstractions;

/// <summary>
/// Brings pictures from outside into the «Фото» library for the old-site transfer of articles (CNT-004 SC-06).
/// Implemented in the host over the Media module. Each photo is marked with a source key, so a repeated transfer
/// finds it instead of downloading it again.
/// </summary>
public interface IContentPhotoImporter
{
    /// <summary>Library photo ids of the source keys already transferred; unknown keys are left out.</summary>
    Task<IReadOnlyDictionary<string, int>> FindImportedAsync(IReadOnlyCollection<string> sourceKeys,
        CancellationToken cancellationToken);

    /// <summary>Downloads the picture and adds it to the library folder at <paramref name="folderPath"/> (names from the root).</summary>
    Task<ContentPhotoDownload> DownloadAsync(Uri url, string sourceKey, IReadOnlyList<string> folderPath,
        CancellationToken cancellationToken);
}

/// <param name="PhotoId">The library photo; null when it failed, then <paramref name="Error"/> says why.</param>
public sealed record ContentPhotoDownload(int? PhotoId, string? Error);
