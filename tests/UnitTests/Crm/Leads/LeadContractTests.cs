using System.Text.Json;
using Crm.Api.DependencyInjection;
using UnitTests.TestSupport;
using YamlDotNet.RepresentationModel;
using Crm.Application.Features.Leads.Shared;

namespace UnitTests.Crm.Leads;

/// <summary>Lead types, operations and policies match docs/sdd/contracts/crm/leads.openapi.yaml (LEAD-001).</summary>
public sealed class LeadContractTests
{
    [TestCase(typeof(LeadSummaryResponse), "CrmLeadSummary")]
    [TestCase(typeof(LeadPageResponse), "CrmLeadPage")]
    [TestCase(typeof(LeadDetailResponse), "CrmLead")]
    [TestCase(typeof(LeadCallResponse), "CrmLeadCall")]
    [TestCase(typeof(LeadPersonResponse), "CrmLeadPerson")]
    [TestCase(typeof(LeadMyDayResponse), "CrmLeadMyDay")]
    [TestCase(typeof(LeadCallResultResponse), "CrmLeadCallResult")]
    [TestCase(typeof(LeadCategoryResponse), "CrmLeadCategory")]
    [TestCase(typeof(LeadSyncResponse), "CrmLeadSync")]
    [TestCase(typeof(LeadSyncStateResponse), "CrmLeadSyncState")]
    [TestCase(typeof(LeadManagerStatsResponse), "CrmLeadManagerStats")]
    [TestCase(typeof(LeadCountResponse), "CrmLeadCount")]
    [TestCase(typeof(LeadIndustryStatsResponse), "CrmLeadIndustryStats")]
    [TestCase(typeof(LeadStatsResponse), "CrmLeadStats")]
    [TestCase(typeof(RecordLeadCallInput), "CrmRecordLeadCallInput")]
    [TestCase(typeof(SaveLeadCallResultInput), "CrmSaveLeadCallResultInput")]
    [TestCase(typeof(SaveLeadCategoryInput), "CrmSaveLeadCategoryInput")]
    public void Serialized_properties_match_the_machine_contract(Type type, string schema)
    {
        var properties = (YamlMappingNode)((YamlMappingNode)Schemas()[schema])["properties"];
        Assert.That(properties.Children.Keys.Cast<YamlScalarNode>().Select(x => x.Value),
            Is.EquivalentTo(type.GetProperties().Select(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name))));
    }

    [TestCase("Leads", "getCrmLeads", "access:leads", null, null)]
    [TestCase("Leads", "getCrmLeadsMyDay", "access:leads", null, null)]
    [TestCase("Leads", "getCrmLeadCallResults", "access:leads", null, null)]
    [TestCase("Leads", "getCrmLead", "access:leads", null, null)]
    [TestCase("Leads", "takeCrmLead", "access:leads", null, "edit")]
    [TestCase("Leads", "releaseCrmLead", "access:leads", null, "edit")]
    [TestCase("Leads", "recordCrmLeadCall", "access:leads", null, "edit")]
    [TestCase("LeadStats", "getCrmLeadStats", "access:lead-stats", null, null)]
    [TestCase("LeadStats", "getCrmLeadStatsCallResults", "access:lead-stats", null, null)]
    [TestCase("LeadStats", "createCrmLeadCallResult", "access:lead-stats", null, "edit")]
    [TestCase("LeadStats", "editCrmLeadCallResult", "access:lead-stats", null, null)]
    [TestCase("LeadStats", "getCrmLeadCategories", "access:lead-stats", null, null)]
    [TestCase("LeadStats", "editCrmLeadCategory", "access:lead-stats", null, null)]
    [TestCase("LeadStats", "syncCrmLeads", "access:lead-stats", "access:lead-stats:create", null)]
    public void Operation_exists_with_its_policy(string tag, string operation, string groupPolicy,
        string? endpointPolicy, string? action)
    {
        var endpoint = MappedEndpoints.Named(app => app.MapCrmEndpoints(), operation);
        Assert.Multiple(() =>
        {
            Assert.That(endpoint.Tag, Is.EqualTo(tag));
            Assert.That(endpoint.Policies, Is.EqualTo(new[] { groupPolicy, endpointPolicy }.OfType<string>()));
            Assert.That(endpoint.Action, Is.EqualTo(action));
            Assert.That(Operations(), Does.Contain(operation));
        });
    }

    private static YamlMappingNode Root()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        using var reader = File.OpenText(Path.Combine(directory!.FullName, "docs/sdd/contracts/crm/leads.openapi.yaml"));
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
