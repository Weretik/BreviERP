using System.Text.Json;
using Crm.Api.DependencyInjection;
using UnitTests.TestSupport;
using Crm.Application.Features.Tasks.Shared;
using Crm.Api.Endpoints.Tasks.ChangeCrmTaskStatus;
using Crm.Api.Endpoints.Tasks.MarkCrmTaskStep;
using Crm.Api.Endpoints.Tasks.SetCrmTaskUrgent;

namespace UnitTests.Crm.Tasks;

/// <summary>Manager task types and operations match the generated contract docs/sdd/contracts/openapi.json (TSK-001).</summary>
public sealed class ManagerTaskContractTests
{
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
        Assert.That(ApiContract.OperationIds, Does.Contain(operation));
    }
}
