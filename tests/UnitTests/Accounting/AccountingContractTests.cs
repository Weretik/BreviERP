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

/// <summary>Types and operations match the generated contract docs/sdd/contracts/openapi.json (ACC-001).</summary>
public sealed class AccountingContractTests
{
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
            Assert.That(ApiContract.OperationIds, Is.SupersetOf(operations));
        });
    }
}
