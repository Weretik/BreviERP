using Production.Api.DependencyInjection;
using UnitTests.TestSupport;
using System.Text.Json;
using YamlDotNet.RepresentationModel;
using Production.Application.Features.Dashboard.Queries.GetWorkshopDashboard;
using Production.Application.Features.Reports.Queries.GetWorkshopReport;
using Production.Application.Features.Reports.Shared;
using Production.Application.Features.Dashboard.Queries.GetWorkshopDashboard.Results;
using Production.Application.Features.Reports.Shared.Rows;

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
            Assert.That(Policy("WorkshopReports"), Is.EqualTo("access:workshop-reports"));
            Assert.That(Policy("WorkshopDashboard"), Is.EqualTo("access:production-orders"));
            Assert.That(Operations("WorkshopReports").Concat(Operations("WorkshopDashboard")),
                Is.EquivalentTo(ContractOperations()));
        });
    }

    private static string Policy(string group) => MappedEndpoints.Policy(app => app.MapProductionEndpoints(), group);

    private static IEnumerable<string> Operations(string group) =>
        MappedEndpoints.Of(app => app.MapProductionEndpoints(), group).Select(x => x.Name);

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
