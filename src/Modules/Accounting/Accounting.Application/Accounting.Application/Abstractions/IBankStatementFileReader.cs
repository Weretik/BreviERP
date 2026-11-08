namespace Accounting.Application.Abstractions;

/// <summary>
/// Reads the cells of a bank statement file (XLSX, XLS or CSV) for the statement import of stage 6; implemented in
/// Infrastructure. A file that cannot be read throws <see cref="BankStatementFileException"/> with a message for the
/// director.
/// </summary>
public interface IBankStatementFileReader
{
    Task<IReadOnlyList<BankSheet>> ReadAsync(Stream content, string fileName, CancellationToken cancellationToken);
}

/// <summary>One sheet of the file (a CSV file has one): its rows from the top, each a list of cells from the left.</summary>
public sealed record BankSheet(string Name, IReadOnlyList<IReadOnlyList<BankCell>> Rows);

/// <summary>
/// A cell: its text as shown, and the typed value when the spreadsheet stored a date or a number (a CSV file stores
/// only text).
/// </summary>
public readonly record struct BankCell(string Text, DateTime? Date = null, decimal? Number = null)
{
    public static readonly BankCell Empty = new(string.Empty);

    public bool IsEmpty => string.IsNullOrWhiteSpace(Text) && Date is null && Number is null;
}

/// <summary>The statement file cannot be used; the message is shown to the director as is.</summary>
public sealed class BankStatementFileException(string message) : Exception(message);
