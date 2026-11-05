using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using Production.Api.Dashboard;
using Production.Api.Reports;
using Production.Application.Contracts;
using YamlDotNet.RepresentationModel;

namespace UnitTests.Production;

/// <summary>Report and overview types and operations match docs/sdd/contracts/production/workshop-reports.openapi.yaml (PRD-007).</summary>
public sealed class WorkshopReportsContractTests
{
    [TestCase(typeof(WorkshopReportPairResponse))]
    [TestCase(typeof(WorkshopReportResponse))]
    [TestCase(typeof(WorkshopReportTotalsResponse))]
    [TestCase(typeof(ReportOutputRowResponse))]
    [TestCase(typeof(ReportStaffRowResponse))]
    [TestCase(typeof(ReportIssueRowResponse))]
    [TestCase(typeof(ReportStockRowResponse))]
    [TestCase(typeof(WorkshopDashboardResponse))]
    [TestCase(typeof(DashboardStageResponse))]
    [TestCase(typeof(DashboardOrderResponse))]
    [TestCase(typeof(DashboardHoursResponse))]
    public void Serialized_properties_match_the_machine_contract(Type type)
    {
        var properties = (YamlMappingNode)((YamlMappingNode)Schemas()[type.Name])["properties"];
        Assert.That(properties.Children.Keys.Cast<YamlScalarNode>().Select(x => x.Value),
            Is.EquivalentTo(type.GetProperties().Select(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name))));
    }

    [Test]
    public void Operations_are_in_the_contract_with_their_policies()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Policy(typeof(WorkshopReportsController)), Is.EqualTo("CanManageWorkshop"));
            Assert.That(Policy(typeof(WorkshopDashboardController)), Is.EqualTo("CanUseProductionOrders"));
            Assert.That(Operations(typeof(WorkshopReportsController)).Concat(Operations(typeof(WorkshopDashboardController))),
                Is.EquivalentTo(ContractOperations()));
        });
    }

    private static string? Policy(Type controller) => controller.GetCustomAttributes(typeof(AuthorizeAttribute), false)
        .Cast<AuthorizeAttribute>().Single().Policy;

    private static IEnumerable<string?> Operations(Type controller) => controller.GetMethods()
        .SelectMany(x => x.GetCustomAttributes(typeof(HttpMethodAttribute), false).Cast<HttpMethodAttribute>())
        .Select(x => x.Name);

    private static IEnumerable<string> ContractOperations() =>
        ((YamlMappingNode)Document()["paths"]).Children.Values.Cast<YamlMappingNode>()
        .SelectMany(path => path.Children.Values.Cast<YamlMappingNode>())
        .Select(operation => ((YamlScalarNode)operation["operationId"]).Value!);

    private static YamlMappingNode Schemas() =>
        (YamlMappingNode)((YamlMappingNode)Document()["components"])["schemas"];

    private static YamlMappingNode Document()
    {
        var path = Path.Combine(Root(), "docs", "sdd", "contracts", "production", "workshop-reports.openapi.yaml");
        var yaml = new YamlStream();
        using var reader = File.OpenText(path);
        yaml.Load(reader);
        return (YamlMappingNode)yaml.Documents[0].RootNode;
    }

    private static string Root()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
