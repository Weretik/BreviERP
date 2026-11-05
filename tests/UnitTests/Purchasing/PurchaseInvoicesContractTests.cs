using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using Purchasing.Api.Register;
using Purchasing.Application.Contracts;
using YamlDotNet.RepresentationModel;

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
    public void Serialized_properties_match_the_machine_contract(Type type, string schema)
    {
        var properties = (YamlMappingNode)((YamlMappingNode)Schemas()[schema])["properties"];
        Assert.That(properties.Children.Keys.Cast<YamlScalarNode>().Select(x => x.Value),
            Is.EquivalentTo(type.GetProperties().Select(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name))));
    }

    [TestCase(typeof(PurchaseInvoiceState), "PurchaseInvoiceState")]
    [TestCase(typeof(PurchaseInvoiceFilter), "PurchaseInvoiceFilter")]
    public void Enum_values_match_the_machine_contract(Type type, string schema)
    {
        var values = (YamlSequenceNode)((YamlMappingNode)Schemas()[schema])["enum"];
        Assert.That(values.Children.Cast<YamlScalarNode>().Select(x => x.Value), Is.EqualTo(Enum.GetNames(type)));
    }

    [Test]
    public void Every_operation_is_in_the_contract_and_needs_the_purchase_policy()
    {
        var controller = typeof(PurchaseInvoicesController);
        var policy = controller.GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>().Single().Policy;
        var operations = controller.GetMethods()
            .SelectMany(x => x.GetCustomAttributes(typeof(HttpMethodAttribute), false).Cast<HttpMethodAttribute>())
            .Select(x => x.Name).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(policy, Is.EqualTo("CanUsePurchaseInvoices"));
            Assert.That(operations, Has.Length.EqualTo(5));
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
