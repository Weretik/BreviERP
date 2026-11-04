namespace Media.Application.Abstractions;

/// <summary>Reads image dimensions and draws previews.</summary>
public interface IMediaImageProcessor
{
    /// <summary>Null when the bytes are not a readable image.</summary>
    MediaImageInfo? Read(byte[] content);

    /// <summary>A WebP preview whose longest side is <paramref name="maxSize"/>; null when the image is already that small or unreadable.</summary>
    byte[]? MakeThumbnail(byte[] content, int maxSize);
}

public sealed record MediaImageInfo(int Width, int Height);
