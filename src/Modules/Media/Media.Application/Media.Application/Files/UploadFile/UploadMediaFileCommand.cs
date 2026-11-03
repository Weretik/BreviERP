using Media.Application.Contracts;

namespace Media.Application.Files.UploadFile;

/// <summary>Uploads one photo to the CDN and adds it to the folder (null = library root).</summary>
public sealed record UploadMediaFileCommand(Stream Content, string FileName, string ContentType, long SizeInBytes, int? FolderId)
    : ICommand<Result<MediaFileResponse>>
{
    /// <summary>The file name without any folder path a browser or OS may send.</summary>
    public string DisplayName => FileName is null ? string.Empty : FileName[(FileName.LastIndexOfAny(['/', '\\']) + 1)..];

    public static IReadOnlyDictionary<string, string> Extensions { get; } = new Dictionary<string, string>
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp",
    };
}
