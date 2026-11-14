namespace Catalog.Application.Abstractions.Reference;

/// <summary>Reads Reference «Типи і розділи» sections for Catalog without a database relation.</summary>
public interface IProductSectionReader
{
    /// <summary>The section, or null when it does not exist.</summary>
    Task<ProductSectionItem?> FindAsync(int id, CancellationToken cancellationToken);

    /// <summary>Every section of «Типи і розділи».</summary>
    Task<IReadOnlyList<ProductSectionItem>> ListAsync(CancellationToken cancellationToken);
}
