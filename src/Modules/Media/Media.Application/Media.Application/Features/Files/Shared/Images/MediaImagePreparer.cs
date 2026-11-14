using Media.Application.Abstractions;

namespace Media.Application.Features.Files.Shared.Images;

/// <summary>Size and CDN preview of one photo, ready for <see cref="Domain.Files.MediaFile.SetImage"/>.</summary>
public sealed record PreparedMediaImage(int? Width, int? Height, string? ThumbnailStorageKey, string? ThumbnailUrl);

/// <summary>
/// Reads a photo's dimensions and uploads its WebP preview. Used on upload and by the nightly backfill.
/// An unreadable image gives empty values; a CDN failure throws so the photo stays unprocessed and is retried.
/// </summary>
public sealed class MediaImagePreparer(IMediaImageProcessor processor, IMediaStorage storage, MediaUploadSettings settings)
{
    public async Task<PreparedMediaImage> PrepareAsync(byte[] content, CancellationToken cancellationToken)
    {
        var info = processor.Read(content);
        if (info is null) return new PreparedMediaImage(null, null, null, null);

        var thumbnail = processor.MakeThumbnail(content, settings.ThumbnailMaxSize);
        if (thumbnail is null) return new PreparedMediaImage(info.Width, info.Height, null, null);

        var key = $"/{settings.ThumbnailFolder.Trim().Trim('/')}/{Guid.NewGuid():N}.webp";
        using var stream = new MemoryStream(thumbnail, writable: false);
        var url = await storage.UploadAsync(stream, key, "image/webp", cancellationToken);
        return new PreparedMediaImage(info.Width, info.Height, key, url);
    }
}
