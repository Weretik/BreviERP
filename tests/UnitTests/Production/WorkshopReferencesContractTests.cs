using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using Production.Api.FabricNorms;
using Production.Api.Staff;
using YamlDotNet.RepresentationModel;
using Production.Application.Features.FabricNorms.Commands.ImportFabricNorms;
using Production.Application.Features.FabricNorms.Queries.GetModelCompositions;
using Production.Application.Features.FabricNorms.Shared;
using Production.Application.Features.Staff.Commands.ImportWorkshopStaff;
using Production.Application.Features.Staff.Shared;

namespace UnitTests.Production;

/// <summary>Workshop types and operations match docs/sdd/contracts/production/workshop-references.openapi.yaml (PRD-001).</summary>
public sealed class WorkshopReferencesContractTests
{
    [TestCase(typeof(WorkshopEmployeeInput), "WorkshopEmployeeInput")]
    [TestCase(typeof(WorkshopEmployeeResponse), "WorkshopEmployeeResponse")]
    [TestCase(typeof(WorkshopStaffImportResult), "WorkshopStaffImportResult")]
    [TestCase(typeof(FabricNormInput), "FabricNormInput")]
    [TestCase(typeof(FabricNormRowResponse), "FabricNormRowResponse")]
    [TestCase(typeof(FabricNormImportRow), "FabricNormImportRow")]
    [TestCase(typeof(FabricNormImportResult), "FabricNormImportResult")]
    [TestCase(typeof(ModelCompositionResponse), "ModelCompositionResponse")]
    [TestCase(typeof(ModelLayerResponse), "ModelLayerResponse")]
    [TestCase(typeof(ModelCompositionLineResponse), "ModelCompositionLineResponse")]
    public void Serialized_properties_match_the_machine_contract(Type type, string schema)
    {
        var properties = (YamlMappingNode)((YamlMappingNode)Schemas()[schema])["properties"];
        Assert.That(properties.Children.Keys.Cast<YamlScalarNode>().Select(x => x.Value),
            Is.EquivalentTo(type.GetProperties().Select(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name))));
    }

    [TestCase(typeof(WorkshopStaffController), "access:workshop-staff")]
    [TestCase(typeof(FabricNormsController), "access:fabric-norms")]
    public void Every_operation_is_in_the_contract_and_needs_the_workshop_policy(Type controller, string expected)
    {
        var policy = controller.GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>().Single().Policy;
        var operations = controller.GetMethods()
            .SelectMany(x => x.GetCustomAttributes(typeof(HttpMethodAttribute), false).Cast<HttpMethodAttribute>())
            .Select(x => x.Name).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(policy, Is.EqualTo(expected));
            Assert.That(operations, Is.Not.Empty);
            Assert.That(Operations(), Is.SupersetOf(operations));
        });
    }

    private static YamlMappingNode Root()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        using var reader = File.OpenText(Path.Combine(directory!.FullName, "docs/sdd/contracts/production/workshop-references.openapi.yaml"));
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
