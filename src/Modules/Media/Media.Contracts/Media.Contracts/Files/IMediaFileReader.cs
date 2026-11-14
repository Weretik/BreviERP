namespace Media.Contracts.Files;

/// <summary>Reads photos of the «Фото» library for other modules.</summary>
public interface IMediaFileReader
{
    /// <summary>Photos by id; missing ids are left out.</summary>
    Task<IReadOnlyList<MediaFileInfo>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken);
}
