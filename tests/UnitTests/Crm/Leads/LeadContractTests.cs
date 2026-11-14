using System.Text.Json;
using BuildingBlocks.Api.Authorization;
using Crm.Api.Leads;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
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

    [TestCase(typeof(LeadsController), "getCrmLeads", "access:leads", null, null)]
    [TestCase(typeof(LeadsController), "getCrmLeadsMyDay", "access:leads", null, null)]
    [TestCase(typeof(LeadsController), "getCrmLeadCallResults", "access:leads", null, null)]
    [TestCase(typeof(LeadsController), "getCrmLead", "access:leads", null, null)]
    [TestCase(typeof(LeadsController), "takeCrmLead", "access:leads", null, "edit")]
    [TestCase(typeof(LeadsController), "releaseCrmLead", "access:leads", null, "edit")]
    [TestCase(typeof(LeadsController), "recordCrmLeadCall", "access:leads", null, "edit")]
    [TestCase(typeof(LeadStatsController), "getCrmLeadStats", "access:lead-stats", null, null)]
    [TestCase(typeof(LeadStatsController), "getCrmLeadStatsCallResults", "access:lead-stats", null, null)]
    [TestCase(typeof(LeadStatsController), "createCrmLeadCallResult", "access:lead-stats", null, "edit")]
    [TestCase(typeof(LeadStatsController), "editCrmLeadCallResult", "access:lead-stats", null, null)]
    [TestCase(typeof(LeadStatsController), "getCrmLeadCategories", "access:lead-stats", null, null)]
    [TestCase(typeof(LeadStatsController), "editCrmLeadCategory", "access:lead-stats", null, null)]
    [TestCase(typeof(LeadStatsController), "syncCrmLeads", "access:lead-stats", "access:lead-stats:create", null)]
    public void Operation_exists_with_its_policy(Type controller, string operation, string classPolicy,
        string? methodPolicy, string? action)
    {
        var method = controller.GetMethods()
            .Single(x => x.GetCustomAttributes(typeof(HttpMethodAttribute), false).Cast<HttpMethodAttribute>()
                .Any(attribute => attribute.Name == operation));
        Assert.Multiple(() =>
        {
            Assert.That(controller.GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>()
                .Single().Policy, Is.EqualTo(classPolicy));
            Assert.That(method.GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>()
                .SingleOrDefault()?.Policy, Is.EqualTo(methodPolicy));
            Assert.That(method.GetCustomAttributes(typeof(AccessActionAttribute), false).Cast<AccessActionAttribute>()
                .SingleOrDefault()?.Action, Is.EqualTo(action));
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
