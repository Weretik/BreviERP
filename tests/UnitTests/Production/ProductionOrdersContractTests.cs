using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using Production.Api.Orders;
using Production.Api.Settings;
using Production.Application.Contracts;
using YamlDotNet.RepresentationModel;

namespace UnitTests.Production;

/// <summary>Order types and operations match docs/sdd/contracts/production/production-orders.openapi.yaml (PRD-002, PRD-003).</summary>
public sealed class ProductionOrdersContractTests
{
    [TestCase(typeof(ItemApplicationInput))]
    [TestCase(typeof(ProductionItemDetailsInput))]
    [TestCase(typeof(CreateProductionItemInput))]
    [TestCase(typeof(CreateProductionOrderInput))]
    [TestCase(typeof(UpdateProductionItemInput))]
    [TestCase(typeof(UpdateProductionOrderInput))]
    [TestCase(typeof(CopyProductionOrderInput))]
    [TestCase(typeof(ProductionOrderImportRow))]
    [TestCase(typeof(ProductionItemImportRow))]
    [TestCase(typeof(ProductionOrderSummaryResponse))]
    [TestCase(typeof(ProductionOrderCounts))]
    [TestCase(typeof(ProductionOrderPageResponse))]
    [TestCase(typeof(ProductionModelSummaryRow))]
    [TestCase(typeof(ProductionModelSummaryResponse))]
    [TestCase(typeof(ProductionFilterOption))]
    [TestCase(typeof(ProductionOrderFilterOptionsResponse))]
    [TestCase(typeof(ProductionOrderPermissions))]
    [TestCase(typeof(ItemApplicationResponse))]
    [TestCase(typeof(ProductionItemResponse))]
    [TestCase(typeof(ProductionOrderResponse))]
    [TestCase(typeof(ProductionOrderChangeResponse))]
    [TestCase(typeof(ProductionQuoteSourceResponse))]
    [TestCase(typeof(ProductionQuoteDraftResponse))]
    [TestCase(typeof(ProductionQuoteDraftItem))]
    [TestCase(typeof(ProductionQuoteDraftOption))]
    [TestCase(typeof(ProductionQuoteDraftEmbroidery))]
    [TestCase(typeof(ProductionEmbroideryResponse))]
    [TestCase(typeof(ProductionEmbroideryChoiceResponse))]
    [TestCase(typeof(ProductionOrderImportResult))]
    [TestCase(typeof(ProductionOrderImportLine))]
    [TestCase(typeof(ProductionShipmentResponse))]
    [TestCase(typeof(ProductionFileResponse))]
    [TestCase(typeof(ProductionTelegramResponse))]
    [TestCase(typeof(ProductionOrderVersionInput))]
    [TestCase(typeof(SetProductionItemStagesInput))]
    [TestCase(typeof(CreateProductionShipmentInput))]
    [TestCase(typeof(CancelProductionOrderInput))]
    [TestCase(typeof(WorkshopSettingsInput))]
    [TestCase(typeof(WorkshopSettingsResponse))]
    [TestCase(typeof(WorkshopTelegramChatResponse))]
    [TestCase(typeof(ProductionProgressImportRow))]
    [TestCase(typeof(ProductionItemProgressImportRow))]
    [TestCase(typeof(ProductionFileImportRow))]
    [TestCase(typeof(ProductionProgressImportResult))]
    [TestCase(typeof(ProductionProgressImportLine))]
    [TestCase(typeof(ProductionReportPriceImportRow))]
    [TestCase(typeof(ProductionItemReportPriceImportRow))]
    [TestCase(typeof(ProductionReportPriceImportResult))]
    [TestCase(typeof(ProductionReportPriceImportLine))]
    public void Serialized_properties_match_the_machine_contract(Type type)
    {
        var properties = (YamlMappingNode)((YamlMappingNode)Schemas()[type.Name])["properties"];
        Assert.That(properties.Children.Keys.Cast<YamlScalarNode>().Select(x => x.Value),
            Is.EquivalentTo(type.GetProperties().Select(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name))));
    }

    [Test]
    public void Every_operation_is_in_the_contract_and_needs_the_orders_policy()
    {
        Type[] controllers =
            [typeof(ProductionOrdersController), typeof(ProductionOrderTrackingController), typeof(WorkshopSettingsController)];
        var policies = controllers.Select(controller => controller.GetCustomAttributes(typeof(AuthorizeAttribute), false)
            .Cast<AuthorizeAttribute>().Single().Policy).Distinct().ToArray();
        var operations = controllers.SelectMany(controller => controller.GetMethods())
            .SelectMany(x => x.GetCustomAttributes(typeof(HttpMethodAttribute), false).Cast<HttpMethodAttribute>())
            .Select(x => x.Name).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(policies, Is.EqualTo(new[] { "access:production-orders", "access:workshop-settings" }));
            Assert.That(operations, Has.Length.EqualTo(28));
            Assert.That(Operations(), Is.EquivalentTo(operations));
        });
    }

    private static YamlMappingNode Root()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        using var reader = File.OpenText(Path.Combine(directory!.FullName, "docs/sdd/contracts/production/production-orders.openapi.yaml"));
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
