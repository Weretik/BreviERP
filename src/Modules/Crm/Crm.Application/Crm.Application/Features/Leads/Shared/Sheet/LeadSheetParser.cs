using Crm.Domain.Leads;
using Crm.Domain.Leads.Errors;
using Crm.Domain.Leads.ValueObjects;

namespace Crm.Application.Features.Leads.Shared.Sheet;

/// <summary>
/// Turns the rows of the leads tab into <see cref="LeadSheetRow"/> (LEAD-001 R-02). Columns are found by their header,
/// so the AI may add or move columns; «ID» and «Предприятие» are required. A row without a whole-number ID or a company
/// is skipped (an empty line or a note). Rows are matched by the company name (<see cref="Lead.KeyOf"/>), not by the ID
/// the AI may repeat (owner, 2026-11-08); a company written twice keeps its first row and the repeats are reported.
/// </summary>
public static class LeadSheetParser
{
    public const int MaxRows = 20000;

    private static readonly string[] DateFormats = ["yyyy-MM-dd", "dd.MM.yyyy", "d.M.yyyy", "dd.MM.yy", "d.M.yy", "dd/MM/yyyy"];

    public static DomainResult<LeadSheetParseResult> Parse(IReadOnlyList<IReadOnlyList<string?>> rows)
    {
        if (rows.Count == 0) return LeadErrors.SheetFormat("Аркуш лідів порожній: немає рядка із заголовками.");
        if (rows.Count > MaxRows + 1)
            return LeadErrors.SheetFormat($"В аркуші лідів понад {MaxRows} рядків.");

        var columns = Columns(rows[0]);
        if (!columns.ContainsKey(Column.Id) || !columns.ContainsKey(Column.Company))
            return LeadErrors.SheetFormat("В аркуші лідів немає колонки «ID» або «Предприятие».");

        var result = new List<LeadSheetRow>();
        var lines = new Dictionary<string, List<int>>(StringComparer.Ordinal);
        var skipped = 0;
        for (var i = 1; i < rows.Count; i++)
        {
            var row = rows[i];
            string? Cell(Column column) =>
                columns.TryGetValue(column, out var index) && index < row.Count && !string.IsNullOrWhiteSpace(row[index])
                    ? row[index]!.Trim()
                    : null;

            var idText = Cell(Column.Id);
            var company = Cell(Column.Company);
            if (!int.TryParse(idText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) || id <= 0 ||
                company is null)
            {
                if (idText is not null || company is not null) skipped++;
                continue;
            }

            var key = Lead.KeyOf(company);
            if (key.Length == 0)
            {
                skipped++;
                continue;
            }

            var line = i + 1;
            if (lines.TryGetValue(key, out var taken))
            {
                taken.Add(line);
                continue;
            }

            lines[key] = [line];

            result.Add(new LeadSheetRow(
                id,
                Date(Cell(Column.AddedOn)),
                Cell(Column.Category)?.ToUpperInvariant(),
                company,
                Cell(Column.Region),
                Cell(Column.Industry),
                Cell(Column.Scale),
                Cell(Column.Signal),
                Date(Cell(Column.SignalDate)),
                Cell(Column.FactOrPlan),
                Cell(Column.SignalUrl),
                Cell(Column.VolumeReason),
                Cell(Column.Offer),
                Cell(Column.Website),
                Cell(Column.Contacts),
                Cell(Column.WhomToCall),
                Cell(Column.TenderCheck),
                Cell(Column.CallReason),
                Cell(Column.Reliability),
                Cell(Column.Email),
                Notes(Cell(Column.Owner), Cell(Column.Status), Cell(Column.CallResult), Cell(Column.NextContact),
                    Cell(Column.Comment)),
                Cell(Column.Owner),
                Cell(Column.CallResult),
                Cell(Column.Comment),
                Date(Cell(Column.NextContact))));
        }

        var duplicates = result
            .Where(x => lines[Lead.KeyOf(x.Company)].Count > 1)
            .Select(x => new LeadSheetDuplicate(x.Company, lines[Lead.KeyOf(x.Company)]))
            .ToList();
        return new LeadSheetParseResult(result, skipped, duplicates);
    }

    public static DateOnly? Date(string? text) =>
        text is not null && DateOnly.TryParseExact(text, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None,
            out var date)
            ? date
            : null;

    /// <summary>The manager columns of the sheet as they were before the admin, kept once on a new lead.</summary>
    private static string? Notes(string? owner, string? status, string? result, string? next, string? comment)
    {
        var lines = new List<string>();
        if (owner is not null) lines.Add($"Хто взяв: {owner}");
        if (status is not null && !status.Equals("Свободно", StringComparison.OrdinalIgnoreCase)) lines.Add($"Статус: {status}");
        if (result is not null) lines.Add($"Результат дзвінка: {result}");
        if (next is not null) lines.Add($"Наступний контакт: {next}");
        if (comment is not null) lines.Add($"Коментар: {comment}");
        return lines.Count == 0 ? null : string.Join("\n", lines);
    }

    private static Dictionary<Column, int> Columns(IReadOnlyList<string?> header)
    {
        var map = new Dictionary<Column, int>();
        for (var i = 0; i < header.Count; i++)
            if (Headers.TryGetValue(Normalize(header[i]), out var column)) map.TryAdd(column, i);
        return map;
    }

    private static string Normalize(string? text) =>
        string.Join(' ', (text ?? "").Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));

    private static readonly Dictionary<string, Column> Headers = new(StringComparer.Ordinal)
    {
        ["id"] = Column.Id,
        ["дата добавления"] = Column.AddedOn,
        ["кто взял в работу"] = Column.Owner,
        ["статус"] = Column.Status,
        ["категория"] = Column.Category,
        ["предприятие"] = Column.Company,
        ["регион"] = Column.Region,
        ["отрасль"] = Column.Industry,
        ["масштаб / персонал"] = Column.Scale,
        ["сигнал"] = Column.Signal,
        ["дата сигнала"] = Column.SignalDate,
        ["факт или план"] = Column.FactOrPlan,
        ["ссылка на сигнал"] = Column.SignalUrl,
        ["почему возможны 50+ единиц"] = Column.VolumeReason,
        ["что предложить"] = Column.Offer,
        ["официальный сайт"] = Column.Website,
        ["открытый контакт"] = Column.Contacts,
        ["кому звонить"] = Column.WhomToCall,
        ["проверка тендеров"] = Column.TenderCheck,
        ["повод для звонка"] = Column.CallReason,
        ["достоверность"] = Column.Reliability,
        ["результат звонка"] = Column.CallResult,
        ["следующий контакт"] = Column.NextContact,
        ["комментарий"] = Column.Comment,
        ["почта"] = Column.Email,
        ["email"] = Column.Email,
    };

    private enum Column
    {
        Id, AddedOn, Owner, Status, Category, Company, Region, Industry, Scale, Signal, SignalDate, FactOrPlan, SignalUrl,
        VolumeReason, Offer, Website, Contacts, WhomToCall, TenderCheck, CallReason, Reliability, CallResult, NextContact,
        Comment, Email,
    }
}

/// <param name="Skipped">Rows with text but without a whole-number ID or a company name.</param>
/// <param name="Duplicates">Companies written on more than one row; only the first of those rows is applied.</param>
public sealed record LeadSheetParseResult(
    IReadOnlyList<LeadSheetRow> Rows,
    int Skipped,
    IReadOnlyList<LeadSheetDuplicate> Duplicates)
{
    /// <summary>The rows left out as repeats of a company.</summary>
    public int Repeats => Duplicates.Sum(x => x.Lines.Count - 1);

    /// <summary>««Ромашка» — рядки 121, 122» for the statistics page, or null without repeats.</summary>
    public string? DuplicatesWarning => Duplicates.Count == 0
        ? null
        : "Підприємство записано в таблиці кілька разів, взято перший рядок: " +
          string.Join("; ", Duplicates.Select(x => $"«{x.Company}» — рядки {string.Join(", ", x.Lines)}")) + ".";
}

/// <summary>A company the sheet has on several rows, with those row numbers; the first one is loaded.</summary>
public sealed record LeadSheetDuplicate(string Company, IReadOnlyList<int> Lines);
