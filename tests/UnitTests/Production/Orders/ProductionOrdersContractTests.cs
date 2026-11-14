using Production.Api.DependencyInjection;
using UnitTests.TestSupport;
using System.Text.Json;
using YamlDotNet.RepresentationModel;
using Production.Application.Features.Orders.Commands.CancelProductionOrder;
using Production.Application.Features.Orders.Commands.CopyProductionOrder;
using Production.Application.Features.Orders.Commands.CreateProductionOrder;
using Production.Application.Features.Orders.Commands.CreateProductionShipment;
using Production.Application.Features.Orders.Commands.ImportProductionOrders;
using Production.Application.Features.Orders.Commands.ImportProductionProgress;
using Production.Application.Features.Orders.Commands.ImportProductionReportPrices;
using Production.Application.Features.Orders.Commands.SetProductionItemStages;
using Production.Application.Features.Orders.Commands.UpdateProductionOrder;
using Production.Application.Features.Orders.Queries.GetProductionEmbroideries;
using Production.Application.Features.Orders.Queries.GetProductionModelSummary;
using Production.Application.Features.Orders.Queries.GetProductionOrderFilterOptions;
using Production.Application.Features.Orders.Queries.GetProductionOrders;
using Production.Application.Features.Orders.Queries.GetProductionQuoteDraft;
using Production.Application.Features.Orders.Queries.GetProductionQuoteSources;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Settings.Commands.UpdateWorkshopSettings;
using Production.Application.Features.Settings.Shared;
using Production.Application.Features.Orders.Commands.ImportProductionOrders.Rows;
using Production.Application.Features.Orders.Commands.ImportProductionProgress.Rows;
using Production.Application.Features.Orders.Commands.ImportProductionReportPrices.Rows;
using Production.Application.Features.Orders.Queries.GetProductionOrders.Results;
using Production.Application.Features.Orders.Queries.GetProductionQuoteDraft.Results;
using Production.Application.Features.Orders.Shared.Items;
using Production.Application.Features.Orders.Shared.Responses;

namespace UnitTests.Production.Orders;

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
    [TestCase(typeof(ProductionOrderSummaryItem))]
    [TestCase(typeof(ProductionStageCount))]
    [TestCase(typeof(ProductionOrderAttention))]
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
        var endpoints = MappedEndpoints.Of(app => app.MapProductionEndpoints(), "ProductionOrders", "ProductionOrderTracking", "WorkshopSettings");
        var policies = endpoints.SelectMany(x => x.Policies).Distinct().ToArray();
        var operations = endpoints.Select(x => x.Name).ToArray();

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
