using System.Text.Json;
using Content.Api.SeoOverview;
using Content.Application.Contracts;
using Identity.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using YamlDotNet.RepresentationModel;

namespace UnitTests.Content;

/// <summary>«SEO-огляд» matches docs/sdd/contracts/content/content-seo-overview.openapi.yaml (CNT-008).</summary>
public sealed class SeoOverviewContractTests
{
    [TestCase(typeof(SeoOverviewResponse), "SeoOverviewResponse")]
    [TestCase(typeof(SeoAreaSummary), "SeoAreaSummary")]
    [TestCase(typeof(SeoIssue), "SeoIssue")]
    [TestCase(typeof(SeoProblem), "SeoProblem")]
    public void Serialized_properties_match_the_machine_contract(Type type, string schema)
    {
        var properties = (YamlMappingNode)((YamlMappingNode)Schemas()[schema])["properties"];
        Assert.That(properties.Children.Keys.Cast<YamlScalarNode>().Select(x => x.Value),
            Is.EquivalentTo(type.GetProperties().Select(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name))));
    }

    [TestCase(typeof(SeoArea), "SeoArea")]
    [TestCase(typeof(SeoProblemKind), "SeoProblemKind")]
    [TestCase(typeof(SeoSiteProblem), "SeoSiteProblem")]
    public void Enum_values_match_the_machine_contract(Type type, string schema)
    {
        var values = (YamlSequenceNode)((YamlMappingNode)Schemas()[schema])["enum"];
        Assert.That(values.Children.Cast<YamlScalarNode>().Select(x => x.Value), Is.EquivalentTo(Enum.GetNames(type)));
    }

    [Test]
    public void Operation_is_in_the_contract_and_needs_the_seo_overview_policy()
    {
        var controller = typeof(SeoOverviewController);
        var policy = controller.GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>().Single().Policy;
        var operations = controller.GetMethods()
            .SelectMany(x => x.GetCustomAttributes(typeof(HttpMethodAttribute), false).Cast<HttpMethodAttribute>())
            .Select(x => x.Name!);
        var documented = ((YamlMappingNode)Root()["paths"]).Children.Values.Cast<YamlMappingNode>()
            .SelectMany(path => path.Children.Values.Cast<YamlMappingNode>())
            .Select(x => ((YamlScalarNode)x["operationId"]).Value!);

        Assert.Multiple(() =>
        {
            Assert.That(policy, Is.EqualTo($"access:{AccessCatalog.ContentSeoOverview}"));
            Assert.That(documented, Is.EquivalentTo(operations));
        });
    }

    [Test]
    public void Page_is_in_access_rights_view_only_for_admin_and_manager()
    {
        var area = AccessCatalog.Find(AccessCatalog.ContentSeoOverview)!;
        Assert.Multiple(() =>
        {
            Assert.That(area.Title, Is.EqualTo("SEO-огляд"));
            Assert.That(area.Actions, Is.EqualTo(AccessAction.View));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Admin)[AccessCatalog.ContentSeoOverview], Is.EqualTo(AccessAction.View));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Manager)[AccessCatalog.ContentSeoOverview], Is.EqualTo(AccessAction.View));
        });
    }

    private static YamlMappingNode Root()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        using var reader = File.OpenText(Path.Combine(directory!.FullName, "docs/sdd/contracts/content/content-seo-overview.openapi.yaml"));
        var yaml = new YamlStream();
        yaml.Load(reader);
        return (YamlMappingNode)yaml.Documents[0].RootNode;
    }

    private static YamlMappingNode Schemas() => (YamlMappingNode)((YamlMappingNode)Root()["components"])["schemas"];
}
