namespace Reference.Contracts.ProductSections;

/// <summary>A row of «Типи і розділи» (<c>Type</c> is <c>Sewing</c> or <c>Ppe</c>) as other modules see it.</summary>
public sealed record ProductSectionInfo(
    int Id, string Type, string Name, bool IsActive);
