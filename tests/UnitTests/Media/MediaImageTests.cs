using Media.Infrastructure.Images;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharp;
using Media.Application.Features.Files.Commands.DeleteMediaFile;
using Media.Application.Features.Files.Shared.Images;

namespace UnitTests.Media;

/// <summary>MED-001 stage 3: dimensions and WebP previews on upload and in the nightly backfill.</summary>
public sealed class MediaImageTests
{
    private static byte[] Png(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        bitmap.Erase(SKColors.CornflowerBlue);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    [Test]
    public void Processor_reads_size_and_makes_a_webp_preview_only_for_big_photos()
    {
        var processor = new SkiaMediaImageProcessor();
        var big = Png(1200, 800);

        var info = processor.Read(big);
        var preview = processor.MakeThumbnail(big, 480);
        using var codec = SKCodec.Create(new SKMemoryStream(preview!));

        Assert.Multiple(() =>
        {
            Assert.That(info, Is.EqualTo(new global::Media.Application.Abstractions.MediaImageInfo(1200, 800)));
            Assert.That(codec.EncodedFormat, Is.EqualTo(SKEncodedImageFormat.Webp));
            Assert.That((codec.Info.Width, codec.Info.Height), Is.EqualTo((480, 320)));
            Assert.That(processor.MakeThumbnail(Png(300, 200), 480), Is.Null, "small photos need no preview");
            Assert.That(processor.Read(MediaTestHarness.Jpeg), Is.Null, "a broken file is not an image");
            Assert.That(processor.MakeThumbnail(MediaTestHarness.Jpeg, 480), Is.Null);
        });
    }

    [TestCase(SKEncodedOrigin.RightTop, 2, 0)]
    [TestCase(SKEncodedOrigin.LeftBottom, 0, 3)]
    [TestCase(SKEncodedOrigin.BottomRight, 3, 2)]
    [TestCase(SKEncodedOrigin.LeftTop, 0, 0)]
    [TestCase(SKEncodedOrigin.RightBottom, 2, 3)]
    public void Phone_photo_turn_moves_the_top_left_pixel_where_exif_says(SKEncodedOrigin origin, int x, int y)
    {
        using var source = new SKBitmap(4, 3);
        source.Erase(SKColors.White);
        source.SetPixel(0, 0, SKColors.Red);

        using var upright = SkiaMediaImageProcessor.ApplyOrientation(source, origin);

        Assert.That(upright.GetPixel(x, y), Is.EqualTo(SKColors.Red));
    }

    [Test]
    public async Task Upload_saves_size_and_preview_and_delete_removes_both_cdn_files()
    {
        using var media = new MediaTestHarness();
        var file = await media.UploadAsync("coat.png", null, Png(1000, 1500), "image/png");

        Assert.Multiple(() =>
        {
            Assert.That((file.Width, file.Height), Is.EqualTo((1000, 1500)));
            Assert.That(file.ThumbnailUrl, Does.StartWith("https://cdn.example.test/library/thumbs/").And.EndWith(".webp"));
            Assert.That(media.Storage.Files, Has.Count.EqualTo(2));
        });

        var deleted = await new DeleteMediaFileHandler(media.Db, media.Storage, media.Usages)
            .Handle(new DeleteMediaFileCommand(file.Id), CancellationToken.None);
        Assert.That(deleted.IsSuccess, Is.True);
        Assert.That(media.Storage.Files, Is.Empty);
    }

    [Test]
    public async Task Preview_failure_keeps_the_upload_and_the_backfill_makes_it_later()
    {
        using var media = new MediaTestHarness();
        media.Storage.FailUploadsUnder = "/library/thumbs";
        var file = await media.UploadAsync("coat.png", null, Png(900, 600), "image/png");
        var broken = await media.UploadAsync("broken.jpg", null);
        Assert.That(file.ThumbnailUrl, Is.Null);
        Assert.That(await media.Db.Files.CountAsync(x => x.ImageProcessedAt == null), Is.EqualTo(1),
            "the broken file counts as processed: there is nothing to retry");

        media.Storage.FailUploadsUnder = null;
        var backfill = new MediaImageBackfill(media.Db, media.Storage, media.Images, media.Time, NullLogger<MediaImageBackfill>.Instance);
        var report = await backfill.RunAsync(10, CancellationToken.None);
        var stored = await media.Db.Files.AsNoTracking().SingleAsync(x => x.Id == file.Id);

        Assert.Multiple(() =>
        {
            Assert.That(report, Is.EqualTo(new MediaImageBackfillReport(1, 0, 0, 0)));
            Assert.That((stored.Width, stored.Height), Is.EqualTo((900, 600)));
            Assert.That(stored.ThumbnailUrl, Does.Contain("/library/thumbs/"));
            Assert.That(broken.Width, Is.Null);
        });
    }

    [Test]
    public async Task Backfill_logs_a_missing_original_and_retries_it_next_night()
    {
        using var media = new MediaTestHarness();
        media.Storage.FailUploadsUnder = "/library/thumbs";
        var file = await media.UploadAsync("coat.png", null, Png(900, 600), "image/png");
        media.Storage.FailUploadsUnder = null;
        media.Storage.Contents.Remove(file.PublicUrl);

        var report = await new MediaImageBackfill(media.Db, media.Storage, media.Images, media.Time, NullLogger<MediaImageBackfill>.Instance)
            .RunAsync(10, CancellationToken.None);

        Assert.That(report, Is.EqualTo(new MediaImageBackfillReport(0, 0, 1, 1)));
    }
}
