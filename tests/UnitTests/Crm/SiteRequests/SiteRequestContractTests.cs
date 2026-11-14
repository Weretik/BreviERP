using System.Text.Json;
using BuildingBlocks.Api.Authorization;
using Crm.Api.SiteRequests;
using Identity.Domain.Authorization;
using Crm.Api.DependencyInjection;
using UnitTests.TestSupport;
using YamlDotNet.RepresentationModel;
using Crm.Application.Features.SiteRequests.Shared;

namespace UnitTests.Crm.SiteRequests;

/// <summary>Site request types, operations and policies match docs/sdd/contracts/crm/site-requests.openapi.yaml (LEAD-002).</summary>
public sealed class SiteRequestContractTests
{
    [TestCase(typeof(SiteRequestResponse), "CrmSiteRequest")]
    [TestCase(typeof(SiteRequestPageResponse), "CrmSiteRequestPage")]
    [TestCase(typeof(SiteRequestCountsResponse), "CrmSiteRequestCounts")]
    [TestCase(typeof(ChangeSiteRequestInput), "CrmChangeSiteRequestInput")]
    [TestCase(typeof(SiteRequestReceivedResponse), "CrmSiteRequestReceived")]
    [TestCase(typeof(PublicSiteRequestForm), "CrmPublicSiteRequestForm")]
    public void Serialized_properties_match_the_machine_contract(Type type, string schema)
    {
        var properties = (YamlMappingNode)((YamlMappingNode)Schemas()[schema])["properties"];
        Assert.That(properties.Children.Keys.Cast<YamlScalarNode>().Select(x => x.Value),
            Is.EquivalentTo(type.GetProperties().Select(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name))));
    }

    [TestCase("getCrmSiteRequests", null)]
    [TestCase("changeCrmSiteRequest", "edit")]
    public void Staff_operations_need_the_page(string operation, string? action)
    {
        var endpoint = MappedEndpoints.Named(app => app.MapCrmEndpoints(), operation);
        Assert.Multiple(() =>
        {
            Assert.That(endpoint.Policies, Is.EqualTo(new[] { $"access:{AccessCatalog.SiteRequests}" }));
            Assert.That(endpoint.Action, Is.EqualTo(action));
            Assert.That(Operations(), Does.Contain(operation));
        });
    }

    [Test]
    public void The_site_posts_anonymously_and_the_page_is_in_access_rights()
    {
        var endpoint = MappedEndpoints.Named(app => app.MapCrmEndpoints(), "submitPublicSiteRequest");
        Assert.Multiple(() =>
        {
            Assert.That(endpoint.Anonymous, Is.True);
            Assert.That(Operations(), Does.Contain("submitPublicSiteRequest"));
            Assert.That(AccessCatalog.Find(AccessCatalog.SiteRequests)!.Title, Is.EqualTo("Заявки з сайту"));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Manager)[AccessCatalog.SiteRequests],
                Is.EqualTo(AccessAction.View | AccessAction.Edit));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Seo), Does.Not.ContainKey(AccessCatalog.SiteRequests));
        });
    }

    private static YamlMappingNode Root()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        using var reader = File.OpenText(Path.Combine(directory!.FullName, "docs/sdd/contracts/crm/site-requests.openapi.yaml"));
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
