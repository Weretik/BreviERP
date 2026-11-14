using System.Globalization;
using System.Text;
using Accounting.Domain.Categories;
using Accounting.Domain.Movements;

namespace Accounting.Application.Features.Reports.Shared;

/// <summary>
/// «Завантажити CSV» of a report: UTF-8 with BOM for Excel, «;» between fields, CRLF, every field in quotes. Sums are
/// hryvnias with a decimal comma, dates dd.MM.yyyy. Text that Excel would read as a formula gets a leading apostrophe.
/// </summary>
public static class FinanceReportCsv
{
    private static readonly UTF8Encoding Utf8WithBom = new(encoderShouldEmitUTF8Identifier: true);

    public static string FileName(FinanceReportResponse report) =>
        $"zvit-{Slug(report.Kind)}-{report.From:yyyy-MM-dd}-{report.To:yyyy-MM-dd}.csv";

    public static byte[] Write(FinanceReportResponse report)
    {
        var columns = Columns(report.Kind);
        var text = new StringBuilder();
        Line(text, columns.Select(x => x.Header));
        foreach (var row in report.Rows) Line(text, columns.Select(x => x.Value(row)));
        var preamble = Utf8WithBom.GetPreamble();
        var body = Utf8WithBom.GetBytes(text.ToString());
        return [.. preamble, .. body];
    }

    private sealed record Column(string Header, Func<FinanceReportRowResponse, string> Value);

    private static IReadOnlyList<Column> Columns(FinanceReportKind kind)
    {
        Column currency = new("Валюта", x => x.Currency);
        Column operations = new("Операцій", x => x.Operations.ToString(CultureInfo.InvariantCulture));
        Column income = new("Доходи", x => Money(x.IncomeMinor));
        Column expense = new("Витрати", x => Money(x.ExpenseMinor));
        Column result = new("Різниця", x => Money(x.ResultMinor));
        Column category = new("Категорія", x => Text(x.Category));
        Column subcategory = new("Підкатегорія", x => Text(x.Subcategory));
        Column type = new("Тип", x => x.Type switch
        {
            CategoryDirection.Income => "Дохід",
            CategoryDirection.Expense => "Витрата",
            _ => string.Empty,
        });
        return kind switch
        {
            FinanceReportKind.Summary => [currency, operations, income, expense, result],
            FinanceReportKind.ExpenseStructure =>
                [category, subcategory, currency, operations, new Column("Сума витрат", x => Money(x.ExpenseMinor))],
            FinanceReportKind.Categories => [type, category, subcategory, currency, operations, income, expense, result],
            FinanceReportKind.Accounts =>
            [
                new Column("Рахунок", x => Text(x.Account)), currency, operations, income, expense,
                new Column("Вхідні перекази", x => Money(x.IncomingTransfersMinor ?? 0)),
                new Column("Вихідні перекази", x => Money(x.OutgoingTransfersMinor ?? 0)),
                new Column("Чистий рух", x => Money(x.ResultMinor)),
            ],
            FinanceReportKind.Counterparties =>
                [new Column("Контрагент", x => Text(x.Counterparty)), currency, operations, income, expense, result],
            _ =>
            [
                new Column("Дата", x => x.Date?.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) ?? string.Empty),
                new Column("Рахунок", x => Text(x.Account)), type, category, subcategory,
                new Column("Контрагент", x => Text(x.Counterparty)),
                new Column("Сума", x => Money(x.AmountMinor ?? 0)), currency,
                new Column("Примітка", x => Text(x.Note)),
                new Column("Джерело", x => x.Source switch
                {
                    MovementSource.Manual => "Вручну",
                    MovementSource.BankImport => "Виписка банку",
                    MovementSource.HomeAccountingImport => "Домашня бухгалтерія",
                    _ => string.Empty,
                }),
            ],
        };
    }

    private static void Line(StringBuilder text, IEnumerable<string> fields)
    {
        text.AppendJoin(';', fields.Select(x => $"\"{x.Replace("\"", "\"\"", StringComparison.Ordinal)}\""));
        text.Append("\r\n");
    }

    /// <summary>Kopecks as hryvnias: <c>-1234,50</c>.</summary>
    public static string Money(long minor)
    {
        var sign = minor < 0 ? "-" : string.Empty;
        var value = Math.Abs(minor);
        return string.Create(CultureInfo.InvariantCulture, $"{sign}{value / 100},{value % 100:00}");
    }

    private static string Text(string? value) =>
        value is { Length: > 0 } && "=+-@\t\r".Contains(value[0], StringComparison.Ordinal) ? "'" + value : value ?? string.Empty;

    private static string Slug(FinanceReportKind kind) => kind switch
    {
        FinanceReportKind.Summary => "zvedenyi",
        FinanceReportKind.ExpenseStructure => "struktura-vytrat",
        FinanceReportKind.Categories => "kategorii",
        FinanceReportKind.Accounts => "rakhunky",
        FinanceReportKind.Counterparties => "kontrahenty",
        _ => "operatsii",
    };
}
