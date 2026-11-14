namespace Purchasing.Application.Abstractions.Sources;

/// <summary>Reads the whole purchase sheet (header and data rows) from Google Sheets; never writes to it.</summary>
public interface IPurchaseSheetReader
{
    /// <exception cref="PurchaseSheetUnavailableException">Google cannot be reached or refuses access.</exception>
    /// <exception cref="Purchasing.Domain.Ingestion.Errors.PurchaseSourceException">The sheet is not the expected one.</exception>
    Task<PurchaseSheetContent> ReadAsync(CancellationToken cancellationToken);
}

/// <summary>Data rows from row 2 in sheet order; <see cref="FirstRowNumber"/> is the sheet row of Rows[0].</summary>
public sealed record PurchaseSheetContent(string SpreadsheetId, long SheetId, IReadOnlyList<IReadOnlyList<string?>> Rows)
{
    public const int FirstRowNumber = 2;
}

/// <summary>Google Sheets cannot be read now; the register is left as it was.</summary>
public sealed class PurchaseSheetUnavailableException(string message, Exception? inner = null) : Exception(message, inner);
