using Media.Application.Abstractions;

namespace Media.Application.Features.Files.Shared.Images;

public sealed record MediaImageBackfillReport(int Processed, int WithoutPreview, int Failed, int Remaining);

/// <summary>
/// Nightly step: fills dimensions and previews for photos uploaded before previews existed or whose preview failed.
/// Works through at most <c>limit</c> photos per run, oldest first; a failed photo is logged and retried next night.
/// </summary>
public sealed class MediaImageBackfill(
    IMediaDbContext db,
    IMediaStorage storage,
    MediaImagePreparer preparer,
    TimeProvider clock,
    ILogger<MediaImageBackfill> logger)
{
    public async Task<MediaImageBackfillReport> RunAsync(int limit, CancellationToken cancellationToken)
    {
        var ids = await db.Files.Where(x => x.ImageProcessedAt == null)
            .OrderBy(x => x.Id).Select(x => x.Id).Take(limit).ToListAsync(cancellationToken);

        int processed = 0, withoutPreview = 0, failed = 0;
        foreach (var id in ids)
        {
            var file = await db.Files.FirstAsync(x => x.Id == id, cancellationToken);
            try
            {
                var content = await storage.DownloadAsync(file.PublicUrl, cancellationToken);
                var image = await preparer.PrepareAsync(content, cancellationToken);
                file.SetImage(image.Width, image.Height, image.ThumbnailStorageKey, image.ThumbnailUrl, clock.GetUtcNow());
                await db.SaveChangesAsync(cancellationToken);
                processed++;
                if (image.ThumbnailUrl is null) withoutPreview++;
            }
            catch (Exception error) when (error is not OperationCanceledException)
            {
                failed++;
                logger.LogWarning("Photo {Id} «{Name}»: no preview yet: {ErrorType}: {Reason}",
                    file.Id, file.FileName, error.GetType().Name, error.Message);
            }
        }

        var remaining = await db.Files.CountAsync(x => x.ImageProcessedAt == null, cancellationToken);
        return new MediaImageBackfillReport(processed, withoutPreview, failed, remaining);
    }
}
