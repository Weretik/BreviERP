using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Accounting.Domain.Audit;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using static UnitTests.Accounting.FinanceMovementFixture;
using Accounting.Application.Features.Audit.Queries.GetFinanceAudit;
using Accounting.Application.Features.Counterparties.Commands.CreateFinanceCounterparty;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Export.Queries.GetFinanceExport;
using Accounting.Application.Features.Reports.Queries.GetFinanceReport;
using Accounting.Application.Features.Reports.Shared;
using Accounting.Application.Features.Transactions.Commands.VoidFinanceTransaction;
using Accounting.Application.Features.Transactions.Shared;
using Accounting.Application.Features.Reports.Queries.GetFinanceReport.Results;

namespace UnitTests.Accounting.Reports;

/// <summary>«Звіти», «Журнал змін» and «Резервна копія» (ACC-001 stage 8).</summary>
[TestFixture]
public sealed class FinanceReportTests
{
    private static readonly DateOnly From = new(2026, 10, 1);
    private static readonly DateOnly To = new(2026, 10, 31);

    [Test]
    public async Task Reports_count_only_active_income_and_expense_of_the_period()
    {
        using var harness = new AccountingTestHarness();
        var (bank, cash) = await Books(harness);

        var summary = await Report(harness, FinanceReportKind.Summary);
        var structure = await Report(harness, FinanceReportKind.ExpenseStructure);
        var categories = await Report(harness, FinanceReportKind.Categories);
        var accounts = await Report(harness, FinanceReportKind.Accounts);
        var onlyCash = await Report(harness, FinanceReportKind.Accounts, accountId: cash);

        Assert.Multiple(() =>
        {
            Assert.That(summary.Totals.Single(), Is.EqualTo(new FinanceReportTotalResponse("UAH", 4, 900_00, 300_00, 600_00)),
                "the voided expense and the November income are left out");
            Assert.That(summary.Rows.Single().ResultMinor, Is.EqualTo(600_00));
            Assert.That(structure.Rows.Select(x => (x.RowType, x.Category, x.Subcategory, x.ExpenseMinor)), Is.EqualTo(new[]
            {
                (FinanceReportRowType.Group, "Матеріали", (string?)null, 220_00L),
                (FinanceReportRowType.Subcategory, "Матеріали", "Тканини", 120_00L),
                (FinanceReportRowType.Subcategory, "Матеріали", FinanceReports.WithoutSubcategory, 100_00L),
                (FinanceReportRowType.Group, "Оренда", null, 80_00L),
            }), "a root holds its subcategories; direct spending on it is «Без підкатегорії»");
            Assert.That(categories.Rows.Select(x => (x.Type, x.Category, x.Subcategory, x.IncomeMinor, x.ExpenseMinor)),
                Is.EqualTo(new[]
                {
                    (CategoryDirection.Income, "Продажі", (string?)null, 900_00L, 0L),
                    (CategoryDirection.Expense, "Матеріали", null, 0L, 100_00L),
                    (CategoryDirection.Expense, "Матеріали", "Тканини", 0L, 120_00L),
                    (CategoryDirection.Expense, "Оренда", null, 0L, 80_00L),
                }));
            Assert.That(accounts.Rows.Select(x => (x.Account, x.IncomeMinor, x.ExpenseMinor, x.IncomingTransfersMinor,
                x.OutgoingTransfersMinor, x.ResultMinor)), Is.EqualTo(new[]
            {
                ("Банк", 900_00L, 300_00L, (long?)0L, (long?)50_00L, 550_00L),
                ("Каса", 0L, 0L, 50_00L, 0L, 50_00L),
            }), "transfers move money between accounts but are not income or expense");
            Assert.That(onlyCash.Rows.Single().Account, Is.EqualTo("Каса"));
            Assert.That(onlyCash.Totals, Is.Empty, "the cash desk had no income or expense");
        });
        Assert.That(bank, Is.Not.EqualTo(cash));
    }

    [Test]
    public async Task Operations_report_is_oldest_first_and_limited_and_names_the_counterparty()
    {
        using var harness = new AccountingTestHarness();
        var (bank, _) = await Books(harness);
        var counterparty = (await new CreateFinanceCounterpartyHandler(harness.Db, harness.Time).Handle(
            new CreateFinanceCounterpartyCommand(new FinanceCounterpartyFields("ТОВ Ромашка", null, null, null, null, null,
                false, null), true, AccountingTestHarness.Director), default)).Value;
        var rentCategory = (await harness.Fresh().Categories.SingleAsync(x => x.Name == "Оренда")).Id.Value;
        await Spend(harness, Fields(bank, rentCategory, 10_00, new DateOnly(2026, 10, 30)) with
        {
            Counterparty = new FinanceCounterpartyLinkInput(CounterpartyKind.Financial, counterparty.Id.ToString()),
        });
        await Spend(harness, Fields(bank, rentCategory, 5_00, new DateOnly(2026, 10, 30)) with { OneoffName = "Сусід" });

        var operations = await Report(harness, FinanceReportKind.Operations);
        var limited = (await new GetFinanceReportHandler(harness.Fresh(), harness.Parties, harness.Parties).Handle(
            new GetFinanceReportQuery(new FinanceReportFilter(FinanceReportKind.Operations, From, To), 2), default)).Value;
        var parties = await Report(harness, FinanceReportKind.Counterparties);
        var onlyRomashka = await Report(harness, FinanceReportKind.Operations,
            counterparty: CounterpartyLink.Of(CounterpartyKind.Financial, counterparty.Id.ToString()).Value);

        Assert.Multiple(() =>
        {
            Assert.That(operations.Rows.Single(x => x.AmountMinor == 80_00).Note, Is.EqualTo("Оренда офісу"));
            Assert.That(operations.Rows.Select(x => x.Date), Is.Ordered);
            Assert.That(operations.Rows.Select(x => x.Counterparty).TakeLast(2),
                Is.EqualTo(new[] { "ТОВ Ромашка", "Сусід (разовий)" }));
            Assert.That((operations.TotalRows, operations.Limited), Is.EqualTo((6, false)));
            Assert.That((limited.Rows.Count, limited.TotalRows, limited.Limited), Is.EqualTo((2, 6, true)));
            Assert.That(parties.Rows.Select(x => (x.Counterparty, x.Operations)), Is.EqualTo(new[]
            {
                ("Сусід (разовий)", 1), ("ТОВ Ромашка", 1), (FinanceReports.WithoutCounterparty, 4),
            }));
            Assert.That(onlyRomashka.Rows.Single().AmountMinor, Is.EqualTo(10_00));
        });
    }

    [Test]
    public void Csv_is_excel_ready_and_escapes_quotes_and_formulas()
    {
        var report = new FinanceReportResponse(FinanceReportKind.Counterparties, From, To, [],
        [
            new FinanceReportRowResponse(FinanceReportRowType.Group, "UAH", 2, 1_234_50, 5, 1_234_45,
                Counterparty: "ТОВ \"Ромашка\""),
            new FinanceReportRowResponse(FinanceReportRowType.Group, "UAH", 1, 0, 7, -7, Counterparty: "=HYPERLINK()"),
        ], 2, false);

        var bytes = FinanceReportCsv.Write(report);
        var text = Encoding.UTF8.GetString(bytes.AsSpan(3));

        Assert.Multiple(() =>
        {
            Assert.That(bytes.Take(3), Is.EqualTo(new byte[] { 0xEF, 0xBB, 0xBF }), "BOM for Excel");
            Assert.That(text, Is.EqualTo(
                "\"Контрагент\";\"Валюта\";\"Операцій\";\"Доходи\";\"Витрати\";\"Різниця\"\r\n"
                + "\"ТОВ \"\"Ромашка\"\"\";\"UAH\";\"2\";\"1234,50\";\"0,05\";\"1234,45\"\r\n"
                + "\"'=HYPERLINK()\";\"UAH\";\"1\";\"0,00\";\"0,07\";\"-0,07\"\r\n"));
            Assert.That(FinanceReportCsv.FileName(report), Is.EqualTo("zvit-kontrahenty-2026-10-01-2026-10-31.csv"));
        });
    }

    [Test]
    public async Task Audit_lists_changes_of_every_record_newest_first_with_a_readable_title()
    {
        using var harness = new AccountingTestHarness();
        await Books(harness);
        var handler = new GetFinanceAuditHandler(harness.Fresh());

        var all = (await handler.Handle(new GetFinanceAuditQuery(null, null, 1), default)).Value;
        var voids = (await handler.Handle(new GetFinanceAuditQuery(AuditEntity.Transaction, AuditAction.Void, 1), default)).Value;
        var accounts = (await handler.Handle(new GetFinanceAuditQuery(AuditEntity.Account, null, 1), default)).Value;

        Assert.Multiple(() =>
        {
            Assert.That(all.Items.Select(x => x.Id), Is.Ordered.Descending);
            Assert.That(all.Total, Is.EqualTo(all.Items.Count));
            Assert.That(voids.Items.Single().Title, Is.EqualTo("20.10.2026 · 40,00 UAH · Помилка"));
            Assert.That(accounts.Items.Select(x => x.Title), Has.Member("Банк"));
            Assert.That(all.Items.Where(x => x.Entity == AuditEntity.Transfer).Select(x => x.Title),
                Has.Member("15.10.2026 · 50,00 UAH · Банк → Каса"));
        });
    }

    [Test]
    public async Task Export_holds_every_record_with_stored_fields_only()
    {
        using var harness = new AccountingTestHarness();
        await Books(harness);

        var file = (await new GetFinanceExportHandler(harness.Fresh(), harness.Time).Handle(new GetFinanceExportQuery(),
            default)).Value;
        using var json = JsonDocument.Parse(file.Content);
        var root = json.RootElement;
        var transaction = root.GetProperty("transactions")[0];

        Assert.Multiple(() =>
        {
            Assert.That(file.FileName, Is.EqualTo("brevi-finance-2026-11-05.json"));
            Assert.That(root.GetProperty("format").GetString(), Is.EqualTo(GetFinanceExportHandler.Format));
            Assert.That(root.GetProperty("accounts").GetArrayLength(), Is.EqualTo(2));
            Assert.That(root.GetProperty("transactions").GetArrayLength(), Is.EqualTo(6), "the voided one is kept");
            Assert.That(root.GetProperty("transfers").GetArrayLength(), Is.EqualTo(1));
            Assert.That(root.GetProperty("auditEntries").GetArrayLength(), Is.GreaterThan(0));
            Assert.That(transaction.TryGetProperty("amountMinor", out _), Is.True);
            Assert.That(transaction.TryGetProperty("details", out _), Is.False, "computed properties are left out");
            Assert.That(transaction.GetProperty("scope").GetString(), Is.EqualTo("Current"));
        });
    }

    /// <summary>
    /// Bank (confirmed) and cash desk; in October: income 900, expenses 100 (Матеріали), 120 (Тканини), 80 (Оренда),
    /// a voided 40 and a transfer of 50 to the cash desk; a November income outside the period.
    /// </summary>
    private static async Task<(Guid Bank, Guid Cash)> Books(AccountingTestHarness harness)
    {
        var bank = await Account(harness, "Банк", 1_000_00, cutoverMinor: 2_000_00);
        var cash = await Account(harness, "Каса", 0, cutoverMinor: 0);
        var materials = await Category(harness, "Матеріали");
        var fabric = await Category(harness, "Тканини", parentId: materials.Id);
        var rent = await Category(harness, "Оренда");
        var sales = await Category(harness, "Продажі", CategoryDirection.Income);
        await Spend(harness, Fields(bank.Id, sales.Id, 900_00, new DateOnly(2026, 10, 2), CategoryDirection.Income));
        await Spend(harness, Fields(bank.Id, materials.Id, 100_00, new DateOnly(2026, 10, 3)));
        await Spend(harness, Fields(bank.Id, fabric.Id, 120_00, new DateOnly(2026, 10, 4)));
        await Spend(harness, Fields(bank.Id, rent.Id, 80_00, new DateOnly(2026, 10, 5), description: "Оренда офісу"));
        var wrong = await Spend(harness, Fields(bank.Id, rent.Id, 40_00, new DateOnly(2026, 10, 20), description: "Помилка"));
        await new VoidFinanceTransactionHandler(harness.Db, harness.Parties, harness.Parties, harness.Time).Handle(
            new VoidFinanceTransactionCommand(wrong.Id, wrong.Version, AccountingTestHarness.Director), default);
        await Spend(harness, Fields(bank.Id, sales.Id, 500_00, new DateOnly(2026, 11, 2), CategoryDirection.Income));
        await Move(harness, bank.Id, cash.Id, 50_00);
        return (bank.Id, cash.Id);
    }

    private static async Task<FinanceReportResponse> Report(AccountingTestHarness harness, FinanceReportKind kind,
        Guid? accountId = null, CounterpartyLink? counterparty = null) =>
        (await new GetFinanceReportHandler(harness.Fresh(), harness.Parties, harness.Parties).Handle(
            new GetFinanceReportQuery(new FinanceReportFilter(kind, From, To, LedgerScope.Current, accountId,
                Counterparty: counterparty is { } link ? new FinanceCounterpartyLinkInput(link.Kind, link.Id) : null)), default)).Value;
}
