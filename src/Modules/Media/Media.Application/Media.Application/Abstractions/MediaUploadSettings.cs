namespace Media.Application.Abstractions;

/// <summary>Upload limits from configuration section <c>MediaUpload</c>.</summary>
public sealed class MediaUploadSettings
{
    public const string Section = "MediaUpload";

    /// <summary>Largest accepted photo in bytes.</summary>
    public long MaxFileSizeBytes { get; set; }

    /// <summary>CDN folder for new originals, for example <c>/library/original</c>.</summary>
    public string BaseFolder { get; set; } = string.Empty;

    /// <summary>CDN folder for WebP previews, for example <c>/library/thumbs</c>.</summary>
    public string ThumbnailFolder { get; set; } = string.Empty;

    /// <summary>Longest side of a preview in pixels; smaller photos get no preview.</summary>
    public int ThumbnailMaxSize { get; set; } = 480;
}
