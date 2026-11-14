using System.Text.Json;
using Crm.Api.DependencyInjection;
using UnitTests.TestSupport;
using YamlDotNet.RepresentationModel;
using Crm.Application.Features.Tasks.Shared;
using Crm.Api.Endpoints.Tasks.ChangeCrmTaskStatus;
using Crm.Api.Endpoints.Tasks.MarkCrmTaskStep;
using Crm.Api.Endpoints.Tasks.SetCrmTaskUrgent;

namespace UnitTests.Crm.Tasks;

/// <summary>Manager task types and operations match docs/sdd/contracts/crm/tasks.openapi.yaml (TSK-001).</summary>
public sealed class ManagerTaskContractTests
{
    [TestCase(typeof(TaskSummaryResponse), "CrmTaskSummary")]
    [TestCase(typeof(TaskPageResponse), "CrmTaskPage")]
    [TestCase(typeof(TaskBoardResponse), "CrmTaskBoard")]
    [TestCase(typeof(TaskDetailResponse), "CrmTask")]
    [TestCase(typeof(TaskStepResponse), "CrmTaskStep")]
    [TestCase(typeof(TaskEventResponse), "CrmTaskEvent")]
    [TestCase(typeof(TaskPersonResponse), "CrmTaskPerson")]
    [TestCase(typeof(TaskOverviewResponse), "CrmTaskOverview")]
    [TestCase(typeof(TaskLoadResponse), "CrmTaskLoad")]
    [TestCase(typeof(TaskAssigneeResponse), "CrmTaskAssignee")]
    [TestCase(typeof(SaveTaskInput), "CrmSaveTaskInput")]
    [TestCase(typeof(TaskStepInput), "CrmTaskStepInput")]
    [TestCase(typeof(ChangeTaskStatusRequest), "CrmChangeTaskStatusRequest")]
    [TestCase(typeof(SetTaskUrgentRequest), "CrmSetTaskUrgentRequest")]
    [TestCase(typeof(MarkTaskStepRequest), "CrmMarkTaskStepRequest")]
    public void Serialized_properties_match_the_machine_contract(Type type, string schema)
    {
        var properties = (YamlMappingNode)((YamlMappingNode)Schemas()[schema])["properties"];
        Assert.That(properties.Children.Keys.Cast<YamlScalarNode>().Select(x => x.Value),
            Is.EquivalentTo(type.GetProperties().Select(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name))));
    }

    [TestCase("getCrmTasks", "Admin,Manager")]
    [TestCase("getCrmTaskBoard", "Admin,Manager")]
    [TestCase("getCrmTaskOverview", "Admin,Manager")]
    [TestCase("getCrmTask", "Admin,Manager")]
    [TestCase("changeCrmTaskStatus", "Admin,Manager")]
    [TestCase("markCrmTaskStep", "Admin,Manager")]
    [TestCase("getCrmTaskAssignees", "Admin")]
    [TestCase("createCrmTask", "Admin")]
    [TestCase("editCrmTask", "Admin")]
    [TestCase("setCrmTaskUrgent", "Admin")]
    [TestCase("deleteCrmTask", "Admin")]
    public void Operation_exists_and_requires_the_expected_roles(string operation, string roles)
    {
        var required = MappedEndpoints.Named(app => app.MapCrmEndpoints(), operation).Roles ?? "Admin,Manager";
        Assert.That(required, Is.EqualTo(roles));
        Assert.That(Operations(), Does.Contain(operation));
    }

    private static YamlMappingNode Root()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        using var reader = File.OpenText(Path.Combine(directory!.FullName, "docs/sdd/contracts/crm/tasks.openapi.yaml"));
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
