using Production.Api.DependencyInjection;
using UnitTests.TestSupport;
using System.Text.Json;
using Production.Application.Features.Dashboard.Queries.GetWorkshopDashboard;
using Production.Application.Features.Reports.Queries.GetWorkshopReport;
using Production.Application.Features.Reports.Shared;
using Production.Application.Features.Dashboard.Queries.GetWorkshopDashboard.Results;
using Production.Application.Features.Reports.Shared.Rows;

namespace UnitTests.Production.Workshop;

/// <summary>Report and overview types and operations match the generated contract docs/sdd/contracts/openapi.json (PRD-007).</summary>
public sealed class WorkshopReportsContractTests
{
    [Test]
    public void Operations_are_in_the_contract_with_their_policies()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Policy("WorkshopReports"), Is.EqualTo("access:workshop-reports"));
            Assert.That(Policy("WorkshopDashboard"), Is.EqualTo("access:production-orders"));
            Assert.That(ApiContract.OperationIds,
                Is.SupersetOf(Operations("WorkshopReports").Concat(Operations("WorkshopDashboard"))));
        });
    }

    private static string Policy(string group) => MappedEndpoints.Policy(app => app.MapProductionEndpoints(), group);

    private static IEnumerable<string> Operations(string group) =>
        MappedEndpoints.Of(app => app.MapProductionEndpoints(), group).Select(x => x.Name);
}
