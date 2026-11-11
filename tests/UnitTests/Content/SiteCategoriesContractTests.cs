using System.Text.Json;
using Content.Api.SiteCategories;
using Content.Application.Contracts;
using Content.Application.SiteCategories.ImportOldSite;
using Content.Domain.SiteCategories;
using Identity.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using YamlDotNet.RepresentationModel;

namespace UnitTests.Content;

/// <summary>«Контент → Розділи» types and operations match docs/sdd/contracts/content/content-categories.openapi.yaml (CNT-003).</summary>
public sealed class SiteCategoriesContractTests
{
    [TestCase(typeof(SiteCategoryRow), "SiteCategoryRow")]
    [TestCase(typeof(SiteCategoryList), "SiteCategoryList")]
    [TestCase(typeof(SiteCategoryTextResponse), "SiteCategoryTextResponse")]
    [TestCase(typeof(SiteCategoryResponse), "SiteCategoryResponse")]
    [TestCase(typeof(SiteCategoryVisibilityResponse), "SiteCategoryVisibilityResponse")]
    [TestCase(typeof(SaveSiteCategoryRequest), "SaveSiteCategoryRequest")]
    [TestCase(typeof(SiteCategoryVisibilityRequest), "SiteCategoryVisibilityRequest")]
    [TestCase(typeof(ReorderSiteCategoriesRequest), "ReorderSiteCategoriesRequest")]
    [TestCase(typeof(SiteCategoryTextInput), "SiteCategoryTextInput")]
    [TestCase(typeof(OldSiteCategory), "OldSiteCategory")]
    [TestCase(typeof(ImportOldSiteCategoriesRequest), "ImportOldSiteCategoriesRequest")]
    [TestCase(typeof(OldSiteCategoryImportResponse), "OldSiteCategoryImportResponse")]
    public void Serialized_properties_match_the_machine_contract(Type type, string schema)
    {
        var properties = (YamlMappingNode)((YamlMappingNode)Schemas()[schema])["properties"];
        Assert.That(properties.Children.Keys.Cast<YamlScalarNode>().Select(x => x.Value),
            Is.EquivalentTo(type.GetProperties().Select(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name))));
    }

    [Test]
    public void Every_operation_is_in_the_contract_and_needs_the_content_categories_policy()
    {
        var controller = typeof(SiteCategoriesController);
        var policy = controller.GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>().Single().Policy;
        var operations = controller.GetMethods()
            .SelectMany(x => x.GetCustomAttributes(typeof(HttpMethodAttribute), false).Cast<HttpMethodAttribute>())
            .Select(x => x.Name).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(policy, Is.EqualTo($"access:{AccessCatalog.ContentCategories}"));
            Assert.That(operations, Has.Length.EqualTo(8));
            Assert.That(Operations(), Is.EquivalentTo(operations));
        });
    }

    [Test]
    public void Page_is_in_access_rights_with_admin_full_and_manager_view()
    {
        var area = AccessCatalog.Find(AccessCatalog.ContentCategories)!;
        Assert.Multiple(() =>
        {
            Assert.That(area.Title, Is.EqualTo("Розділи сайту"));
            Assert.That(area.Actions.HasFlag(AccessAction.Create | AccessAction.Edit | AccessAction.Delete), Is.True);
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Admin)[AccessCatalog.ContentCategories], Is.EqualTo(area.Actions));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Manager)[AccessCatalog.ContentCategories], Is.EqualTo(AccessAction.View));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Accountant), Does.Not.ContainKey(AccessCatalog.ContentCategories));
        });
    }

    private static YamlMappingNode Root()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        using var reader = File.OpenText(Path.Combine(directory!.FullName, "docs/sdd/contracts/content/content-categories.openapi.yaml"));
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
