using Accounting.Api.DependencyInjection;
using UnitTests.TestSupport;
using System.Text.Json;
using Accounting.Domain.Statements;
using Accounting.Domain.Accounts;
using Accounting.Domain.Audit;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;
using YamlDotNet.RepresentationModel;
using Accounting.Application.Features.Accounts.Shared;
using Accounting.Application.Features.Attachments.Shared;
using Accounting.Application.Features.Audit.Queries.GetFinanceAudit;
using Accounting.Application.Features.Categories.Shared;
using Accounting.Application.Features.Counterparties.Queries.GetFinanceCounterparties;
using Accounting.Application.Features.Counterparties.Queries.GetFinanceCounterpartyChoices;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.History.Queries.GetFinanceHistory;
using Accounting.Application.Features.Import.Shared;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Overview.Queries.GetFinanceOverview;
using Accounting.Application.Features.Reports.Queries.GetFinanceReport;
using Accounting.Application.Features.Reports.Shared;
using Accounting.Application.Features.Settings.Queries.GetAccountingSettings;
using Accounting.Application.Features.Statements.Queries.GetBankStatement;
using Accounting.Application.Features.Statements.Shared;
using Accounting.Application.Features.Transactions.Queries.GetFinanceTransactions;
using Accounting.Application.Features.Transactions.Shared;
using Accounting.Application.Features.Transfers.Queries.GetFinanceTransfers;
using Accounting.Application.Features.Transfers.Shared;
using Accounting.Api.Endpoints.Accounts.CreateFinanceAccount;
using Accounting.Api.Endpoints.Accounts.SetFinanceAccountCutover;
using Accounting.Api.Endpoints.Accounts.UpdateFinanceAccount;
using Accounting.Api.Endpoints.Categories.CreateFinanceCategory;
using Accounting.Api.Endpoints.Categories.UpdateFinanceCategory;
using Accounting.Api.Endpoints.Counterparties.CreateFinanceCounterparty;
using Accounting.Api.Endpoints.Counterparties.MergeFinanceCounterparty;
using Accounting.Api.Endpoints.Counterparties.UpdateFinanceCounterparty;
using Accounting.Api.Endpoints.Shared;
using Accounting.Api.Endpoints.Statements.UpdateBankStatementLine;
using Accounting.Api.Endpoints.Transactions.CreateFinanceTransaction;
using Accounting.Api.Endpoints.Transactions.ReviewFinanceTransaction;
using Accounting.Api.Endpoints.Transactions.UpdateFinanceTransaction;
using Accounting.Api.Endpoints.Transfers.CreateFinanceTransfer;
using Accounting.Api.Endpoints.Transfers.UpdateFinanceTransfer;
using Accounting.Domain.Statements.Enums;
using Accounting.Application.Features.Counterparties.Queries.GetFinanceCounterparties.Results;
using Accounting.Application.Features.Import.Shared.Reading;
using Accounting.Application.Features.Import.Shared.Report;
using Accounting.Application.Features.Overview.Queries.GetFinanceOverview.Results;
using Accounting.Application.Features.Reports.Queries.GetFinanceReport.Results;
using Accounting.Application.Features.Statements.Queries.GetBankStatement.Results;
using Accounting.Application.Features.Statements.Shared.Responses;

namespace UnitTests.Accounting;

/// <summary>Types and operations match docs/sdd/contracts/accounting/finance.openapi.yaml (ACC-001).</summary>
public sealed class AccountingContractTests
{
    [TestCase(typeof(AccountingSettingsResponse), "AccountingSettingsResponse")]
    [TestCase(typeof(FinanceAccountResponse), "FinanceAccountResponse")]
    [TestCase(typeof(FinanceAccountInput), "FinanceAccountInput")]
    [TestCase(typeof(FinanceAccountUpdateInput), "FinanceAccountUpdateInput")]
    [TestCase(typeof(FinanceAccountCutoverInput), "FinanceAccountCutoverInput")]
    [TestCase(typeof(FinanceVersionInput), "FinanceVersionInput")]
    [TestCase(typeof(FinanceCategoryResponse), "FinanceCategoryResponse")]
    [TestCase(typeof(FinanceCategoryInput), "FinanceCategoryInput")]
    [TestCase(typeof(FinanceCategoryUpdateInput), "FinanceCategoryUpdateInput")]
    [TestCase(typeof(FinanceHistoryEntryResponse), "FinanceHistoryEntryResponse")]
    [TestCase(typeof(FinanceAccountBalanceResponse), "FinanceAccountBalanceResponse")]
    [TestCase(typeof(FinanceOverviewResponse), "FinanceOverviewResponse")]
    [TestCase(typeof(FinanceOverviewCurrencyResponse), "FinanceOverviewCurrencyResponse")]
    [TestCase(typeof(FinanceOverviewCategoryResponse), "FinanceOverviewCategoryResponse")]
    [TestCase(typeof(FinanceTransactionResponse), "FinanceTransactionResponse")]
    [TestCase(typeof(FinanceTransactionTotalResponse), "FinanceTransactionTotalResponse")]
    [TestCase(typeof(FinanceTransactionPageResponse), "FinanceTransactionPageResponse")]
    [TestCase(typeof(FinanceTransactionInput), "FinanceTransactionInput")]
    [TestCase(typeof(FinanceTransactionUpdateInput), "FinanceTransactionUpdateInput")]
    [TestCase(typeof(FinanceTransferResponse), "FinanceTransferResponse")]
    [TestCase(typeof(FinanceTransferTotalResponse), "FinanceTransferTotalResponse")]
    [TestCase(typeof(FinanceTransferPageResponse), "FinanceTransferPageResponse")]
    [TestCase(typeof(FinanceTransferInput), "FinanceTransferInput")]
    [TestCase(typeof(FinanceTransferUpdateInput), "FinanceTransferUpdateInput")]
    [TestCase(typeof(FinanceCounterpartyLinkInput), "FinanceCounterpartyLinkInput")]
    [TestCase(typeof(FinanceCounterpartyRefResponse), "FinanceCounterpartyRefResponse")]
    [TestCase(typeof(FinanceCounterpartyResponse), "FinanceCounterpartyResponse")]
    [TestCase(typeof(FinancePartyResponse), "FinancePartyResponse")]
    [TestCase(typeof(FinanceCounterpartyCountsResponse), "FinanceCounterpartyCountsResponse")]
    [TestCase(typeof(FinanceCounterpartyPageResponse), "FinanceCounterpartyPageResponse")]
    [TestCase(typeof(FinanceCounterpartyChoiceResponse), "FinanceCounterpartyChoiceResponse")]
    [TestCase(typeof(FinanceCounterpartyInput), "FinanceCounterpartyInput")]
    [TestCase(typeof(FinanceCounterpartyUpdateInput), "FinanceCounterpartyUpdateInput")]
    [TestCase(typeof(FinanceCounterpartyMergeInput), "FinanceCounterpartyMergeInput")]
    [TestCase(typeof(FinanceTransactionReviewInput), "FinanceTransactionReviewInput")]
    [TestCase(typeof(FinanceAttachmentResponse), "FinanceAttachmentResponse")]
    [TestCase(typeof(LegacyFinanceImportReport), "LegacyFinanceImportReport")]
    [TestCase(typeof(LegacyImportCountResponse), "LegacyImportCountResponse")]
    [TestCase(typeof(LegacyImportPartiesResponse), "LegacyImportPartiesResponse")]
    [TestCase(typeof(LegacyImportTotalResponse), "LegacyImportTotalResponse")]
    [TestCase(typeof(LegacyImportBalanceResponse), "LegacyImportBalanceResponse")]
    [TestCase(typeof(BankStatementResponse), "BankStatementResponse")]
    [TestCase(typeof(BankStatementDetailsResponse), "BankStatementDetailsResponse")]
    [TestCase(typeof(BankLineStateCount), "BankLineStateCount")]
    [TestCase(typeof(BankStatementTotals), "BankStatementTotals")]
    [TestCase(typeof(BankStatementRollback), "BankStatementRollback")]
    [TestCase(typeof(BankStatementLineResponse), "BankStatementLineResponse")]
    [TestCase(typeof(BankStatementLineInput), "BankStatementLineInput")]
    [TestCase(typeof(FinanceReportResponse), "FinanceReportResponse")]
    [TestCase(typeof(FinanceReportTotalResponse), "FinanceReportTotalResponse")]
    [TestCase(typeof(FinanceReportRowResponse), "FinanceReportRowResponse")]
    [TestCase(typeof(FinanceAuditPageResponse), "FinanceAuditPageResponse")]
    [TestCase(typeof(FinanceAuditEntryResponse), "FinanceAuditEntryResponse")]
    public void Serialized_properties_match_the_machine_contract(Type type, string schema)
    {
        var properties = (YamlMappingNode)((YamlMappingNode)Schemas()[schema])["properties"];
        Assert.That(properties.Children.Keys.Cast<YamlScalarNode>().Select(x => x.Value),
            Is.EquivalentTo(type.GetProperties().Select(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name))));
    }

    [TestCase(typeof(FinanceAccountType), "FinanceAccountType")]
    [TestCase(typeof(CategoryDirection), "CategoryDirection")]
    [TestCase(typeof(LedgerScope), "LedgerScope")]
    [TestCase(typeof(AuditEntity), "AuditEntity")]
    [TestCase(typeof(AuditAction), "AuditAction")]
    [TestCase(typeof(MovementSource), "MovementSource")]
    [TestCase(typeof(MovementStatus), "MovementStatus")]
    [TestCase(typeof(VerificationStatus), "VerificationStatus")]
    [TestCase(typeof(FinanceJournalSort), "FinanceJournalSort")]
    [TestCase(typeof(FinanceSortDirection), "FinanceSortDirection")]
    [TestCase(typeof(CounterpartyKind), "CounterpartyKind")]
    [TestCase(typeof(FinanceCounterpartyTab), "FinanceCounterpartyTab")]
    [TestCase(typeof(LegacyImportKind), "LegacyImportKind")]
    [TestCase(typeof(BankStatementStatus), "BankStatementStatus")]
    [TestCase(typeof(BankLineState), "BankLineState")]
    [TestCase(typeof(BankDuplicate), "BankDuplicate")]
    [TestCase(typeof(LegacyImportMovement), "LegacyImportMovement")]
    [TestCase(typeof(FinanceReportKind), "FinanceReportKind")]
    [TestCase(typeof(FinanceReportRowType), "FinanceReportRowType")]
    public void Enum_values_match_the_machine_contract(Type type, string schema)
    {
        var values = (YamlSequenceNode)((YamlMappingNode)Schemas()[schema])["enum"];
        Assert.That(values.Children.Cast<YamlScalarNode>().Select(x => x.Value), Is.EqualTo(Enum.GetNames(type)));
    }

    [Test]
    public void Every_operation_is_in_the_contract_and_needs_the_director_policy()
    {
        var endpoints = MappedEndpoints.Of(app => app.MapAccountingEndpoints());
        var policies = endpoints.SelectMany(x => x.Policies).Distinct().ToArray();
        var operations = endpoints.Select(x => x.Name).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(operations, Has.Length.EqualTo(51));
            Assert.That(policies, Is.EqualTo(new[] { "access:accounting" }));
            Assert.That(Operations(), Is.EquivalentTo(operations));
        });
    }

    private static YamlMappingNode Root()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        using var reader = File.OpenText(Path.Combine(directory!.FullName, "docs/sdd/contracts/accounting/finance.openapi.yaml"));
        var yaml = new YamlStream();
        yaml.Load(reader);
        return (YamlMappingNode)yaml.Documents[0].RootNode;
    }

    private static YamlMappingNode Schemas() =>
        (YamlMappingNode)((YamlMappingNode)Root()["components"])["schemas"];

    private static IEnumerable<string> Operations() =>
        ((YamlMappingNode)Root()["paths"]).Children.Values.Cast<YamlMappingNode>()
        .SelectMany(path => path.Children.Values.OfType<YamlMappingNode>())
        .Where(operation => operation.Children.ContainsKey("operationId"))
        .Select(operation => ((YamlScalarNode)operation["operationId"]).Value!);
}
