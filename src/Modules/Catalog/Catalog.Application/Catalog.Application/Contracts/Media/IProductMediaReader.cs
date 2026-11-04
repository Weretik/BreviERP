namespace Catalog.Application.Contracts.Media;

/// <summary>Reads photos of the Media module library for product photos, without a database relation.</summary>
public interface IProductMediaReader
{
    /// <summary>Public URLs of the photos that exist; missing ids are left out.</summary>
    Task<IReadOnlyDictionary<int, string>> GetUrlsAsync(IReadOnlyCollection<int> mediaFileIds, CancellationToken cancellationToken);

    /// <summary>The photos that exist with their preview and name; missing ids are left out.</summary>
    Task<IReadOnlyDictionary<int, ProductMediaFile>> GetFilesAsync(IReadOnlyCollection<int> mediaFileIds, CancellationToken cancellationToken);
}

/// <param name="ThumbnailUrl">Small preview; null when the library has none, then the full photo is shown.</param>
public sealed record ProductMediaFile(int Id, string FileName, string PublicUrl, string? ThumbnailUrl);
