using Ardalis.Result;

namespace Media.Contracts.Files;

/// <summary>Adds files to the «Фото» library for other modules.</summary>
public interface IMediaFileUploader
{
    /// <summary>Finds or creates the folder at the path of names from the library root and returns its id.</summary>
    Task<Result<int>> EnsureFolderAsync(IReadOnlyList<string> path, CancellationToken cancellationToken);

    Task<Result<MediaFileInfo>> UploadAsync(MediaUpload upload, CancellationToken cancellationToken);
}

/// <summary>One file to add; <paramref name="SourceKey"/> marks an imported photo so a repeated import skips it.</summary>
public sealed record MediaUpload(
    Stream Content,
    string FileName,
    string ContentType,
    long SizeInBytes,
    int? FolderId,
    string? SourceKey = null);
