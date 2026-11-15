using Production.Api.DependencyInjection;
using UnitTests.TestSupport;
using System.Text.Json;
using Production.Application.Features.FabricNorms.Commands.ImportFabricNorms;
using Production.Application.Features.FabricNorms.Queries.GetModelCompositions;
using Production.Application.Features.FabricNorms.Shared;
using Production.Application.Features.Staff.Commands.ImportWorkshopStaff;
using Production.Application.Features.Staff.Shared;

namespace UnitTests.Production.Workshop;

/// <summary>Workshop types and operations match the generated contract docs/sdd/contracts/openapi.json (PRD-001).</summary>
public sealed class WorkshopReferencesContractTests
{
    [TestCase("WorkshopStaff", "access:workshop-staff")]
    [TestCase("FabricNorms", "access:fabric-norms")]
    public void Every_operation_is_in_the_contract_and_needs_the_workshop_policy(string group, string expected)
    {
        var policy = MappedEndpoints.Policy(app => app.MapProductionEndpoints(), group);
        var operations = MappedEndpoints.Of(app => app.MapProductionEndpoints(), group).Select(x => x.Name).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(policy, Is.EqualTo(expected));
            Assert.That(operations, Is.Not.Empty);
            Assert.That(ApiContract.OperationIds, Is.SupersetOf(operations));
        });
    }
}
