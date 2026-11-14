using System.Text.Json;
using Scheduling.Api.DependencyInjection;
using Scheduling.Application.Features.Tasks.Commands.UpdateScheduledTaskSchedule;
using Scheduling.Application.Features.Tasks.Shared;
using UnitTests.TestSupport;
using YamlDotNet.RepresentationModel;

namespace UnitTests.Scheduling.Api;

/// <summary>
/// «Фонові задачі» types, operations and access match docs/sdd/contracts/scheduling/background-tasks.openapi.yaml
/// (SCH-001 SC-09, SC-10).
/// </summary>
public sealed class SchedulingContractTests
{
    [TestCase(typeof(ScheduledTaskResponse), "ScheduledTaskResponse")]
    [TestCase(typeof(ScheduledTaskRunResponse), "ScheduledTaskRunResponse")]
    [TestCase(typeof(ScheduleInput), "ScheduleInput")]
    [TestCase(typeof(ScheduledTaskScheduleInput), "ScheduledTaskScheduleInput")]
    public void Serialized_properties_match_the_machine_contract(Type type, string schema)
    {
        var properties = (YamlMappingNode)((YamlMappingNode)Schemas()[schema])["properties"];
        Assert.That(properties.Children.Keys.Cast<YamlScalarNode>().Select(x => x.Value),
            Is.EquivalentTo(type.GetProperties().Select(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name))));
    }

    [TestCase("getScheduledTasks", "GET")]
    [TestCase("getScheduledTaskRuns", "GET")]
    [TestCase("updateScheduledTaskSchedule", "PUT")]
    [TestCase("runScheduledTaskNow", "POST")]
    public void Operation_needs_the_background_tasks_page(string operation, string method)
    {
        var endpoint = MappedEndpoints.Named(app => app.MapSchedulingEndpoints(), operation);
        Assert.Multiple(() =>
        {
            Assert.That(endpoint.Tag, Is.EqualTo("ScheduledTasks"));
            Assert.That(endpoint.Method, Is.EqualTo(method), "the HTTP method names the «Права доступу» action");
            Assert.That(endpoint.Policies, Is.EqualTo(new[] { "access:background-tasks" }));
            Assert.That(endpoint.Action, Is.Null);
            Assert.That(Operations(), Does.Contain(operation));
        });
    }

    [Test]
    public void Tick_is_outside_the_contract_and_checks_the_caller_itself()
    {
        var tick = MappedEndpoints.Of(app => app.MapSchedulingEndpoints()).Single(x => x.Route == "/internal/scheduler/tick");
        Assert.That(tick.Anonymous, Is.True, "the Google OIDC token is checked in the endpoint (R-10)");
        Assert.That(tick.Method, Is.EqualTo("POST"));
        Assert.That(((YamlMappingNode)Root()["paths"]).Children.Keys.Cast<YamlScalarNode>().Select(x => x.Value),
            Has.None.Contains("internal"));
    }

    private static YamlMappingNode Root()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        using var reader = File.OpenText(Path.Combine(directory!.FullName, "docs/sdd/contracts/scheduling/background-tasks.openapi.yaml"));
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
