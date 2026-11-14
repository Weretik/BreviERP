namespace Catalog.Application.Abstractions.Reference;

/// <param name="Type">"Sewing" or "Ppe", as Reference returns it.</param>
public sealed record ProductSectionItem(int Id, string Type, string Name, bool IsActive);
