using Production.Api.DependencyInjection;
using UnitTests.TestSupport;
using System.Text.Json;
using YamlDotNet.RepresentationModel;
using Production.Application.Features.Forecast.Commands.SaveForecastQueue;
using Production.Application.Features.Forecast.Queries.GetForecast;
using Production.Application.Features.Work.Commands.CreateProductionWork;
using Production.Application.Features.Work.Commands.ImportProductionWork;
using Production.Application.Features.Work.Commands.VoidProductionWork;
using Production.Application.Features.Work.Queries.GetWorkChoices;
using Production.Application.Features.Work.Queries.GetWorkLog;
using Production.Application.Features.Work.Shared;

namespace UnitTests.Production.Payroll;

/// <summary>Work log and forecast types and operations match docs/sdd/contracts/production/work-log.openapi.yaml (PRD-005).</summary>
public sealed class ProductionWorkContractTests
{
    [TestCase(typeof(ProductionWorkResponse))]
    [TestCase(typeof(WorkEmployeeChoice))]
    [TestCase(typeof(WorkItemChoice))]
    [TestCase(typeof(WorkChoicesResponse))]
    [TestCase(typeof(WorkLogTotals))]
    [TestCase(typeof(WorkLogResponse))]
    [TestCase(typeof(ProductionItemWorkResponse))]
    [TestCase(typeof(CreateProductionWorkInput))]
    [TestCase(typeof(VoidProductionWorkInput))]
    [TestCase(typeof(ProductionWorkImportRow))]
    [TestCase(typeof(ProductionWorkImportInput))]
    [TestCase(typeof(ProductionWorkImportLine))]
    [TestCase(typeof(ProductionWorkImportResult))]
    [TestCase(typeof(ForecastRowResponse))]
    [TestCase(typeof(ForecastSettingsResponse))]
    [TestCase(typeof(ForecastResponse))]
    [TestCase(typeof(SaveForecastQueueInput))]
    public void Serialized_properties_match_the_machine_contract(Type type)
    {
        var properties = (YamlMappingNode)((YamlMappingNode)Schemas()[type.Name])["properties"];
        Assert.That(properties.Children.Keys.Cast<YamlScalarNode>().Select(x => x.Value),
            Is.EquivalentTo(type.GetProperties().Select(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name))));
    }

    [Test]
    public void Every_operation_is_in_the_contract_and_needs_the_orders_policy()
    {
        var endpoints = MappedEndpoints.Of(app => app.MapProductionEndpoints(), "ProductionWork", "ProductionForecast");
        var policies = endpoints.SelectMany(x => x.Policies).Distinct().ToArray();
        var operations = endpoints.Select(x => x.Name).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(policies, Is.EqualTo(new[] { "access:work-log", "access:forecast" }));
            Assert.That(operations, Has.Length.EqualTo(8));
            Assert.That(Operations(), Is.EquivalentTo(operations));
        });
    }

    private static YamlMappingNode Root()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        using var reader = File.OpenText(Path.Combine(directory!.FullName, "docs/sdd/contracts/production/work-log.openapi.yaml"));
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
