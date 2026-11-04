using Media.Application.Files.BackfillImages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Host.Seed.Jobs.Sync;

/// <summary>
/// Photo previews (MED-001 stage 3): makes dimensions and WebP previews for library photos that have none yet,
/// such as photos uploaded before previews existed. At most <see cref="BatchSize"/> photos per night; on a normal
/// night it finds nothing.
/// </summary>
internal sealed class MediaThumbnailsSyncTask : ISyncTask
{
    public const int BatchSize = 500;

    public string Name => "media-thumbnails";

    public async Task RunAsync(IServiceProvider services, ILogger logger, CancellationToken cancellationToken)
    {
        var report = await services.GetRequiredService<MediaImageBackfill>().RunAsync(BatchSize, cancellationToken);
        logger.LogInformation(
            "Processed {Processed} photos ({WithoutPreview} small or unreadable, no preview), failed {Failed}, left for next runs {Remaining}",
            report.Processed, report.WithoutPreview, report.Failed, report.Remaining);
    }
}
