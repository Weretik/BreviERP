using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Categories;
using Accounting.Domain.Ledger;
using Accounting.Domain.Statements;
using BuildingBlocks.Domain.Errors;

namespace Accounting.Application.Statements.Reading;

/// <summary>
/// Turns the cells of a statement file into lines: finds the header, reads every row under it and checks it against
/// the chosen account. A problem of the whole file is a returned error under «File»; a problem of one row stays in
/// that line's error.
/// </summary>
internal static partial class BankStatementParser
{
    private const int HeaderSearchRows = 50;
    private const int MaxColumns = 128;

    private static readonly JsonSerializerOptions RawJson = new()
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
    };

    private static readonly string[] DateFormats =
    [
        "dd.MM.yyyy", "dd.MM.yyyy HH:mm", "dd.MM.yyyy HH:mm:ss", "d.M.yyyy", "d.M.yyyy H:mm", "d.M.yyyy H:mm:ss",
        "dd.MM.yy", "yyyy-MM-dd", "yyyy-MM-dd HH:mm", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-ddTHH:mm:ss", "dd/MM/yyyy",
        "dd/MM/yyyy HH:mm", "dd/MM/yyyy HH:mm:ss",
    ];

    private static readonly string[] TimeFormats = ["HH:mm:ss", "H:mm:ss", "HH:mm", "H:mm"];

    private static readonly string[] TotalWords = ["разом", "итого", "всього", "всего", "залишок", "остаток", "обороти"];

    public static DomainResult<IReadOnlyList<BankLineData>> Parse(IReadOnlyList<BankSheet> sheets,
        FinanceAccount account, string fileName)
    {
        var header = FindHeader(sheets);
        if (header.IsFailure) return header.Error!;
        var (sheet, headerRow, columns) = header.Value;
        if (OtherIban(account, fileName, sheet.Rows.Take(headerRow).SelectMany(x => x).Select(x => x.Text))
            is { } otherAccount) return otherAccount;

        var names = HeaderNames(sheet.Rows[headerRow]);
        var lines = new List<BankLineData>();
        for (var index = headerRow + 1; index < sheet.Rows.Count; index++)
        {
            var row = sheet.Rows[index];
            if (row.All(x => x.IsEmpty)) continue;
            var line = Read(row, index + 1, columns, names, account);
            if (line is null) continue;
            lines.Add(line);
            if (lines.Count > BankStatement.MaxLines)
                return FileError($"У виписці більше {BankStatement.MaxLines} рядків. Розділіть її на частини.");
        }

        return lines.Count == 0
            ? FileError("Під заголовком немає жодної операції.")
            : DomainResult<IReadOnlyList<BankLineData>>.Success(lines);
    }

    /// <summary>A problem of the whole file, shown under the file field.</summary>
    private static Error FileError(string message) => Error.Validation("Accounting.Statement.File", message, "File");

    private static DomainResult<(BankSheet Sheet, int Row, Dictionary<BankColumn, int> Columns)> FindHeader(
        IReadOnlyList<BankSheet> sheets)
    {
        foreach (var sheet in sheets)
            for (var index = 0; index < Math.Min(sheet.Rows.Count, HeaderSearchRows); index++)
            {
                var row = sheet.Rows[index];
                if (row.Count > MaxColumns)
                    return FileError($"У файлі більше {MaxColumns} колонок.");
                var found = row.Select((cell, column) => (Kind: BankHeaders.Of(cell.Text), Column: column))
                    .Where(x => x.Kind is not null).Select(x => (Kind: x.Kind!.Value, x.Column)).ToList();
                var kinds = found.Select(x => x.Kind).ToHashSet();
                if (!kinds.Contains(BankColumn.Date)
                    || !kinds.Overlaps([BankColumn.Amount, BankColumn.Debit, BankColumn.Credit])) continue;

                var twice = BankHeaders.Single.FirstOrDefault(kind => found.Count(x => x.Kind == kind) > 1, (BankColumn)(-1));
                if (twice != (BankColumn)(-1))
                    return FileError(
                        "У виписці кілька колонок суми або валюти; незрозуміло, яку брати. Залиште одну і завантажте знову.");
                return (sheet, index, found.GroupBy(x => x.Kind).ToDictionary(x => x.Key, x => x.First().Column));
            }

        return FileError(
            "Не знайдено заголовок виписки: потрібні колонки дати і суми (або дебету чи кредиту) у перших 50 рядках.");
    }

    /// <summary>An IBAN above the header or in the file name that is not the account's means a statement of another account.</summary>
    private static Error? OtherIban(FinanceAccount account, string fileName, IEnumerable<string> above)
    {
        if (account.Iban.Length == 0) return null;
        var found = above.Append(fileName).SelectMany(x => Iban().Matches(x.ToUpperInvariant()).Select(m => m.Value))
            .ToHashSet();
        var other = found.FirstOrDefault(x => x != account.Iban);
        return other is not null && !found.Contains(account.Iban)
            ? FileError(
                $"Це виписка іншого рахунку ({other}), а у «{account.Name}» IBAN {account.Iban}. Оберіть потрібний рахунок.")
            : null;
    }

    private static string[] HeaderNames(IReadOnlyList<BankCell> header) =>
        header.Select((cell, index) => BankHeaders.Normalize(cell.Text) is { Length: > 0 } name ? name : $"column_{index + 1}")
            .ToArray();

    private static BankLineData? Read(IReadOnlyList<BankCell> row, int rowNumber, Dictionary<BankColumn, int> columns,
        string[] names, FinanceAccount account)
    {
        BankCell Cell(BankColumn kind) =>
            columns.TryGetValue(kind, out var index) && index < row.Count ? row[index] : BankCell.Empty;
        string Text(BankColumn kind) => Cell(kind).Text.Trim();

        var errors = new List<string>();
        var date = ReadDate(Cell(BankColumn.Date));
        if (date is null)
        {
            var text = string.Join(' ', row.Select(x => x.Text)).ToLowerInvariant();
            if (TotalWords.Any(text.Contains)) return null;
            if (Cell(BankColumn.Date).IsEmpty && Cell(BankColumn.Amount).IsEmpty && Cell(BankColumn.Debit).IsEmpty
                && Cell(BankColumn.Credit).IsEmpty) return null;
            errors.Add("Некоректна дата");
        }

        var time = columns.ContainsKey(BankColumn.Time) ? ReadTime(Cell(BankColumn.Time)) : null;
        var (type, amount) = ReadMovement(Cell(BankColumn.Amount), Cell(BankColumn.Debit), Cell(BankColumn.Credit),
            Text(BankColumn.Direction), errors);

        var currency = Text(BankColumn.Currency).ToUpperInvariant();
        if (currency.Length == 0) currency = account.Currency;
        else if (currency is "ГРН" or "ГРН.") currency = "UAH";
        if (currency != account.Currency) errors.Add($"Валюта {currency} не збігається з валютою рахунку {account.Currency}");

        if (date is { } day)
        {
            if (day < LedgerCalendar.CutoverDate)
                errors.Add("Дата до 01.10.2026 — це «Домашня бухгалтерія», її виписки не імпортуються");
            else if (day < account.OpeningDate)
                errors.Add($"Рахунок ведеться з {account.OpeningDate:dd.MM.yyyy}");
        }

        var edrpou = Code(Text(BankColumn.Edrpou));
        var iban = NotLetterOrDigit().Replace(Text(BankColumn.Iban), string.Empty).ToUpperInvariant();
        var purposeRaw = Cell(BankColumn.Purpose).Text;
        var document = Text(BankColumn.Document);
        var reference = Text(BankColumn.Reference);
        var bankId = reference.Length == 0 ? null : reference;
        var error = string.Join("; ", errors);
        var fingerprint = error.Length == 0
            ? BankFingerprint.Of(account.Id.Value, date!.Value, time, amount!.Value, currency, type!.Value, iban, edrpou,
                document, bankId, purposeRaw)
            : null;

        return new BankLineData(rowNumber, date, time, type, amount, error.Length == 0 ? currency : null,
            Text(BankColumn.Counterparty), edrpou, iban, purposeRaw.Trim(), document, bankId, fingerprint, error,
            Raw(row, names));
    }

    /// <summary>
    /// The day of the date cell; a time written in the same cell is not the line's time (the old Calculator took the
    /// time only from a time column, and the fingerprint depends on it).
    /// </summary>
    private static DateOnly? ReadDate(BankCell cell)
    {
        if (cell.Date is { } typed) return DateOnly.FromDateTime(typed);
        if (cell.Number is { } serial and > 20_000 and < 80_000) return DateOnly.FromDateTime(DateTime.FromOADate((double)serial));

        var text = string.Join(' ', cell.Text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        return DateTime.TryParseExact(text, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? DateOnly.FromDateTime(parsed)
            : null;
    }

    private static TimeOnly? ReadTime(BankCell cell)
    {
        if (cell.Date is { } typed) return TimeOnly.FromDateTime(typed);
        if (cell.Number is { } fraction and >= 0 and < 1)
            return TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(Math.Round((double)fraction * 86_400)));
        return TimeOnly.TryParseExact(cell.Text.Trim(), TimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None,
            out var time)
            ? time
            : null;
    }

    private static (CategoryDirection? Type, long? Amount) ReadMovement(BankCell amountCell, BankCell debitCell,
        BankCell creditCell, string direction, List<string> errors)
    {
        var credit = Money(creditCell);
        var debit = Money(debitCell);
        var amount = Money(amountCell);
        if (credit.Invalid || debit.Invalid || amount.Invalid)
        {
            errors.Add("Некоректна сума");
            return (null, null);
        }

        CategoryDirection? type;
        decimal value;
        if (credit.Value is > 0) (type, value) = (CategoryDirection.Income, credit.Value.Value);
        else if (debit.Value is > 0) (type, value) = (CategoryDirection.Expense, debit.Value.Value);
        else if (amount.Value is { } signed && signed != 0)
        {
            value = Math.Abs(signed);
            var said = DirectionOf(direction);
            type = signed < 0 ? CategoryDirection.Expense : said ?? CategoryDirection.Income;
        }
        else
        {
            errors.Add("Некоректна сума");
            return (null, null);
        }

        var minor = value * 100;
        if (minor != decimal.Round(minor) || minor > MoneyMinor.Max)
        {
            errors.Add("Некоректна сума");
            return (null, null);
        }

        return (type, (long)minor);
    }

    private static CategoryDirection? DirectionOf(string text)
    {
        var value = text.ToLowerInvariant();
        if (value.Length == 0) return null;
        if (value.Contains("дебет") || value.Contains("списан") || value.Contains("витрат") || value.Contains("расход"))
            return CategoryDirection.Expense;
        if (value.Contains("кредит") || value.Contains("зарах") || value.Contains("дохід") || value.Contains("приход"))
            return CategoryDirection.Income;
        return null;
    }

    /// <summary>A sum written as a number or as text with spaces, a comma or a dot as the decimal sign, a minus or brackets.</summary>
    private static (decimal? Value, bool Invalid) Money(BankCell cell)
    {
        if (cell.Number is { } number) return (number, false);
        var text = cell.Text.Trim().Replace(" ", string.Empty).Replace("\u00A0", string.Empty).Replace("\u202F", string.Empty);
        if (text.Length == 0) return (null, false);
        var negative = text.StartsWith('(') && text.EndsWith(')');
        if (negative) text = text[1..^1];
        if (text.Contains(',') && !text.Contains('.')) text = text.Replace(',', '.');
        else text = text.Replace(",", string.Empty);
        return decimal.TryParse(text, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture, out var value)
            ? (negative ? -value : value, false)
            : (null, true);
    }

    /// <summary>ЄДРПОУ (8 digits) or ІПН (10 digits) as text; a code a spreadsheet stored as a number lost its leading zero.</summary>
    private static string Code(string text)
    {
        var digits = new string(text.Where(char.IsAsciiDigit).ToArray());
        return digits.Length switch
        {
            7 => "0" + digits,
            9 => "0" + digits,
            _ => digits,
        };
    }

    private static string Raw(IReadOnlyList<BankCell> row, string[] names)
    {
        var values = new Dictionary<string, string>();
        for (var index = 0; index < row.Count && index < names.Length; index++)
            values.TryAdd(names[index], row[index].Text);
        return JsonSerializer.Serialize(values, RawJson);
    }

    [GeneratedRegex(@"UA\d{27}")]
    private static partial Regex Iban();

    [GeneratedRegex(@"[^\p{L}\p{N}]")]
    private static partial Regex NotLetterOrDigit();
}
