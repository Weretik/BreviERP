using System.Text.Json;
using Scheduling.Api.DependencyInjection;
using Scheduling.Application.Features.Tasks.Commands.UpdateScheduledTaskSchedule;
using Scheduling.Application.Features.Tasks.Shared;
using UnitTests.TestSupport;

namespace UnitTests.Scheduling.Api;

/// <summary>
/// «Фонові задачі» types, operations and access match the generated contract docs/sdd/contracts/openapi.json
/// (SCH-001 SC-09, SC-10).
/// </summary>
public sealed class SchedulingContractTests
{
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
            Assert.That(ApiContract.OperationIds, Does.Contain(operation));
        });
    }

    [Test]
    public void Tick_is_outside_the_contract_and_checks_the_caller_itself()
    {
        var tick = MappedEndpoints.Of(app => app.MapSchedulingEndpoints()).Single(x => x.Route == "/internal/scheduler/tick");
        Assert.That(tick.Anonymous, Is.True, "the Google OIDC token is checked in the endpoint (R-10)");
        Assert.That(tick.Method, Is.EqualTo("POST"));
        Assert.That(ApiContract.Paths,
            Has.None.Contains("internal"));
    }
}
