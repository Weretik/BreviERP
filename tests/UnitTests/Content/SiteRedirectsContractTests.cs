using System.Text.Json;
using Content.Api.PublicRedirects;
using Content.Api.Redirects;
using Content.Application.Contracts;
using Content.Application.Redirects.ImportOldSite;
using Content.Domain.Redirects;
using Identity.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using YamlDotNet.RepresentationModel;

namespace UnitTests.Content;

/// <summary>
/// «Контент → Переадресації» and the public redirects match docs/sdd/contracts/content/content-redirects.openapi.yaml
/// and content-public-redirects.openapi.yaml (CNT-005).
/// </summary>
public sealed class SiteRedirectsContractTests
{
    private const string Admin = "content-redirects.openapi.yaml";
    private const string Public = "content-public-redirects.openapi.yaml";

    [TestCase(typeof(SiteRedirectResponse), Admin, "SiteRedirectResponse")]
    [TestCase(typeof(SiteRedirectList), Admin, "SiteRedirectList")]
    [TestCase(typeof(SaveSiteRedirectRequest), Admin, "SaveSiteRedirectRequest")]
    [TestCase(typeof(OldSiteRedirectItem), Admin, "OldSiteRedirectItem")]
    [TestCase(typeof(ImportOldSiteRedirectsRequest), Admin, "ImportOldSiteRedirectsRequest")]
    [TestCase(typeof(OldSiteRedirectRow), Admin, "OldSiteRedirectRow")]
    [TestCase(typeof(OldSiteRedirectImportResponse), Admin, "OldSiteRedirectImportResponse")]
    [TestCase(typeof(PublicRedirect), Public, "PublicRedirect")]
    [TestCase(typeof(PublicRedirectList), Public, "PublicRedirectList")]
    public void Serialized_properties_match_the_machine_contract(Type type, string file, string schema)
    {
        var properties = (YamlMappingNode)((YamlMappingNode)Schemas(file)[schema])["properties"];
        Assert.That(properties.Children.Keys.Cast<YamlScalarNode>().Select(x => x.Value),
            Is.EquivalentTo(type.GetProperties().Select(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name))));
    }

    [Test]
    public void Source_is_written_by_name_as_in_the_contract()
    {
        var values = ((YamlSequenceNode)((YamlMappingNode)Schemas(Admin)["SiteRedirectSource"])["enum"])
            .Cast<YamlScalarNode>().Select(x => x.Value);
        Assert.Multiple(() =>
        {
            Assert.That(values, Is.EquivalentTo(Enum.GetNames<SiteRedirectSource>()));
            Assert.That(JsonSerializer.Serialize(SiteRedirectSource.SlugChange), Is.EqualTo("\"SlugChange\""));
        });
    }

    [Test]
    public void Every_operation_is_in_the_contract_and_needs_the_content_redirects_policy()
    {
        var controller = typeof(ContentRedirectsController);
        var policy = controller.GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>().Single().Policy;

        Assert.Multiple(() =>
        {
            Assert.That(policy, Is.EqualTo($"access:{AccessCatalog.ContentRedirects}"));
            Assert.That(Operations(Admin), Is.EquivalentTo(OperationsOf(controller)));
            Assert.That(Operations(Public), Is.EquivalentTo(OperationsOf(typeof(PublicRedirectsController))));
        });
    }

    [Test]
    public void Page_is_in_access_rights_with_admin_full_and_manager_view()
    {
        var area = AccessCatalog.Find(AccessCatalog.ContentRedirects)!;
        Assert.Multiple(() =>
        {
            Assert.That(area.Title, Is.EqualTo("Переадресації"));
            Assert.That(area.Actions.HasFlag(AccessAction.Create | AccessAction.Edit | AccessAction.Delete), Is.True);
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Admin)[AccessCatalog.ContentRedirects], Is.EqualTo(area.Actions));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Manager)[AccessCatalog.ContentRedirects], Is.EqualTo(AccessAction.View));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Accountant), Does.Not.ContainKey(AccessCatalog.ContentRedirects));
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
