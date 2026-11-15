using Production.Api.DependencyInjection;
using UnitTests.TestSupport;
using System.Text.Json;
using Production.Application.Features.Archive.Commands.ImportWorkshopArchive;
using Production.Application.Features.Archive.Queries.GetWorkshopArchive;
using Production.Application.Features.Archive.Queries.GetWorkshopArchive.Results;

namespace UnitTests.Production.Workshop;

/// <summary>Archive types and operations match the generated contract docs/sdd/contracts/openapi.json (PRD-008).</summary>
public sealed class WorkshopArchiveContractTests
{
    [Test]
    public void Operations_are_in_the_contract_with_their_policies()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Policy("WorkshopArchive"), Is.EqualTo("access:workshop-archive"));
            Assert.That(ApiContract.OperationIds, Is.SupersetOf(Operations("WorkshopArchive")));
        });
    }

    private static string Policy(string group) => MappedEndpoints.Policy(app => app.MapProductionEndpoints(), group);

    private static IEnumerable<string> Operations(string group) =>
        MappedEndpoints.Of(app => app.MapProductionEndpoints(), group).Select(x => x.Name);
}
