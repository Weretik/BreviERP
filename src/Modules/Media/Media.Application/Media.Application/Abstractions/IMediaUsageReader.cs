namespace Media.Application.Abstractions;

/// <summary>
/// Asks the modules that use photos (today Catalog products) where a photo is used.
/// Implemented in the host, so Media knows nothing about those modules.
/// </summary>
public interface IMediaUsageReader
{
    Task<IReadOnlyList<MediaUsage>> FindAsync(IReadOnlyCollection<int> fileIds, CancellationToken cancellationToken);
}

/// <param name="Area">Where the photo is used, for example «Товари».</param>
/// <param name="Title">What uses it, for example the product name.</param>
public sealed record MediaUsage(int FileId, string Area, int OwnerId, string Title);
