namespace Media.Domain.Files;

/// <summary>A photo stored on the CDN. It is created only after the CDN accepted the file, so every row has a working URL.</summary>
public sealed class MediaFile
{
    public const int FileNameMaxLength = 255;
    public const int AltMaxLength = 300;

    private MediaFile() { }

    public MediaFile(
        string fileName,
        int? folderId,
        string contentType,
        long sizeInBytes,
        string storageProvider,
        string storageKey,
        string publicUrl,
        DateTimeOffset now)
    {
        if (sizeInBytes <= 0) throw new ArgumentOutOfRangeException(nameof(sizeInBytes));
        FileName = MediaNames.Normalize(fileName, FileNameMaxLength);
        FolderId = folderId;
        ContentType = Required(contentType, nameof(contentType));
        SizeInBytes = sizeInBytes;
        StorageProvider = Required(storageProvider, nameof(storageProvider));
        StorageKey = Required(storageKey, nameof(storageKey));
        PublicUrl = Required(publicUrl, nameof(publicUrl));
        CreatedAt = now;
        UpdatedAt = now;
    }

    public int Id { get; private set; }

    /// <summary>The name staff see and search by; the original file name until renamed.</summary>
    public string FileName { get; private set; } = null!;

    /// <summary>Default description for the site; a product photo may override it.</summary>
    public string? Alt { get; private set; }

    /// <summary>Null when the photo lies in the library root.</summary>
    public int? FolderId { get; private set; }

    public string ContentType { get; private set; } = null!;
    public long SizeInBytes { get; private set; }
    public string StorageProvider { get; private set; } = null!;
    public string StorageKey { get; private set; } = null!;
    public string PublicUrl { get; private set; } = null!;
    public int? Width { get; private set; }
    public int? Height { get; private set; }

    /// <summary>Small WebP preview on the CDN; null when the photo is already small or could not be read.</summary>
    public string? ThumbnailStorageKey { get; private set; }

    public string? ThumbnailUrl { get; private set; }

    /// <summary>When dimensions and the preview were made; null for photos the nightly backfill has not reached yet.</summary>
    public DateTimeOffset? ImageProcessedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(string fileName, string? alt, int? folderId, DateTimeOffset now)
    {
        FileName = MediaNames.Normalize(fileName, FileNameMaxLength);
        Alt = NormalizeAlt(alt);
        FolderId = folderId;
        UpdatedAt = now;
    }

    public void MoveTo(int? folderId, DateTimeOffset now)
    {
        FolderId = folderId;
        UpdatedAt = now;
    }

    /// <summary>Records the image size and preview. Unreadable images keep null values but count as processed.</summary>
    public void SetImage(int? width, int? height, string? thumbnailStorageKey, string? thumbnailUrl, DateTimeOffset now)
    {
        if (width is <= 0 || height is <= 0) throw new ArgumentOutOfRangeException(nameof(width), "Image size must be positive.");
        if ((thumbnailStorageKey is null) != (thumbnailUrl is null))
            throw new ArgumentException("Thumbnail key and URL go together.", nameof(thumbnailUrl));
        Width = width;
        Height = height;
        ThumbnailStorageKey = thumbnailStorageKey?.Trim();
        ThumbnailUrl = thumbnailUrl?.Trim();
        ImageProcessedAt = now;
    }

    private static string? NormalizeAlt(string? alt)
    {
        if (string.IsNullOrWhiteSpace(alt)) return null;
        var trimmed = alt.Trim();
        if (trimmed.Length > AltMaxLength) throw new ArgumentException($"Alt must not exceed {AltMaxLength} characters.", nameof(alt));
        return trimmed;
    }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException($"{name} is required.", name) : value.Trim();
}
