namespace Media.Application.Abstractions;

/// <summary>Upload limits from configuration section <c>MediaUpload</c>.</summary>
public sealed class MediaUploadSettings
{
    public const string Section = "MediaUpload";

    /// <summary>Largest accepted photo in bytes.</summary>
    public long MaxFileSizeBytes { get; set; }

    /// <summary>CDN folder for new originals, for example <c>/library/original</c>.</summary>
    public string BaseFolder { get; set; } = string.Empty;
}
