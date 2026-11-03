namespace Catalog.Application.Contracts.Media;

/// <summary>Reads photos of the Media module library for product photos, without a database relation.</summary>
public interface IProductMediaReader
{
    /// <summary>Public URLs of the photos that exist; missing ids are left out.</summary>
    Task<IReadOnlyDictionary<int, string>> GetUrlsAsync(IReadOnlyCollection<int> mediaFileIds, CancellationToken cancellationToken);
}
