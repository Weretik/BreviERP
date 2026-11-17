namespace Media.Contracts.Files;

/// <summary>A library photo that a record of another module holds.</summary>
/// <param name="Area">The section shown in the library, for example «Товари».</param>
public sealed record MediaFileUsage(int MediaFileId, string Area, long OwnerId, string Title);

/// <summary>
/// Implemented by every module that keeps library photos, so the library shows where a photo is used and does not
/// delete it. Media calls every registered source and knows none of the modules (ADR-0019).
/// </summary>
public interface IMediaFileUsageSource
{
    Task<IReadOnlyList<MediaFileUsage>> UsagesAsync(IReadOnlyCollection<int> mediaFileIds,
        CancellationToken cancellationToken);
}
