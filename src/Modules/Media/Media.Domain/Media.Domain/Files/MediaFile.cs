using BuildingBlocks.Domain.Results;

namespace Media.Domain.Files;

/// <summary>A photo stored on the CDN. It is created only after the CDN accepted the file, so every row has a working URL.</summary>
public sealed class MediaFile
{
    public const int FileNameMaxLength = 255;
    public const int AltMaxLength = 300;
    public const int SourceKeyMaxLength = 500;

    private MediaFile() { }

    /// <summary>
    /// A new photo. The file name is the staff's input and is checked as a rule; size, type and storage values come from
    /// the server and are guarded.
    /// </summary>
    public static DomainResult<MediaFile> Create(
        string fileName,
        int? folderId,
        string contentType,
        long sizeInBytes,
        string storageProvider,
        string storageKey,
        string publicUrl,
        DateTimeOffset now,
        string? sourceKey = null)
    {
        var name = MediaNames.Normalize(fileName, FileNameMaxLength, "fileName");
        if (name.IsFailure) return name.Error!;
        return new MediaFile(name.Value, folderId, contentType, sizeInBytes, storageProvider, storageKey, publicUrl, now, sourceKey);
    }

    private MediaFile(
        string fileName,
        int? folderId,
        string contentType,
        long sizeInBytes,
        string storageProvider,
        string storageKey,
        string publicUrl,
        DateTimeOffset now,
        string? sourceKey = null)
    {
        if (sizeInBytes <= 0) throw new ArgumentOutOfRangeException(nameof(sizeInBytes));
        FileName = fileName;
        FolderId = folderId;
        ContentType = Required(contentType, nameof(contentType));
        SizeInBytes = sizeInBytes;
        StorageProvider = Required(storageProvider, nameof(storageProvider));
        StorageKey = Required(storageKey, nameof(storageKey));
        PublicUrl = Required(publicUrl, nameof(publicUrl));
        SourceKey = NormalizeSourceKey(sourceKey);
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
    /// <summary>Where an imported photo came from (for example «old-site:catalog/a.jpg»), so a repeated import skips it; null for uploads.</summary>
    public string? SourceKey { get; private set; }

    public int? Width { get; private set; }
    public int? Height { get; private set; }

    /// <summary>Small WebP preview on the CDN; null when the photo is already small or could not be read.</summary>
    public string? ThumbnailStorageKey { get; private set; }

    public string? ThumbnailUrl { get; private set; }

    /// <summary>When dimensions and the preview were made; null for photos the nightly backfill has not reached yet.</summary>
    public DateTimeOffset? ImageProcessedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public DomainResult Update(string fileName, string? alt, int? folderId, DateTimeOffset now)
    {
        var name = MediaNames.Normalize(fileName, FileNameMaxLength, "fileName");
        if (name.IsFailure) return name.Error!;
        var trimmedAlt = string.IsNullOrWhiteSpace(alt) ? null : alt.Trim();
        if (trimmedAlt?.Length > AltMaxLength) return MediaErrors.AltTooLong(AltMaxLength);
        FileName = name.Value;
        Alt = trimmedAlt;
        FolderId = folderId;
        UpdatedAt = now;
        return DomainResult.Success();
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

    private static string? NormalizeSourceKey(string? sourceKey)
    {
        if (string.IsNullOrWhiteSpace(sourceKey)) return null;
        var trimmed = sourceKey.Trim();
        if (trimmed.Length > SourceKeyMaxLength) throw new ArgumentException($"Source key must not exceed {SourceKeyMaxLength} characters.", nameof(sourceKey));
        return trimmed;
    }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException($"{name} is required.", name) : value.Trim();
}
