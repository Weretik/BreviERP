using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using Production.Api.Orders;
using Production.Api.Stock;
using Production.Application.Contracts;
using YamlDotNet.RepresentationModel;

namespace UnitTests.Production;

/// <summary>Stock types and operations match docs/sdd/contracts/production/fabric-stock.openapi.yaml (PRD-004).</summary>
public sealed class FabricStockContractTests
{
    [TestCase(typeof(FabricStockPermissions))]
    [TestCase(typeof(FabricStockFabricChoice))]
    [TestCase(typeof(FabricStockSupplierChoice))]
    [TestCase(typeof(FabricStockChoicesResponse))]
    [TestCase(typeof(FabricStockRowResponse))]
    [TestCase(typeof(FabricStockTotals))]
    [TestCase(typeof(FabricStockListResponse))]
    [TestCase(typeof(FabricMovementResponse))]
    [TestCase(typeof(FabricReservationResponse))]
    [TestCase(typeof(FabricStockLineResponse))]
    [TestCase(typeof(CreateFabricMovementInput))]
    [TestCase(typeof(CorrectFabricStockInput))]
    [TestCase(typeof(ReverseFabricMovementInput))]
    [TestCase(typeof(FabricPlanPartResponse))]
    [TestCase(typeof(FabricIssueLineResponse))]
    [TestCase(typeof(FabricIssueResponse))]
    [TestCase(typeof(ProductionItemFabricResponse))]
    [TestCase(typeof(FabricIssueLineInput))]
    [TestCase(typeof(CreateFabricIssueInput))]
    [TestCase(typeof(ReverseFabricIssueInput))]
    [TestCase(typeof(FabricConsumptionRowResponse))]
    [TestCase(typeof(FabricConsumptionResponse))]
    [TestCase(typeof(FabricStockImportRow))]
    [TestCase(typeof(FabricMovementImportRow))]
    [TestCase(typeof(FabricIssueImportRow))]
    [TestCase(typeof(FabricIssueLineImportRow))]
    [TestCase(typeof(FabricReservationImportRow))]
    [TestCase(typeof(FabricStockImportInput))]
    [TestCase(typeof(FabricStockImportLine))]
    [TestCase(typeof(FabricStockImportResult))]
    public void Serialized_properties_match_the_machine_contract(Type type)
    {
        var properties = (YamlMappingNode)((YamlMappingNode)Schemas()[type.Name])["properties"];
        Assert.That(properties.Children.Keys.Cast<YamlScalarNode>().Select(x => x.Value),
            Is.EquivalentTo(type.GetProperties().Select(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name))));
    }

    [Test]
    public void Every_operation_is_in_the_contract_and_needs_the_orders_policy()
    {
        Type[] controllers = [typeof(FabricStockController), typeof(ProductionOrderFabricController)];
        var policies = controllers.Select(controller => controller.GetCustomAttributes(typeof(AuthorizeAttribute), false)
            .Cast<AuthorizeAttribute>().Single().Policy).Distinct().ToArray();
        var operations = controllers.SelectMany(controller => controller.GetMethods())
            .SelectMany(x => x.GetCustomAttributes(typeof(HttpMethodAttribute), false).Cast<HttpMethodAttribute>())
            .Select(x => x.Name).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(policies, Is.EqualTo(new[] { "CanUseProductionOrders" }));
            Assert.That(operations, Has.Length.EqualTo(11));
            Assert.That(Operations(), Is.EquivalentTo(operations));
        });
    }

    private static YamlMappingNode Root()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        using var reader = File.OpenText(Path.Combine(directory!.FullName, "docs/sdd/contracts/production/fabric-stock.openapi.yaml"));
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
