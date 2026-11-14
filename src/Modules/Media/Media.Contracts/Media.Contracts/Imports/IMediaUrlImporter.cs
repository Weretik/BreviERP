namespace Media.Contracts.Imports;

/// <summary>Downloads pictures from a web address into the «Фото» library, once per source key.</summary>
public interface IMediaUrlImporter
{
    /// <summary>Already imported photos by source key; unknown keys are left out.</summary>
    Task<IReadOnlyDictionary<string, int>> FindImportedAsync(IReadOnlyCollection<string> sourceKeys,
        CancellationToken cancellationToken);

    Task<MediaUrlImport> ImportAsync(Uri url, string sourceKey, IReadOnlyList<string> folderPath,
        CancellationToken cancellationToken);
}

/// <summary>The library photo id, or the reason it was not imported.</summary>
public sealed record MediaUrlImport(int? MediaFileId, string? Error);
