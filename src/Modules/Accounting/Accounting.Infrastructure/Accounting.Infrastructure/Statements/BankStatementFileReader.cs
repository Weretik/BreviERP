using System.Globalization;
using System.IO.Compression;
using System.Text;
using Accounting.Application.Abstractions;
using ExcelDataReader;
using ExcelDataReader.Exceptions;

namespace Accounting.Infrastructure.Statements;

/// <summary>
/// Reads XLSX and XLS statements with ExcelDataReader and CSV ones as text (UTF-8 or UTF-16 with a byte order mark,
/// else UTF-8 when valid, else windows-1251; the separator is detected). Limits keep a small file from unpacking into
/// gigabytes or from holding more rows than a statement can.
/// </summary>
public sealed class BankStatementFileReader : IBankStatementFileReader
{
    private const int MaxSheets = 10;
    private const int MaxRows = 10_100;
    private const int MaxColumns = 129;
    private const long MaxUnpackedBytes = 64L * 1024 * 1024;

    static BankStatementFileReader() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    public Task<IReadOnlyList<BankSheet>> ReadAsync(Stream content, string fileName, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        try
        {
            return Task.FromResult(extension switch
            {
                ".csv" => ReadCsv(content),
                ".xlsx" => ReadWorkbook(CheckZip(content), cancellationToken),
                ".xls" => ReadWorkbook(content, cancellationToken),
                _ => throw new BankStatementFileException("Оберіть виписку у форматі XLSX, XLS або CSV."),
            });
        }
        catch (InvalidPasswordException)
        {
            throw new BankStatementFileException("Файл захищено паролем. Збережіть виписку без пароля і завантажте знову.");
        }
        catch (Exception error) when (error is ExcelReaderException or InvalidDataException or NotSupportedException
                                          or ArgumentException or InvalidOperationException
                                          or System.Xml.XmlException)
        {
            throw new BankStatementFileException(extension == ".csv"
                ? "Не вдалося прочитати CSV-файл."
                : "Не вдалося прочитати таблицю: файл пошкоджено або це не XLSX/XLS. Збережіть виписку в Excel ще раз.");
        }
    }

    /// <summary>An XLSX file is a zip archive; its parts together may not unpack to more than 64 MiB.</summary>
    private static Stream CheckZip(Stream content)
    {
        using (var zip = new ZipArchive(content, ZipArchiveMode.Read, leaveOpen: true))
        {
            long total = 0;
            foreach (var entry in zip.Entries)
            {
                total += entry.Length;
                if (total > MaxUnpackedBytes)
                    throw new BankStatementFileException("Таблиця розпаковується в завеликий файл. Збережіть лише виписку.");
            }
        }

        content.Position = 0;
        return content;
    }

    private static IReadOnlyList<BankSheet> ReadWorkbook(Stream content, CancellationToken cancellationToken)
    {
        using var reader = ExcelReaderFactory.CreateReader(content, new ExcelReaderConfiguration { LeaveOpen = true });
        return ReadSheets(reader, cancellationToken);
    }

    private static IReadOnlyList<BankSheet> ReadCsv(Stream content)
    {
        using var buffer = new MemoryStream();
        content.CopyTo(buffer);
        var text = Decode(buffer.GetBuffer().AsSpan(0, (int)buffer.Length));
        using var utf8 = new MemoryStream(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(text)).ToArray());
        using var reader = ExcelReaderFactory.CreateCsvReader(utf8, new ExcelReaderConfiguration
        {
            FallbackEncoding = Encoding.UTF8,
            AutodetectSeparators = [',', ';', '\t'],
            AnalyzeInitialCsvRows = 50,
            LeaveOpen = true,
        });
        return ReadSheets(reader, CancellationToken.None);
    }

    /// <summary>UTF-8 or UTF-16 by the byte order mark, else strict UTF-8, else windows-1251.</summary>
    private static string Decode(ReadOnlySpan<byte> bytes)
    {
        if (bytes.StartsWith(new byte[] { 0xEF, 0xBB, 0xBF })) return Encoding.UTF8.GetString(bytes[3..]);
        if (bytes.StartsWith(new byte[] { 0xFF, 0xFE })) return Encoding.Unicode.GetString(bytes[2..]);
        if (bytes.StartsWith(new byte[] { 0xFE, 0xFF })) return Encoding.BigEndianUnicode.GetString(bytes[2..]);
        try
        {
            return new UTF8Encoding(false, throwOnInvalidBytes: true).GetString(bytes);
        }
        catch (DecoderFallbackException)
        {
            return Encoding.GetEncoding(1251).GetString(bytes);
        }
    }

    private static IReadOnlyList<BankSheet> ReadSheets(IExcelDataReader reader, CancellationToken cancellationToken)
    {
        var sheets = new List<BankSheet>();
        do
        {
            if (sheets.Count == MaxSheets) break;
            var rows = new List<IReadOnlyList<BankCell>>();
            while (rows.Count < MaxRows && reader.Read())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var count = Math.Min(reader.FieldCount, MaxColumns);
                var cells = new BankCell[count];
                for (var index = 0; index < count; index++) cells[index] = Cell(reader.GetValue(index));
                var last = Array.FindLastIndex(cells, x => !x.IsEmpty);
                rows.Add(last < 0 ? [] : cells[..(last + 1)]);
            }

            sheets.Add(new BankSheet(reader.Name ?? string.Empty, rows));
        } while (reader.NextResult());

        return sheets;
    }

    private static BankCell Cell(object? value) => value switch
    {
        null => BankCell.Empty,
        DateTime date => new BankCell(date.TimeOfDay == TimeSpan.Zero
            ? date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)
            : date.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture), Date: date),
        double number when double.IsFinite(number) && Math.Abs(number) < 1e15 =>
            new BankCell(number.ToString(CultureInfo.InvariantCulture), Number: (decimal)number),
        TimeSpan time => new BankCell(time.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture)),
        bool flag => new BankCell(flag ? "true" : "false"),
        _ => new BankCell(Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty),
    };
}
