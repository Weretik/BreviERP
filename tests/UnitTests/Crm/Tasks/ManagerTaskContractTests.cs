using System.Text.Json;
using Crm.Api.DependencyInjection;
using UnitTests.TestSupport;
using Crm.Application.Features.Tasks.Shared;
using Crm.Api.Endpoints.Tasks.ChangeCrmTaskStatus;
using Crm.Api.Endpoints.Tasks.MarkCrmTaskStep;
using Crm.Api.Endpoints.Tasks.SetCrmTaskUrgent;

namespace UnitTests.Crm.Tasks;

/// <summary>
/// Manager task types and operations match the generated contract docs/sdd/contracts/openapi.json (TSK-001); own tasks
/// are «Мої завдання», managing them is «Завдання менеджерам» (ACC-002 R-01), never a role.
/// </summary>
public sealed class ManagerTaskContractTests
{
    [TestCase("getCrmTasks", "access:tasks")]
    [TestCase("getCrmTaskBoard", "access:tasks")]
    [TestCase("getCrmTaskOverview", "access:tasks")]
    [TestCase("getCrmTask", "access:tasks")]
    [TestCase("changeCrmTaskStatus", "access:tasks")]
    [TestCase("markCrmTaskStep", "access:tasks")]
    [TestCase("getCrmTaskAssignees", "access:manager-tasks")]
    [TestCase("createCrmTask", "access:manager-tasks")]
    [TestCase("editCrmTask", "access:manager-tasks")]
    [TestCase("setCrmTaskUrgent", "access:manager-tasks")]
    [TestCase("deleteCrmTask", "access:manager-tasks")]
    public void Operation_exists_and_needs_its_page_of_access_rights(string operation, string policy)
    {
        var endpoint = MappedEndpoints.Named(app => app.MapCrmEndpoints(), operation);
        Assert.That(endpoint.Policies, Is.EqualTo(new[] { policy }));
        Assert.That(endpoint.Roles, Is.Null);
        Assert.That(ApiContract.OperationIds, Does.Contain(operation));
    }
}
