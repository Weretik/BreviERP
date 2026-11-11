using System.Text.Json;
using Content.Api.PublicSitePages;
using Content.Api.SitePages;
using Content.Application.Contracts;
using Content.Domain.SitePages;
using Identity.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using YamlDotNet.RepresentationModel;

namespace UnitTests.Content;

/// <summary>
/// «Сторінки сайту» and the public page texts match docs/sdd/contracts/content/content-site-pages.openapi.yaml and
/// content-public-site-pages.openapi.yaml (CNT-007, CNT-010).
/// </summary>
public sealed class SitePagesContractTests
{
    private const string Admin = "content-site-pages.openapi.yaml";
    private const string Public = "content-public-site-pages.openapi.yaml";

    [TestCase(typeof(SitePageResponse), Admin, "SitePageResponse")]
    [TestCase(typeof(SitePageListResponse), Admin, "SitePageListResponse")]
    [TestCase(typeof(SitePageText), Admin, "SitePageText")]
    [TestCase(typeof(SaveSitePageRequest), Admin, "SaveSitePageRequest")]
    [TestCase(typeof(SiteFaqItem), Admin, "SiteFaqItem")]
    [TestCase(typeof(PublicSitePagesResponse), Public, "PublicSitePagesResponse")]
    [TestCase(typeof(PublicSitePage), Public, "PublicSitePage")]
    [TestCase(typeof(SitePageText), Public, "SitePageText")]
    public void Serialized_properties_match_the_machine_contract(Type type, string file, string schema)
    {
        var properties = (YamlMappingNode)((YamlMappingNode)Schemas(file)[schema])["properties"];
        Assert.That(properties.Children.Keys.Cast<YamlScalarNode>().Select(x => x.Value),
            Is.EquivalentTo(type.GetProperties().Select(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name))));
    }

    [Test]
    public void Every_operation_is_in_the_contract_and_needs_the_site_pages_policy()
    {
        var controller = typeof(SitePagesController);
        var policy = controller.GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>().Single().Policy;

        Assert.Multiple(() =>
        {
            Assert.That(policy, Is.EqualTo($"access:{AccessCatalog.ContentSitePages}"));
            Assert.That(Operations(Admin), Is.EquivalentTo(OperationsOf(controller)));
            Assert.That(Operations(Public), Is.EquivalentTo(OperationsOf(typeof(PublicSitePagesController))));
        });
    }

    [Test]
    public void Page_is_in_access_rights_with_admin_edit_and_manager_view()
    {
        var area = AccessCatalog.Find(AccessCatalog.ContentSitePages)!;
        Assert.Multiple(() =>
        {
            Assert.That(area.Title, Is.EqualTo("Сторінки сайту"));
            Assert.That(area.Actions, Is.EqualTo(AccessAction.View | AccessAction.Edit));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Admin)[AccessCatalog.ContentSitePages], Is.EqualTo(area.Actions));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Manager)[AccessCatalog.ContentSitePages], Is.EqualTo(AccessAction.View));
        });
    }

    private static string[] OperationsOf(Type controller) => controller.GetMethods()
        .SelectMany(x => x.GetCustomAttributes(typeof(HttpMethodAttribute), false).Cast<HttpMethodAttribute>())
        .Select(x => x.Name!).ToArray();

    private static YamlMappingNode Root(string file)
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        using var reader = File.OpenText(Path.Combine(directory!.FullName, "docs/sdd/contracts/content", file));
        var yaml = new YamlStream();
        yaml.Load(reader);
        return (YamlMappingNode)yaml.Documents[0].RootNode;
    }

    private static YamlMappingNode Schemas(string file) =>
        (YamlMappingNode)((YamlMappingNode)Root(file)["components"])["schemas"];

    private static IEnumerable<string> Operations(string file) =>
        ((YamlMappingNode)Root(file)["paths"]).Children.Values.Cast<YamlMappingNode>()
        .SelectMany(path => path.Children.Where(x => ((YamlScalarNode)x.Key).Value != "parameters"))
        .Select(x => ((YamlScalarNode)((YamlMappingNode)x.Value)["operationId"]).Value!);
}
