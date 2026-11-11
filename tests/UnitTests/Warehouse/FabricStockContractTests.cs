using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using Warehouse.Api.Stock;
using Warehouse.Application.Contracts;
using YamlDotNet.RepresentationModel;

namespace UnitTests.Warehouse;

/// <summary>Stock types and operations match docs/sdd/contracts/warehouse/fabric-stock.openapi.yaml (WH-001…WH-006).</summary>
public sealed class FabricStockContractTests
{
    [TestCase(typeof(FabricStockPermissions))]
    [TestCase(typeof(FabricStockSourceResponse))]
    [TestCase(typeof(FabricStockColorResponse))]
    [TestCase(typeof(FabricStockFabricResponse))]
    [TestCase(typeof(FabricStockListResponse))]
    [TestCase(typeof(FabricMovementRowResponse))]
    [TestCase(typeof(FabricMovementPageResponse))]
    [TestCase(typeof(FabricStockFabricChoice))]
    [TestCase(typeof(FabricStockChoicesResponse))]
    [TestCase(typeof(OpenFabricStockInput))]
    [TestCase(typeof(ReceiveFabricInput))]
    [TestCase(typeof(IssueFabricInput))]
    [TestCase(typeof(AdjustFabricStockInput))]
    [TestCase(typeof(ReverseFabricMovementInput))]
    [TestCase(typeof(FabricStockOperationResponse))]
    [TestCase(typeof(EditFabricMovementInput))]
    [TestCase(typeof(VoidFabricMovementInput))]
    [TestCase(typeof(RestoreFabricMovementInput))]
    [TestCase(typeof(FabricMovementStateResponse))]
    [TestCase(typeof(FabricMovementRevisionResponse))]
    [TestCase(typeof(FabricMovementHistoryResponse))]
    [TestCase(typeof(FabricPlanPartResponse))]
    [TestCase(typeof(FabricIssueLineResponse))]
    [TestCase(typeof(FabricIssueResponse))]
    [TestCase(typeof(ProductionItemFabricResponse))]
    [TestCase(typeof(FabricIssueLineInput))]
    [TestCase(typeof(CreateFabricIssueInput))]
    [TestCase(typeof(FabricIssueComparisonResponse))]
    [TestCase(typeof(RefreshItemFabricPlanInput))]
    [TestCase(typeof(FabricIssueItemResponse))]
    [TestCase(typeof(FabricIssueItemsResponse))]
    [TestCase(typeof(ReverseFabricIssueInput))]
    [TestCase(typeof(FabricConsumptionRowResponse))]
    [TestCase(typeof(FabricConsumptionResponse))]
    [TestCase(typeof(StockTransferInput))]
    [TestCase(typeof(V133StockRow))]
    [TestCase(typeof(V133MovementRow))]
    [TestCase(typeof(V133RevisionRow))]
    [TestCase(typeof(V133BatchRow))]
    [TestCase(typeof(V133IssueLineRow))]
    [TestCase(typeof(V133IssueComponentRow))]
    [TestCase(typeof(V133ReservationRow))]
    [TestCase(typeof(V133OrderRow))]
    [TestCase(typeof(V133ItemRow))]
    [TestCase(typeof(StockTransferResult))]
    [TestCase(typeof(StockTransferSource))]
    [TestCase(typeof(StockTransferTarget))]
    [TestCase(typeof(StockTransferCurrent))]
    [TestCase(typeof(StockTransferProblem))]
    [TestCase(typeof(FabricPlanFactResponse))]
    [TestCase(typeof(FabricPlanFactRowResponse))]
    [TestCase(typeof(FabricPlanFactModelResponse))]
    [TestCase(typeof(FabricPlanFactItemResponse))]
    [TestCase(typeof(FabricPlanFactMaterialResponse))]
    [TestCase(typeof(FabricBookingResponse))]
    [TestCase(typeof(FabricBookingPairResponse))]
    [TestCase(typeof(FabricBookingItemResponse))]
    [TestCase(typeof(FabricBookingUnknownResponse))]
    [TestCase(typeof(FabricValuationResponse))]
    [TestCase(typeof(FabricValuationFabricResponse))]
    [TestCase(typeof(FabricValuationColorResponse))]
    [TestCase(typeof(FabricValuationShortageResponse))]
    [TestCase(typeof(HardwareOrderResponse))]
    [TestCase(typeof(HardwareNeedResponse))]
    [TestCase(typeof(HardwareNeedOrderResponse))]
    [TestCase(typeof(HardwareIncompleteResponse))]
    public void Serialized_properties_match_the_machine_contract(Type type)
    {
        var properties = (YamlMappingNode)((YamlMappingNode)Schemas()[type.Name])["properties"];
        Assert.That(properties.Children.Keys.Cast<YamlScalarNode>().Select(x => x.Value),
            Is.EquivalentTo(type.GetProperties().Select(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name))));
    }

    [Test]
    public void Every_operation_is_in_the_contract_and_needs_the_orders_policy()
    {
        Type[] controllers = [typeof(FabricStockController), typeof(OrderItemFabricController)];
        var policies = controllers.Select(controller => controller.GetCustomAttributes(typeof(AuthorizeAttribute), false)
            .Cast<AuthorizeAttribute>().Single().Policy).Distinct().ToArray();
        var operations = controllers.SelectMany(controller => controller.GetMethods())
            .SelectMany(x => x.GetCustomAttributes(typeof(HttpMethodAttribute), false).Cast<HttpMethodAttribute>())
            .Select(x => x.Name).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(policies, Is.EqualTo(new[] { "access:fabric-stock", "access:production-orders" }));
            Assert.That(operations, Has.Length.EqualTo(22));
            Assert.That(Operations().Where(x => !Reports.Contains(x)), Is.EquivalentTo(operations));
        });
    }

    private static readonly string[] Reports = ["getFabricPlanFact", "getFabricBooking", "getFabricValuation", "getHardwareOrder"];

    [Test]
    public void Every_report_is_in_the_contract_and_is_its_own_page_of_access_rights()
    {
        var actions = typeof(StockReportsController).GetMethods()
            .Select(x => (Route: x.GetCustomAttributes(typeof(HttpMethodAttribute), false).Cast<HttpMethodAttribute>()
                .SingleOrDefault(), Policy: x.GetCustomAttributes(typeof(AuthorizeAttribute), false)
                .Cast<AuthorizeAttribute>().SingleOrDefault()?.Policy))
            .Where(x => x.Route is not null).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(actions.Select(x => x.Route!.Name), Is.EquivalentTo(Reports));
            Assert.That(Operations().Where(Reports.Contains), Is.EquivalentTo(Reports));
            Assert.That(actions.Select(x => x.Policy), Is.EquivalentTo(new[]
            {
                "access:fabric-plan-fact", "access:fabric-booking", "access:fabric-valuation", "access:hardware-order"
            }));
            Assert.That(actions.Select(x => global::Identity.Application.Security.Access.AccessPolicies.Parse(x.Policy!)),
                Has.None.Null, "every policy names a page of «Права доступу»");
            Assert.That(global::Identity.Domain.Authorization.AccessCatalog.DefaultsOf("Manager"),
                Does.Not.ContainKey(global::Identity.Domain.Authorization.AccessCatalog.FabricValuation), "the owner's page");
        });
    }

    private static YamlMappingNode Root()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        using var reader = File.OpenText(Path.Combine(directory!.FullName, "docs/sdd/contracts/warehouse/fabric-stock.openapi.yaml"));
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
