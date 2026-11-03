namespace Catalog.Application.Contracts.Reference;

/// <summary>Reads Reference «Типи і розділи» sections for Catalog without a database relation.</summary>
public interface IProductSectionReader
{
    /// <summary>The section, or null when it does not exist.</summary>
    Task<ProductSectionItem?> FindAsync(int id, CancellationToken cancellationToken);
}

/// <param name="Type">"Sewing" or "Ppe", as Reference returns it.</param>
public sealed record ProductSectionItem(int Id, string Type, string Name, bool IsActive);
