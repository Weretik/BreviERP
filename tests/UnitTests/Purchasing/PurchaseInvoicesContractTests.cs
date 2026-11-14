using System.Text.Json;
using Purchasing.Api.DependencyInjection;
using Purchasing.Api.Endpoints.Documents;
using Purchasing.Api.Endpoints.Register;
using UnitTests.TestSupport;
using YamlDotNet.RepresentationModel;
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

namespace UnitTests.Purchasing;

/// <summary>Register types and operations match docs/sdd/contracts/purchasing/purchase-invoices.openapi.yaml (PUR-001).</summary>
public sealed class PurchaseInvoicesContractTests
{
    [TestCase(typeof(PurchaseInvoicePage), "PurchaseInvoicePage")]
    [TestCase(typeof(PurchaseInvoiceResponse), "PurchaseInvoiceResponse")]
    [TestCase(typeof(PurchaseInvoiceLinkResponse), "PurchaseInvoiceLinkResponse")]
    [TestCase(typeof(PurchaseSyncStateResponse), "PurchaseSyncStateResponse")]
    [TestCase(typeof(PurchaseSyncResponse), "PurchaseSyncResponse")]
    [TestCase(typeof(PurchaseSupplierOption), "PurchaseSupplierOption")]
    [TestCase(typeof(PurchaseInvoiceReviewRequest), "PurchaseInvoiceReviewRequest")]
    [TestCase(typeof(PurchaseInvoiceReviewResponse), "PurchaseInvoiceReviewResponse")]
    [TestCase(typeof(PurchaseDocumentResponse), "PurchaseDocumentResponse")]
    [TestCase(typeof(PurchaseDocumentItemResponse), "PurchaseDocumentItemResponse")]
    [TestCase(typeof(PurchaseRecognitionAttemptResponse), "PurchaseRecognitionAttemptResponse")]
    [TestCase(typeof(PurchaseRecognitionResponse), "PurchaseRecognitionResponse")]
    [TestCase(typeof(PurchaseRecognitionBatchResponse), "PurchaseRecognitionBatchResponse")]
    [TestCase(typeof(PurchaseDocumentSaveRequest), "PurchaseDocumentSaveRequest")]
    [TestCase(typeof(PurchaseDocumentSaveResponse), "PurchaseDocumentSaveResponse")]
    [TestCase(typeof(PurchasePaymentRequest), "PurchasePaymentRequest")]
    [TestCase(typeof(PurchaseInvoicePaymentResponse), "PurchaseInvoicePaymentResponse")]
    [TestCase(typeof(PurchasePaymentCandidateResponse), "PurchasePaymentCandidateResponse")]
    [TestCase(typeof(PurchaseInvoicePaymentsResponse), "PurchaseInvoicePaymentsResponse")]
    public void Serialized_properties_match_the_machine_contract(Type type, string schema)
    {
        var properties = (YamlMappingNode)((YamlMappingNode)Schemas()[schema])["properties"];
        Assert.That(properties.Children.Keys.Cast<YamlScalarNode>().Select(x => x.Value),
            Is.EquivalentTo(type.GetProperties().Select(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name))));
    }

    [TestCase(typeof(PurchaseInvoiceState), "PurchaseInvoiceState")]
    [TestCase(typeof(PurchaseInvoiceFilter), "PurchaseInvoiceFilter")]
    [TestCase(typeof(PurchaseDocumentVat), "PurchaseDocumentVat")]
    [TestCase(typeof(PurchaseDocumentSource), "PurchaseDocumentSource")]
    [TestCase(typeof(PurchaseRecognitionResult), "PurchaseRecognitionResult")]
    public void Enum_values_match_the_machine_contract(Type type, string schema)
    {
        var values = (YamlSequenceNode)((YamlMappingNode)Schemas()[schema])["enum"];
        Assert.That(values.Children.Cast<YamlScalarNode>().Select(x => x.Value), Is.EqualTo(Enum.GetNames(type)));
    }

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
            Assert.That(Operations(), Is.EquivalentTo(operations));
        });
    }

    private static YamlMappingNode Root()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        using var reader = File.OpenText(Path.Combine(directory!.FullName, "docs/sdd/contracts/purchasing/purchase-invoices.openapi.yaml"));
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
