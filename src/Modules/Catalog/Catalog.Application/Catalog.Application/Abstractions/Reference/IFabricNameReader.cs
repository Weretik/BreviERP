namespace Catalog.Application.Abstractions.Reference;

/// <summary>Names of Reference fabrics for Catalog without a database relation.</summary>
public interface IFabricNameReader
{
    /// <summary>Every fabric name by its ID.</summary>
    Task<IReadOnlyDictionary<int, string>> ReadAsync(CancellationToken cancellationToken);
}
