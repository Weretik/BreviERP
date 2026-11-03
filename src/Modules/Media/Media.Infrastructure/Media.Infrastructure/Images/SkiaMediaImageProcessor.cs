using Media.Application.Abstractions;
using SkiaSharp;

namespace Media.Infrastructure.Images;

/// <summary>Reads and resizes JPEG, PNG and WebP photos with SkiaSharp; previews are WebP.</summary>
internal sealed class SkiaMediaImageProcessor : IMediaImageProcessor
{
    private const int WebpQuality = 80;

    public MediaImageInfo? Read(byte[] content)
    {
        using var codec = CreateCodec(content);
        if (codec is null) return null;
        var size = OrientedSize(codec);
        return size.Width > 0 && size.Height > 0 ? new MediaImageInfo(size.Width, size.Height) : null;
    }

    public byte[]? MakeThumbnail(byte[] content, int maxSize)
    {
        if (maxSize <= 0) throw new ArgumentOutOfRangeException(nameof(maxSize));
        using var codec = CreateCodec(content);
        if (codec is null) return null;
        var oriented = OrientedSize(codec);
        if (Math.Max(oriented.Width, oriented.Height) <= maxSize) return null;

        using var decoded = SKBitmap.Decode(codec);
        if (decoded is null) return null;
        using var upright = ApplyOrientation(decoded, codec.EncodedOrigin);

        var scale = (float)maxSize / Math.Max(upright.Width, upright.Height);
        var target = new SKImageInfo(
            Math.Max(1, (int)Math.Round(upright.Width * scale)),
            Math.Max(1, (int)Math.Round(upright.Height * scale)),
            SKColorType.Rgba8888,
            SKAlphaType.Premul);
        using var resized = upright.Resize(target, new SKSamplingOptions(SKCubicResampler.Mitchell));
        if (resized is null) return null;

        using var image = SKImage.FromBitmap(resized);
        using var data = image.Encode(SKEncodedImageFormat.Webp, WebpQuality);
        return data?.ToArray();
    }

    private static SKCodec? CreateCodec(byte[] content) =>
        content.Length == 0 ? null : SKCodec.Create(new SKMemoryStream(content));

    /// <summary>Phone photos keep their pixels sideways and store the turn in EXIF; staff expect the turned size.</summary>
    private static SKSizeI OrientedSize(SKCodec codec) =>
        SwapsSides(codec.EncodedOrigin)
            ? new SKSizeI(codec.Info.Height, codec.Info.Width)
            : new SKSizeI(codec.Info.Width, codec.Info.Height);

    private static bool SwapsSides(SKEncodedOrigin origin) => origin is
        SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;

    /// <summary>Turns the decoded pixels upright; each EXIF origin maps a source point (x, y) to the upright canvas.</summary>
    internal static SKBitmap ApplyOrientation(SKBitmap source, SKEncodedOrigin origin)
    {
        float w = source.Width, h = source.Height;
        SKMatrix? matrix = origin switch
        {
            SKEncodedOrigin.TopRight => new SKMatrix(-1, 0, w, 0, 1, 0, 0, 0, 1),
            SKEncodedOrigin.BottomRight => new SKMatrix(-1, 0, w, 0, -1, h, 0, 0, 1),
            SKEncodedOrigin.BottomLeft => new SKMatrix(1, 0, 0, 0, -1, h, 0, 0, 1),
            SKEncodedOrigin.LeftTop => new SKMatrix(0, 1, 0, 1, 0, 0, 0, 0, 1),
            SKEncodedOrigin.RightTop => new SKMatrix(0, -1, h, 1, 0, 0, 0, 0, 1),
            SKEncodedOrigin.RightBottom => new SKMatrix(0, -1, h, -1, 0, w, 0, 0, 1),
            SKEncodedOrigin.LeftBottom => new SKMatrix(0, 1, 0, -1, 0, w, 0, 0, 1),
            _ => null,
        };
        if (matrix is null) return source.Copy();

        var swap = SwapsSides(origin);
        var result = new SKBitmap(swap ? source.Height : source.Width, swap ? source.Width : source.Height,
            source.ColorType, source.AlphaType);
        using var canvas = new SKCanvas(result);
        canvas.SetMatrix(matrix.Value);
        canvas.DrawBitmap(source, 0, 0);
        return result;
    }
}
