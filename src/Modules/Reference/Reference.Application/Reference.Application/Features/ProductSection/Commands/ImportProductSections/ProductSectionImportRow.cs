namespace Reference.Application.Features.ProductSection.Commands.ImportProductSections;

/// <summary>A row of the sheet «Типи і розділи»; <paramref name="Type"/> is <c>Sewing</c> or <c>Ppe</c>.</summary>
public sealed record ProductSectionImportRow(
    int RowNumber,
    string Type,
    string Name,
    bool IsActive,
    string? Comment);
