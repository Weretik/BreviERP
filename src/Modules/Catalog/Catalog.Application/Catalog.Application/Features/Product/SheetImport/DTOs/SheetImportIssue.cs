namespace Catalog.Application.Features.Product.SheetImport.DTOs;

/// <summary>A problem or warning of one sheet row, in Ukrainian.</summary>
/// <param name="Scope">sewing or ppe.</param>
/// <param name="RowNumber">Sheet row; null for a stored product that is not in the sheet.</param>
public sealed record SheetImportIssue(string Scope, int? RowNumber, string? Code, string Message);
