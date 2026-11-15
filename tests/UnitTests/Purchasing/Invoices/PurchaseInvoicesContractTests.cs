using System.Text.Json;
using Purchasing.Api.DependencyInjection;
using Purchasing.Api.Endpoints.Documents;
using Purchasing.Api.Endpoints.Register;
using UnitTests.TestSupport;
using Purchasing.Application.Features.Documents.Commands.RecognizeNewPurchaseFiles;
using Purchasing.Application.Features.Documents.Commands.SaveCheckedPurchaseDocument;
using Purchasing.Application.Features.Documents.Shared;
using Purchasing.Application.Features.Register.Commands.ReviewPurchaseInvoice;
using Purchasing.Application.Features.Register.Commands.SyncPurchaseRegister;
using Purchasing.Application.Features.Register.Queries.GetPurchaseInvoices;
using Purchasing.Application.Features.Register.Queries.GetPurchasePaymentCandidates;
using Purchasing.Application.Features.Register.Queries.GetPurchaseSuppliers;
using Purchasing.Application.Features.Register.Queries.GetPurchaseSyncState;
using Purchasing.Application.Features.Register.Shared;
using Purchasing.Api.Endpoints.Documents.SavePurchaseDocument;
using Purchasing.Api.Endpoints.Register.ReviewPurchaseInvoice;
using Purchasing.Api.Endpoints.Register.Shared;
using Purchasing.Application.Features.Documents.Shared.Recognition;
using Purchasing.Application.Features.Register.Queries.GetPurchaseInvoices.Results;

namespace UnitTests.Purchasing.Invoices;

/// <summary>Register types and operations match the generated contract docs/sdd/contracts/openapi.json (PUR-001).</summary>
public sealed class PurchaseInvoicesContractTests
{
    [Test]
    public void Every_operation_is_in_the_contract_and_needs_the_purchase_policy()
    {
        var endpoints = MappedEndpoints.Of(app => app.MapPurchasingEndpoints());
        var policies = endpoints.SelectMany(x => x.Policies).Distinct().ToArray();
        var operations = endpoints.Select(x => x.Name).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(policies, Is.EqualTo(new[] { "access:purchase-invoices" }));
            Assert.That(operations, Has.Length.EqualTo(12));
            Assert.That(ApiContract.OperationIds, Is.SupersetOf(operations));
        });
    }
}
