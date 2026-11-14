using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Audit;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;
using Accounting.Domain.Transfers;
using Ardalis.Result;
using static UnitTests.Accounting.FinanceMovementFixture;
using Accounting.Application.Features.Import.Commands.ImportLegacyFinance;
using Accounting.Application.Features.Import.Shared;
using Accounting.Application.Features.Import.Shared.Reading;
using Accounting.Application.Features.Import.Shared.Report;

namespace UnitTests.Accounting;

/// <summary>The move of the old Calculator «Фінанси» export (ACC-001 stage 5, v217 §20).</summary>
public sealed class LegacyFinanceImportTests
{
    private const string Bank = "f0000000-0000-4000-8000-000000000001";
    private const string OldCard = "01b92942-2b85-4d45-bf78-b090840a2432";
    private const string Root = "c1000000-0000-4000-8000-000000000001";
    private const string Child = "c1000000-0000-4000-8000-000000000002";
    private const string CurrentIncome = "c2000000-0000-4000-8000-000000000001";
    private const string Manual = "9c4bdc2d-3900-4495-bac1-c920012c37b8";
    private const string KnownClient = "000efb58-fe96-49c3-893f-bc14f71b9a87";
    private const string UnknownClient = "000efb58-fe96-49c3-893f-bc14f71b9a88";
    private const string UnusedClient = "000efb58-fe96-49c3-893f-bc14f71b9a89";
    private const string Supplier = "5a000000-0000-4000-8000-000000000001";
    private const string BrevisClient = "7d1c4f4e-0d7e-4c55-9c39-6b3c56f4a001";

    [Test]
    public async Task A_check_reports_counts_links_and_balances_without_writing()
    {
        using var harness = Harness();

        var result = await Import(harness, Export(), dryRun: true);
        var report = result.Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(report.Applied, Is.False);
            Assert.That(report.CanApply, Is.True, string.Join("; ", report.Errors));
            Assert.That(Count(report, LegacyImportKind.Accounts), Is.EqualTo((2, 2, 0, 0)));
            Assert.That(Count(report, LegacyImportKind.Categories), Is.EqualTo((3, 3, 0, 0)));
            Assert.That(Count(report, LegacyImportKind.Counterparties), Is.EqualTo((5, 2, 0, 3)),
                "manual and unknown client added; known client, supplier linked; unused client left out");
            Assert.That(Count(report, LegacyImportKind.Transactions), Is.EqualTo((5, 5, 0, 0)));
            Assert.That(Count(report, LegacyImportKind.Transfers), Is.EqualTo((1, 1, 0, 0)));
            Assert.That(report.Parties, Is.EqualTo(new LegacyImportPartiesResponse(1, 1, 1, 0, 1)));
            Assert.That(report.Totals.Single(x => x is
                    { Scope: LedgerScope.Current, Movement: LegacyImportMovement.Income, Status: MovementStatus.Active })
                .AmountMinor, Is.EqualTo(300_00));
            var bank = report.Balances.Single(x => x.AccountId == Guid.Parse(Bank));
            Assert.That(bank.ArchiveMinor, Is.EqualTo(1_000_00 - 100_00 - 400_00));
            Assert.That(bank.CutoverMinor, Is.EqualTo(500_00));
            Assert.That(bank.CurrentMinor, Is.EqualTo(500_00 + 300_00 - 20_00));
            Assert.That(bank.DifferenceMinor, Is.Null);
            Assert.That(harness.Fresh().Transactions.Count(), Is.Zero, "a check writes nothing");
        });
    }

    [Test]
    public async Task The_move_keeps_ids_sources_links_and_log_and_a_second_run_adds_nothing()
    {
        using var harness = Harness();

        var applied = await Import(harness, Export(), dryRun: false, gzip: true);
        var again = await Import(harness, Export(), dryRun: true);
        var db = harness.Fresh();
        var accounts = await Accounts(harness);

        Assert.Multiple(() =>
        {
            Assert.That(applied.IsSuccess, Is.True);
            Assert.That(applied.Value.Applied, Is.True, string.Join("; ", applied.Value.Errors));
            var archive = db.Transactions.Single(x => x.Id == FinanceTransactionId.From(Guid.Parse(Tx(1))));
            Assert.That(archive.Source, Is.EqualTo(MovementSource.HomeAccountingImport));
            Assert.That(archive.Scope, Is.EqualTo(LedgerScope.LegacyArchive));
            Assert.That(archive.CategoryId, Is.EqualTo(FinanceCategoryId.From(Guid.Parse(Child))));
            Assert.That(archive.ExternalId, Is.EqualTo("home-expense:r1"));
            Assert.That(archive.CreatedBy, Is.EqualTo("system:home-accounting-import"));
            var withClient = db.Transactions.Single(x => x.Id == FinanceTransactionId.From(Guid.Parse(Tx(2))));
            Assert.That(withClient.CounterpartyKind, Is.EqualTo(CounterpartyKind.Client));
            Assert.That(withClient.CounterpartyId, Is.EqualTo(BrevisClient));
            Assert.That(withClient.Fingerprint, Is.EqualTo("abc"));
            Assert.That(withClient.BankPurpose, Is.EqualTo("Оплата за рахунком 5"));
            var unknown = db.Transactions.Single(x => x.Id == FinanceTransactionId.From(Guid.Parse(Tx(3))));
            Assert.That(unknown.CounterpartyKind, Is.EqualTo(CounterpartyKind.Financial));
            Assert.That(unknown.CounterpartyId, Is.EqualTo(UnknownClient));
            Assert.That(unknown.Status, Is.EqualTo(MovementStatus.Void));
            var supplier = db.Transactions.Single(x => x.Id == FinanceTransactionId.From(Guid.Parse(Tx(4))));
            Assert.That(supplier.CounterpartyKind, Is.EqualTo(CounterpartyKind.Supplier));
            Assert.That(supplier.CounterpartyId, Is.EqualTo("12"));
            var uncategorised = db.Transactions.Single(x => x.Id == FinanceTransactionId.From(Guid.Parse(Tx(5))));
            Assert.That(uncategorised.Verification, Is.EqualTo(VerificationStatus.NeedsCategory));
            Assert.That(db.Counterparties.Count(), Is.EqualTo(2));
            Assert.That(db.Transfers.Single().Note, Is.EqualTo("З картки\nособисто"));
            var bank = accounts.Single(x => x.Id == Guid.Parse(Bank));
            Assert.That(bank.CutoverConfirmed, Is.True);
            Assert.That(bank.Balance!.LegacyMinor, Is.EqualTo(500_00));
            Assert.That(bank.Balance.CurrentMinor, Is.EqualTo(780_00), "same as the report");
            Assert.That(accounts.Single(x => x.Id == Guid.Parse(OldCard)).Active, Is.False);
            Assert.That(db.AuditEntries.Where(x => x.Entity == AuditEntity.Transaction).Select(x => x.Action),
                Is.EquivalentTo(new[] { AuditAction.Create, AuditAction.Void }));
            Assert.That(db.AuditEntries.Single(x => x.Entity == AuditEntity.Account).Action,
                Is.EqualTo(AuditAction.CutoverConfirm));
            Assert.That(again.Value.Counts.Sum(x => x.ToAdd), Is.Zero, "everything is already moved");
            Assert.That(Count(again.Value, LegacyImportKind.Transactions), Is.EqualTo((5, 0, 5, 0)));
        });
    }

    [Test]
    public async Task A_category_with_the_same_name_and_another_id_stops_the_move()
    {
        using var harness = Harness();
        await Category(harness, "Офіс", CategoryDirection.Expense, LedgerScope.LegacyArchive);

        var result = await Import(harness, Export(), dryRun: false);

        Assert.Multiple(() =>
        {
            Assert.That(result.Value.Applied, Is.False);
            Assert.That(result.Value.CanApply, Is.False);
            Assert.That(result.Value.Errors, Has.Some.Contains("Категорія «Офіс»"));
            Assert.That(harness.Fresh().Transactions.Count(), Is.Zero, "nothing is written");
        });
    }

    [Test]
    public async Task A_record_that_breaks_a_rule_is_reported_and_the_move_waits()
    {
        using var harness = Harness();
        var export = Export();
        // An archive date in the current books.
        export["transactions"]![0]!["transaction_date"] = "2026-10-05";

        var result = await Import(harness, export, dryRun: false);

        Assert.Multiple(() =>
        {
            Assert.That(result.Value.CanApply, Is.False);
            Assert.That(result.Value.ErrorCount, Is.EqualTo(1));
            Assert.That(result.Value.Errors.Single(), Does.StartWith("Операція 2026-10-05"));
        });
    }

    [Test]
    public async Task A_file_that_is_not_the_export_is_refused()
    {
        using var harness = Harness();

        var text = await new ImportLegacyFinanceHandler(harness.Db, harness.Parties, harness.Parties).Handle(
            new ImportLegacyFinanceCommand(new MemoryStream("не json"u8.ToArray()), true), default);
        var other = await Import(harness, new System.Text.Json.Nodes.JsonObject { ["format"] = "brevi-finance-v4" }, true);

        Assert.Multiple(() =>
        {
            Assert.That(text.Status, Is.EqualTo(ResultStatus.Invalid));
            Assert.That(other.Value.CanApply, Is.False);
            Assert.That(other.Value.Errors, Has.Some.Contains("brevi-finance-v4"));
        });
    }

    private static AccountingTestHarness Harness()
    {
        var harness = new AccountingTestHarness();
        harness.Parties.Clients.Add(new AccountingParty(BrevisClient, "Агробат ТОВ", "", true, "14423122"));
        harness.Parties.Suppliers.Add(new AccountingParty("12", "Тканини Плюс", "", true, "10521"));
        return harness;
    }

    private static (int, int, int, int) Count(LegacyFinanceImportReport report, LegacyImportKind kind) =>
        report.Counts.Single(x => x.Kind == kind) is var x ? (x.InFile, x.ToAdd, x.AlreadyMoved, x.Skipped) : default;

    private static async Task<Result<LegacyFinanceImportReport>> Import(AccountingTestHarness harness,
        System.Text.Json.Nodes.JsonObject export, bool dryRun, bool gzip = false)
    {
        var bytes = Encoding.UTF8.GetBytes(export.ToJsonString());
        if (gzip)
        {
            using var packed = new MemoryStream();
            await using (var zip = new GZipStream(packed, CompressionLevel.Fastest, leaveOpen: true))
                await zip.WriteAsync(bytes);
            bytes = packed.ToArray();
        }

        return await new ImportLegacyFinanceHandler(harness.Db, harness.Parties, harness.Parties).Handle(
            new ImportLegacyFinanceCommand(new MemoryStream(bytes), dryRun), default);
    }

    private static string Tx(int n) => $"7a000000-0000-4000-8000-00000000000{n}";

    private static System.Text.Json.Nodes.JsonObject Export()
    {
        const string at = "2026-09-25T21:43:31.231Z";
        object Account(string id, string name, string type, long opening, long? cutover, int active) => new
        {
            id, name, type, currency = "UAH", opening_minor = opening, opening_date = "2020-04-16", iban = "",
            cutover_balance_minor = cutover, cutover_balance_confirmed = cutover is null ? 0 : 1,
            cutover_balance_at = (string?)null, cutover_balance_by = (string?)null, owner = "", note = "", active,
            sort_order = 1, created_at = at, updated_at = at,
        };
        object Category(string id, string name, string direction, string? parent, string scope) => new
        {
            id, name, direction, parent_id = parent, active = 1, ledger_scope = scope, sort_order = 0, note = "",
            created_at = at, updated_at = "",
        };
        object Party(string id, string name, string source, string? type, string? keepin, string? supplier) => new
        {
            id, name, normalized_name = name.ToLowerInvariant(), external_source = source, external_type = type,
            external_id = keepin, edrpou = "3173 8765", iban = "", bank_name = "", aliases_json = "[\"Інша назва\"]",
            verified = 1, active = 1, default_category_id = (string?)null, auto_apply_category = 0, created_at = at,
            updated_at = at, roles_json = "[]", keepin_id = keepin, supplier_id = supplier,
        };
        object Tx(int n, string date, string type, string account, long amount, string? category, string? party,
            string scope, string source = "HOME_ACCOUNTING", string status = "ACTIVE", string verification = "VERIFIED",
            string? fingerprint = null, string purpose = "") => new
        {
            id = LegacyFinanceImportTests.Tx(n), transaction_date = date, type, account_id = account,
            from_account_id = (string?)null, amount_minor = amount, currency = "UAH", category_id = category,
            counterparty_id = party, description = "Опис " + n, comment = "", source,
            external_id = source == "HOME_ACCOUNTING" ? $"home-expense:r{n}" : null, fingerprint,
            verification_status = verification, bank_transaction_id = (string?)null, bank_purpose = purpose,
            ledger_scope = scope, created_by = source == "HOME_ACCOUNTING" ? "system:home-accounting-import" : "director@brevi.test",
            created_at = at, updated_at = at, status, version = 1, oneoff_name = "",
        };
        object Log(string transactionId, string action) => new
        {
            id = Guid.NewGuid().ToString(), transaction_id = transactionId, actor = "director@brevi.test", at, action,
            before_json = (string?)null, after_json = "{\"id\":\"" + transactionId + "\"}",
        };

        var export = new
        {
            format = "brevi-finance-v5",
            exportedAt = "2026-11-05T07:52:48.933Z",
            timezone = "Europe/Kyiv",
            accounts = new[]
            {
                Account(Bank, "Бреви Приватбанк", "bank", 1_000_00, 500_00, 1),
                Account(OldCard, "Карточка Альфа-банка", "card", 0, null, 0),
            },
            categories = new[]
            {
                Category(Child, "Папір", "EXPENSE", Root, "LEGACY_ARCHIVE"),
                Category(Root, "Офіс", "EXPENSE", null, "LEGACY_ARCHIVE"),
                Category(CurrentIncome, "Продажі", "INCOME", null, "CURRENT"),
            },
            counterparties = new[]
            {
                Party(Manual, "Податкова", "manual", null, null, null),
                Party(KnownClient, "Агробат ТОВ", "keepincrm", "client", "14423122", null),
                Party(UnknownClient, "Старий клієнт", "keepincrm", "client", "999", null),
                Party(UnusedClient, "Без операцій", "keepincrm", "client", "1000", null),
                Party(Supplier, "Тканини Плюс", "sheets", "supplier", null, "10521"),
            },
            transactions = new[]
            {
                Tx(1, "2025-03-01", "EXPENSE", Bank, 100_00, Child, null, "LEGACY_ARCHIVE"),
                Tx(2, "2026-10-02", "INCOME", Bank, 300_00, CurrentIncome, KnownClient, "CURRENT", "BANK_IMPORT",
                    fingerprint: "abc", purpose: "Оплата за рахунком 5"),
                Tx(3, "2026-10-03", "INCOME", Bank, 50_00, CurrentIncome, UnknownClient, "CURRENT", "MANUAL", "VOID"),
                Tx(4, "2026-10-04", "EXPENSE", Bank, 20_00, null, Supplier, "CURRENT", "MANUAL",
                    verification: "NEEDS_CATEGORY"),
                Tx(5, "2025-04-01", "EXPENSE", OldCard, 10_00, null, Manual, "LEGACY_ARCHIVE",
                    verification: "NEEDS_CATEGORY"),
            },
            transfers = new[]
            {
                new
                {
                    id = "7b000000-0000-4000-8000-000000000001", transfer_date = "2025-05-01", from_account_id = Bank,
                    to_account_id = OldCard, amount_minor = 400_00L, currency = "UAH", note = "З картки",
                    source = "HOME_ACCOUNTING", ledger_scope = "LEGACY_ARCHIVE", external_id = "home-transfer:r1",
                    fingerprint = (string?)null, created_by = "system:home-accounting-import", created_at = at,
                    updated_at = at, status = "ACTIVE", version = 1, personal_note = "особисто",
                },
            },
            attachments = Array.Empty<object>(),
            auditLog = new[] { Log(LegacyFinanceImportTests.Tx(3), "CREATE"), Log(LegacyFinanceImportTests.Tx(3), "VOID") },
            transferAuditLog = Array.Empty<object>(),
            categoryAuditLog = Array.Empty<object>(),
            counterpartyAuditLog = Array.Empty<object>(),
            accountAuditLog = new[]
            {
                new
                {
                    id = Guid.NewGuid().ToString(), account_id = Bank, actor = "director@brevi.test", at,
                    action = "CUTOVER_BALANCE_CONFIRM", snapshot_json = "{\"cutover_balance_minor\":50000}",
                },
            },
            settings = new[] { new { key = "financeCutoverDate", value = "2026-10-01" } },
            complete = true,
        };
        return System.Text.Json.Nodes.JsonNode.Parse(JsonSerializer.Serialize(export))!.AsObject();
    }
}
