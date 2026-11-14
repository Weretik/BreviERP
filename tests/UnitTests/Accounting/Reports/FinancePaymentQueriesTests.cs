using Accounting.Application.Abstractions;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using static UnitTests.Accounting.FinanceMovementFixture;
using Accounting.Application.Features.Payments.Queries.GetFinancePaymentCandidates;
using Accounting.Application.Features.Payments.Queries.GetFinanceTransactionsByIds;
using Accounting.Application.Features.Transactions.Commands.VoidFinanceTransaction;
using Accounting.Application.Features.Transactions.Shared;

namespace UnitTests.Accounting.Reports;

/// <summary>Expenses for the payment links of «Рахунки на закупку» (ACC-001 stage 9).</summary>
[TestFixture]
public sealed class FinancePaymentQueriesTests
{
    [Test]
    public async Task Candidates_are_active_expenses_of_the_supplier_or_found_by_text_or_amount_with_the_amount_first()
    {
        using var harness = new AccountingTestHarness();
        harness.Parties.Suppliers.Add(new AccountingParty("7", "Текстиль-Контакт", "", true));
        var bank = await Account(harness);
        var materials = await Category(harness, "Матеріали");
        var sales = await Category(harness, "Продажі", CategoryDirection.Income);
        var oldMaterials = await Category(harness, "Старі матеріали", scope: LedgerScope.LegacyArchive);
        var supplier = new FinanceCounterpartyLinkInput(CounterpartyKind.Supplier, "7");
        var older = await Spend(harness, Fields(bank.Id, materials.Id, 1_560_00, new DateOnly(2026, 10, 3), description: "Рахунок 15")
            with { Counterparty = supplier });
        var newer = await Spend(harness, Fields(bank.Id, materials.Id, 500_00, new DateOnly(2026, 10, 9), description: "нитки сині")
            with { Counterparty = supplier });
        var archive = await Spend(harness, Fields(bank.Id, oldMaterials.Id, 1_560_00, Archive, description: "Стара тканина"),
            LedgerScope.LegacyArchive);
        var income = await Spend(harness, Fields(bank.Id, sales.Id, 1_560_00, type: CategoryDirection.Income));
        var voided = await Spend(harness, Fields(bank.Id, materials.Id, 70_00, description: "нитки помилково")
            with { Counterparty = supplier });
        await new VoidFinanceTransactionHandler(harness.Db, harness.Parties, harness.Parties, harness.Time).Handle(
            new VoidFinanceTransactionCommand(voided.Id, voided.Version, AccountingTestHarness.Director), default);
        var handler = new GetFinancePaymentCandidatesHandler(harness.Fresh(), harness.Parties, harness.Parties);

        var bySupplier = (await handler.Handle(new GetFinancePaymentCandidatesQuery(7, 1_560_00, null), default)).Value;
        var byAmount = (await handler.Handle(new GetFinancePaymentCandidatesQuery(null, 1_560_00, null), default)).Value;
        // SQLite lowers only Latin letters, so the text here is already lower-case; PostgreSQL lowers Cyrillic too.
        var byText = (await handler.Handle(new GetFinancePaymentCandidatesQuery(7, null, "нитки"), default)).Value;
        var byTypedAmount = (await handler.Handle(new GetFinancePaymentCandidatesQuery(null, null, "1 560,00"), default)).Value;
        var none = (await handler.Handle(new GetFinancePaymentCandidatesQuery(null, null, null), default)).Value;

        Assert.Multiple(() =>
        {
            Assert.That(bySupplier.Select(x => x.Id), Is.EqualTo(new[] { older.Id, newer.Id }),
                "the invoice amount first, voided ones left out");
            Assert.That(bySupplier[0].Counterparty?.Name, Is.EqualTo("Текстиль-Контакт"));
            Assert.That(byAmount.Select(x => x.Id), Is.EqualTo(new[] { older.Id, archive.Id }),
                "both scopes, expenses only");
            Assert.That(byText.Select(x => x.Id), Is.EqualTo(new[] { newer.Id }));
            Assert.That(byTypedAmount.Select(x => x.Id), Is.EquivalentTo(new[] { older.Id, archive.Id }));
            Assert.That(none, Is.Empty);
        });
        Assert.That(income.Id, Is.Not.EqualTo(older.Id));
    }

    [Test]
    public async Task Operations_by_id_include_voided_ones_and_skip_unknown_ids()
    {
        using var harness = new AccountingTestHarness();
        var bank = await Account(harness);
        var materials = await Category(harness, "Матеріали");
        var paid = await Spend(harness, Fields(bank.Id, materials.Id, 100_00));
        await new VoidFinanceTransactionHandler(harness.Db, harness.Parties, harness.Parties, harness.Time).Handle(
            new VoidFinanceTransactionCommand(paid.Id, paid.Version, AccountingTestHarness.Director), default);

        var found = (await new GetFinanceTransactionsByIdsHandler(harness.Fresh(), harness.Parties, harness.Parties)
            .Handle(new GetFinanceTransactionsByIdsQuery([paid.Id, Guid.NewGuid()]), default)).Value;

        Assert.That(found.Single().Status, Is.EqualTo(MovementStatus.Void));
    }

    [TestCase("1 250,40", 1_250_40L)]
    [TestCase("1250.4", 1_250_40L)]
    [TestCase("нитки", null)]
    [TestCase("12,345", null)]
    public void Amount_of_a_search_is_hryvnias_with_kopecks(string text, long? minor) =>
        Assert.That(GetFinancePaymentCandidatesHandler.AmountOf(text), Is.EqualTo(minor));
}
