namespace Catalog.Application.Abstractions.Pricing.PpeSupplierPrices;

/// <summary>A problem found during an import, shown to the staff member with its reason.</summary>
public sealed record PpePriceImportProblem(string? ProductCode, string? ProductName, string Message);
