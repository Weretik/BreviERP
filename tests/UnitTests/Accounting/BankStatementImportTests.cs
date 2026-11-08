using System.IO.Compression;
using System.Text;
using Accounting.Application.Abstractions;
using Accounting.Application.Contracts;
using Accounting.Application.Statements;
using Accounting.Domain.Accounts;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Statements;
using Accounting.Domain.Transactions;
using Accounting.Infrastructure.Statements;
using Ardalis.Result;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Accounting;

/// <summary>The PrivatBank statement import (ACC-001 stage 6, v217 §14).</summary>
public sealed class BankStatementImportTests
{
    private const string BankIban = "UA633052990000026004041206862";
    private const string SalaryIban = "UA563052990000026209888559038";
    private static readonly Guid BankId = Guid.Parse("f0000000-0000-4000-8000-000000000001");
    private static readonly Guid SalaryId = Guid.Parse("f0000000-0000-4000-8000-000000000002");
    private const string ClientId = "7d1c4f4e-0d7e-4c55-9c39-6b3c56f4a001";

    private const string Header =
        "Номер;Дата проводки;Час проводки;Сума;Валюта;Призначення платежу;ЄДРПОУ;Назва контрагента;Рахунок контрагента;МФО контрагента;Референс";

    /// <summary>A real line of the old Calculator (a bank fee) with the fingerprint the old Calculator gave it.</summary>
    private const string FeeRow =
        "ACOEXFIG2Y;12.10.2026;10:05:00;-5;UAH;Комiсiя за виконання платежiв в нацiональнiй валютi у сумi 677.25 грн вiд 12.10.2026, згiдно з вiдкритою офертою банку N б/н вiд 25.02.2020 та тарифiв банку, без ПДВ.;14360570;ЗА ДЕБЕТУВАННЯ РАХУНКУ(UAH);UA463052990000065101918401438;305299;JBKLQACOEXFIG2.Y";

    private const string FeeFingerprint = "d1eca7c349d96f595f5b82c695a8ae3c11ebdf0870c655d58061634e95067d7f";

    private static readonly string[] Rows =
    [
        FeeRow,
        "4858;14.10.2026;14:58:00;28299,60;UAH;Оплата за спецодяг згідно рах. 340;05387914;ТДВ \"ПТАХОФАБРИКА\";UA133005280000026009455051511;300528;HS42Q1001K1HMZ.P",
        "6712;15.10.2026;13:03:00;-3709.79;UAH;Оплата згідно рахунку 12879;32252472;ТОВ \"ТЕКСІКА\";UA923005280000026003001350156;300528;JBKLQAUOF2IQMC.1",
        "7001;16.10.2026;09:00:00;-71133;UAH;За виготовлення одягу;35521636;Бреви;" + SalaryIban + ";305299;JBKLQAUOF2K2UR.1",
        "7002;17.10.2026;09:00:00;-10;USD;Валютний рядок;;;;;JBKLQA0000001.1",
        "7003;20.09.2026;09:00:00;-10;UAH;Вересневий рядок;;;;;JBKLQA0000002.1",
        ";;;;;Разом обороти;;;;;",
    ];

    [Test]
    public async Task A_statement_is_read_into_recognised_lines_without_touching_the_books()
    {
        using var harness = await Harness();

        var uploaded = await Upload(harness, Csv(Rows), "stmts_35521636_" + BankIban + "_1.csv");
        var details = await Details(harness, uploaded.Value.Id);
        var lines = details.Lines.ToDictionary(x => x.RowNumber);
        var stored = harness.Fresh().BankStatementLines.Single(x => x.RowNumber == 2);

        Assert.Multiple(() =>
        {
            Assert.That(uploaded.IsSuccess, Is.True, string.Join("; ", uploaded.ValidationErrors.Select(x => x.ErrorMessage)));
            Assert.That(details.Statement.LineCount, Is.EqualTo(6), "the total row is skipped");
            Assert.That(stored.Fingerprint, Is.EqualTo(FeeFingerprint), "the old Calculator fingerprint");
            Assert.That(lines[2].State, Is.EqualTo(BankLineState.Ready), "bank by ЄДРПОУ with its auto category");
            Assert.That(lines[2].Type, Is.EqualTo(CategoryDirection.Expense));
            Assert.That(lines[2].AmountMinor, Is.EqualTo(5_00));
            Assert.That(lines[3].Counterparty?.Kind, Is.EqualTo(CounterpartyKind.Client), "client by ЄДРПОУ");
            Assert.That(lines[3].State, Is.EqualTo(BankLineState.NeedsCategory));
            Assert.That(lines[3].AmountMinor, Is.EqualTo(28_299_60));
            Assert.That(lines[4].State, Is.EqualTo(BankLineState.NewCounterparty));
            Assert.That(lines[5].State, Is.EqualTo(BankLineState.Transfer));
            Assert.That(lines[5].TransferAccountId, Is.EqualTo(SalaryId));
            Assert.That(lines[6].Error, Does.Contain("USD"));
            Assert.That(lines[7].Error, Does.Contain("01.10.2026"));
            Assert.That(details.Totals, Is.EqualTo(new BankStatementTotals(3, 28_299_60, 5_00 + 3_709_79, 1)));
            Assert.That(harness.Fresh().Transactions.Count(), Is.Zero);
        });
    }

    [Test]
    public async Task The_same_file_opens_the_statement_uploaded_before()
    {
        using var harness = await Harness();
        var first = await Upload(harness, Csv(Rows));

        var again = await Upload(harness, Csv(Rows), "інша назва.csv");

        Assert.Multiple(() =>
        {
            Assert.That(again.Value.Id, Is.EqualTo(first.Value.Id));
            Assert.That(again.Value.Existing, Is.True);
            Assert.That(harness.Fresh().BankStatements.Count(), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Booking_adds_operations_transfers_and_unconfirmed_counterparties_once()
    {
        using var harness = await Harness();
        var uploaded = (await Upload(harness, Csv(Rows))).Value;

        var booked = await Book(harness, uploaded);
        var db = harness.Fresh();
        var operations = db.Transactions.ToList();
        var transfer = db.Transfers.Single();
        var created = db.Counterparties.Single(x => x.Edrpou == "32252472");
        var again = await Book(harness, booked.Value);

        Assert.Multiple(() =>
        {
            Assert.That(booked.IsSuccess, Is.True);
            Assert.That(booked.Value.Status, Is.EqualTo(BankStatementStatus.Booked));
            Assert.That((booked.Value.TransactionCount, booked.Value.TransferCount), Is.EqualTo((3, 1)));
            Assert.That(operations, Has.All.Matches<FinanceTransaction>(x => x.Source == MovementSource.BankImport
                && x.Scope == LedgerScope.Current && x.Fingerprint != null));
            var fee = operations.Single(x => x.BankTransactionId == "JBKLQACOEXFIG2.Y");
            Assert.That(fee.Verification, Is.EqualTo(VerificationStatus.Verified));
            Assert.That(fee.ExternalId, Is.EqualTo("JBKLQACOEXFIG2.Y"));
            Assert.That(operations.Single(x => x.Type == CategoryDirection.Income).Verification,
                Is.EqualTo(VerificationStatus.NeedsCategory));
            Assert.That(created.Verified, Is.False);
            Assert.That(created.Name, Is.EqualTo("ТОВ \"ТЕКСІКА\""));
            Assert.That(operations.Single(x => x.BankTransactionId == "JBKLQAUOF2IQMC.1").CounterpartyId,
                Is.EqualTo(created.Id.Value.ToString("D")));
            Assert.That((transfer.FromAccountId.Value, transfer.ToAccountId.Value), Is.EqualTo((BankId, SalaryId)));
            Assert.That(transfer.AmountMinor, Is.EqualTo(71_133_00));
            Assert.That(db.BankStatementLines.Count(x => x.TransactionId != null || x.TransferId != null), Is.EqualTo(4));
            Assert.That(db.AuditEntries.Count(), Is.EqualTo(3 + 1 + 1), "operations, transfer and created counterparty");
            Assert.That(again.Status, Is.EqualTo(ResultStatus.Error).Or.EqualTo(ResultStatus.Conflict).Or.EqualTo(ResultStatus.Invalid));
        });
    }

    [Test]
    public async Task Lines_already_in_the_books_are_duplicates()
    {
        using var harness = await Harness();
        await Book(harness, (await Upload(harness, Csv(Rows[..2]))).Value);
        var category = harness.Db.Categories.Single(x => x.Name == "Банк");
        harness.Db.Transactions.Add(FinanceTransaction.Create(FinanceTransactionId.New(), LedgerScope.Current,
            FinanceTransactionDetails.Of(new DateOnly(2026, 10, 15), CategoryDirection.Expense, FinanceAccountId.From(BankId),
                3_709_79, category.Id, null, null, "Вручну", null),
            harness.Db.Accounts.Single(x => x.Id == FinanceAccountId.From(BankId)), category, null,
            AccountingTestHarness.Director, harness.Time.GetUtcNow()));
        await harness.Db.SaveChangesAsync();

        var second = (await Upload(harness, Csv(Rows[..3]))).Value;
        var lines = (await Details(harness, second.Id)).Lines.ToDictionary(x => x.RowNumber);
        var include = await Edit(harness, second.Id, lines[4].Id, excluded: false);
        var includeExact = await Edit(harness, second.Id, lines[2].Id, excluded: false);

        Assert.Multiple(() =>
        {
            Assert.That(lines[2].Duplicate, Is.EqualTo(BankDuplicate.Exact));
            Assert.That(lines[3].Duplicate, Is.EqualTo(BankDuplicate.Exact));
            Assert.That(lines[4].Duplicate, Is.EqualTo(BankDuplicate.Probable), "a hand-made operation of the same day and sum");
            Assert.That(lines[4].Excluded, Is.True);
            Assert.That(lines[4].DuplicateNote, Does.Contain("вручну"));
            Assert.That(include.Value.WillBook, Is.True, "a probable duplicate may be included");
            Assert.That(includeExact.IsSuccess, Is.False);
        });
    }

    [Test]
    public async Task A_line_takes_the_directors_category_and_one_off_name_until_booking()
    {
        using var harness = await Harness();
        var uploaded = (await Upload(harness, Csv(Rows))).Value;
        var line = (await Details(harness, uploaded.Id)).Lines.Single(x => x.RowNumber == 4);
        var category = harness.Db.Categories.Single(x => x.Name == "Тканини");
        var income = harness.Db.Categories.Single(x => x.Name == "Продажі");

        var edited = await Edit(harness, uploaded.Id, line.Id, category.Id.Value, oneoff: "Тексіка", description: "Тканина");
        var wrong = await Edit(harness, uploaded.Id, line.Id, income.Id.Value);
        await Book(harness, uploaded);
        var afterBooking = await Edit(harness, uploaded.Id, line.Id, category.Id.Value);
        var operation = harness.Fresh().Transactions.Single(x => x.BankTransactionId == "JBKLQAUOF2IQMC.1");

        Assert.Multiple(() =>
        {
            Assert.That(edited.Value.State, Is.EqualTo(BankLineState.Ready));
            Assert.That(edited.Value.OneoffName, Is.EqualTo("Тексіка"));
            Assert.That(wrong.IsSuccess, Is.False, "an income category for an expense line");
            Assert.That(afterBooking.IsSuccess, Is.False);
            Assert.That(operation.OneoffName, Is.EqualTo("Тексіка"));
            Assert.That(operation.Description, Is.EqualTo("Тканина"));
            Assert.That(operation.Verification, Is.EqualTo(VerificationStatus.Verified));
            Assert.That(harness.Fresh().Counterparties.Any(x => x.Edrpou == "32252472"), Is.False);
        });
    }

    [Test]
    public async Task A_card_statement_in_xlsx_is_read_below_its_title_rows()
    {
        using var harness = await Harness();
        var xlsx = Xlsx(
            ["Виписка з карткового рахунку " + SalaryIban],
            [],
            ["Дата", "Категорія", "Картка", "Опис операції", "Сума в валюті картки", "Валюта картки",
                "Сума в валюті транзакції", "Валюта транзакції"],
            ["09.10.2026 13:15:10", "Зарахування переказу", "4149 **** **** 8661", "Согоконь О.", "8000", "UAH", "8000", "UAH"],
            ["08.10.2026 14:59:12", "Кіно", "4149 **** **** 8661", "Megogo", "-249", "UAH", "249", "UAH"]);

        var wrongAccount = await Upload(harness, xlsx, "картка.xlsx");
        var card = await Upload(harness, xlsx, "картка.xlsx", SalaryId);
        var lines = (await Details(harness, card.Value.Id)).Lines;

        Assert.Multiple(() =>
        {
            Assert.That(wrongAccount.Status, Is.EqualTo(ResultStatus.Invalid));
            Assert.That(wrongAccount.ValidationErrors.Single().ErrorMessage, Does.Contain("іншого рахунку"));
            Assert.That(card.IsSuccess, Is.True, string.Join("; ", card.ValidationErrors.Select(x => x.ErrorMessage)));
            Assert.That(lines.Select(x => (x.Type, x.AmountMinor, x.Purpose)), Is.EqualTo(new[]
            {
                ((CategoryDirection?)CategoryDirection.Income, (long?)8_000_00, "Согоконь О."),
                (CategoryDirection.Expense, 249_00, "Megogo"),
            }));
            Assert.That(lines.Select(x => x.State), Is.All.EqualTo(BankLineState.NeedsCategory),
                "a card line has no bank counterparty to create");
            Assert.That(lines[0].Time, Is.Null, "the time is taken only from a time column");
        });
    }

    [TestCase("Дата;Опис\n12.10.2026;Щось\n", "заголовок")]
    [TestCase("Дата;Сума;Сума операції\n12.10.2026;1;1\n", "кілька колонок")]
    [TestCase("", "порожній")]
    public async Task A_file_that_cannot_be_used_is_refused_with_the_reason(string csv, string reason)
    {
        using var harness = await Harness();

        var result = await Upload(harness, Encoding.UTF8.GetBytes(csv));

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(ResultStatus.Invalid));
            Assert.That(result.ValidationErrors.Single().ErrorMessage, Does.Contain(reason));
        });
    }

    [Test]
    public async Task A_windows_1251_csv_with_commas_is_read()
    {
        using var harness = await Harness();
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var csv = "Дата операції,Сума,Призначення платежу\n15.10.2026,\"-1 250,50\",Оренда цеху\n";

        var result = await Upload(harness, Encoding.GetEncoding(1251).GetBytes(csv));
        var line = (await Details(harness, result.Value.Id)).Lines.Single();

        Assert.Multiple(() =>
        {
            Assert.That(line.Purpose, Is.EqualTo("Оренда цеху"));
            Assert.That(line.AmountMinor, Is.EqualTo(1_250_50));
            Assert.That(line.Type, Is.EqualTo(CategoryDirection.Expense));
        });
    }

    private static async Task<AccountingTestHarness> Harness()
    {
        var harness = new AccountingTestHarness();
        var now = harness.Time.GetUtcNow();
        var db = harness.Db;
        db.Accounts.Add(FinanceAccount.Create(FinanceAccountId.From(BankId), FinanceAccountDetails.Of("Бреви Приватбанк",
            FinanceAccountType.Bank, "UAH", BankIban, 0, new DateOnly(2024, 1, 1), null, 1), now));
        db.Accounts.Add(FinanceAccount.Create(FinanceAccountId.From(SalaryId), FinanceAccountDetails.Of("Карта ЗП",
            FinanceAccountType.Card, "UAH", SalaryIban, 0, new DateOnly(2024, 1, 1), null, 2), now));
        var bankFees = FinanceCategory.CreateRoot(FinanceCategoryId.New(), "Банк", CategoryDirection.Expense,
            LedgerScope.Current, null, 1, now);
        db.Categories.Add(bankFees);
        db.Categories.Add(FinanceCategory.CreateRoot(FinanceCategoryId.New(), "Тканини", CategoryDirection.Expense,
            LedgerScope.Current, null, 2, now));
        db.Categories.Add(FinanceCategory.CreateRoot(FinanceCategoryId.New(), "Продажі", CategoryDirection.Income,
            LedgerScope.Current, null, 3, now));
        db.Counterparties.Add(FinanceCounterparty.Create(FinanceCounterpartyId.New(),
            FinanceCounterpartyDetails.Of("ПриватБанк", "14360570", null, null, null, bankFees.Id, true, null), bankFees,
            verified: true, now));
        await db.SaveChangesAsync();
        harness.Parties.Clients.Add(new AccountingParty(ClientId, "Птахофабрика", "05387914", true));
        return harness;
    }

    private static byte[] Csv(IEnumerable<string> rows) =>
        Encoding.UTF8.GetBytes(string.Join("\r\n", new[] { Header }.Concat(rows)));

    private static Task<Result<BankStatementResponse>> Upload(AccountingTestHarness harness, byte[] content,
        string fileName = "виписка.csv", Guid? accountId = null) =>
        new UploadBankStatementCommandHandler(harness.Db, new BankStatementFileReader(), harness.Parties, harness.Parties,
                harness.Time)
            .Handle(new UploadBankStatementCommand(accountId ?? BankId, new MemoryStream(content), fileName,
                AccountingTestHarness.Director), default).AsTask();

    private static async Task<BankStatementDetailsResponse> Details(AccountingTestHarness harness, Guid id) =>
        (await new GetBankStatementQueryHandler(harness.Fresh()).Handle(new GetBankStatementQuery(id, null, 1), default)).Value;

    private static async Task<Result<BankStatementResponse>> Book(AccountingTestHarness harness, BankStatementResponse statement)
    {
        await using var db = harness.Fresh();
        try
        {
            return await new BookBankStatementCommandHandler(db, harness.Parties, harness.Parties, harness.Time)
                .Handle(new BookBankStatementCommand(statement.Id, statement.Version, AccountingTestHarness.Director), default);
        }
        catch (BuildingBlocks.Domain.Exceptions.DomainException error)
        {
            return Result.Invalid(new ValidationError("Statement", error.Message));
        }
    }

    private static async Task<Result<BankStatementLineResponse>> Edit(AccountingTestHarness harness, Guid statementId,
        Guid lineId, Guid? categoryId = null, string? oneoff = null, string? description = null, bool excluded = false)
    {
        await using var db = harness.Fresh();
        try
        {
            return await new UpdateBankLineCommandHandler(db, harness.Parties, harness.Parties).Handle(
                new UpdateBankLineCommand(statementId, lineId, categoryId, null, oneoff, description, null, excluded), default);
        }
        catch (BuildingBlocks.Domain.Exceptions.DomainException error)
        {
            return Result.Invalid(new ValidationError("Line", error.Message));
        }
    }

    /// <summary>A minimal XLSX workbook with one sheet of text cells.</summary>
    private static byte[] Xlsx(params string[][] rows)
    {
        static string Escape(string text) => System.Security.SecurityElement.Escape(text);
        var sheet = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\"?><worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData>");
        for (var r = 0; r < rows.Length; r++)
        {
            sheet.Append($"<row r=\"{r + 1}\">");
            for (var c = 0; c < rows[r].Length; c++)
                sheet.Append($"<c r=\"{(char)('A' + c)}{r + 1}\" t=\"inlineStr\"><is><t>{Escape(rows[r][c])}</t></is></c>");
            sheet.Append("</row>");
        }

        sheet.Append("</sheetData></worksheet>");
        var parts = new Dictionary<string, string>
        {
            ["[Content_Types].xml"] = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"><Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Default Extension=\"xml\" ContentType=\"application/xml\"/><Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/><Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/></Types>",
            ["_rels/.rels"] = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/></Relationships>",
            ["xl/workbook.xml"] = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"><sheets><sheet name=\"Виписка\" sheetId=\"1\" r:id=\"rId1\"/></sheets></workbook>",
            ["xl/_rels/workbook.xml.rels"] = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/></Relationships>",
            ["xl/worksheets/sheet1.xml"] = sheet.ToString(),
        };
        using var buffer = new MemoryStream();
        using (var zip = new ZipArchive(buffer, ZipArchiveMode.Create, leaveOpen: true))
            foreach (var (name, xml) in parts)
            {
                using var writer = new StreamWriter(zip.CreateEntry(name).Open(), new UTF8Encoding(false));
                writer.Write(xml);
            }

        return buffer.ToArray();
    }
}
