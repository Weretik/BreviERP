using Microsoft.AspNetCore.Routing;
using UnitTests.TestSupport;
using Content.Api.DependencyInjection;
using System.Text.Json;
using Content.Domain.Articles;
using Identity.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using YamlDotNet.RepresentationModel;
using Content.Application.Features.Articles.Commands.ImportOldSiteArticles;
using Content.Application.Features.Articles.Commands.SetContentArticleVisibility;
using Content.Application.Features.Articles.Queries.GetContentArticles;
using Content.Application.Features.Articles.Shared;
using Content.Api.Endpoints.Articles.ImportOldSiteArticles;
using Content.Api.Endpoints.Articles.SetContentArticleVisibility;
using Content.Api.Endpoints.Articles.Shared;
using Identity.Domain.Authorization.Roles;
using Content.Application.Features.Articles.Commands.ImportOldSiteArticles.Results;
using Content.Application.Features.Articles.Shared.Admin;

namespace UnitTests.Content.Articles;

/// <summary>«Контент → Статті» types and operations match docs/sdd/contracts/content/content-articles.openapi.yaml (CNT-004).</summary>
public sealed class ArticlesContractTests
{
    private static readonly Action<IEndpointRouteBuilder> Map = app => app.MapContentEndpoints();

    [TestCase(typeof(ArticleRow), "ArticleRow")]
    [TestCase(typeof(ArticleList), "ArticleList")]
    [TestCase(typeof(ArticleTextResponse), "ArticleTextResponse")]
    [TestCase(typeof(ArticlePhoto), "ArticlePhoto")]
    [TestCase(typeof(ArticleResponse), "ArticleResponse")]
    [TestCase(typeof(ArticleVisibilityResponse), "ArticleVisibilityResponse")]
    [TestCase(typeof(SaveContentArticleRequest), "SaveContentArticleRequest")]
    [TestCase(typeof(ContentArticleVisibilityRequest), "ContentArticleVisibilityRequest")]
    [TestCase(typeof(ArticleTextInput), "ArticleTextInput")]
    [TestCase(typeof(ArticleBlock), "ArticleBlock")]
    [TestCase(typeof(OldSiteArticleBlock), "OldSiteArticleBlock")]
    [TestCase(typeof(OldSiteArticleText), "OldSiteArticleText")]
    [TestCase(typeof(OldSiteArticle), "OldSiteArticle")]
    [TestCase(typeof(ImportOldSiteArticlesRequest), "ImportOldSiteArticlesRequest")]
    [TestCase(typeof(OldSiteArticleImportResponse), "OldSiteArticleImportResponse")]
    public void Serialized_properties_match_the_machine_contract(Type type, string schema)
    {
        var properties = (YamlMappingNode)((YamlMappingNode)Schemas()[schema])["properties"];
        Assert.That(properties.Children.Keys.Cast<YamlScalarNode>().Select(x => x.Value),
            Is.EquivalentTo(type.GetProperties().Select(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name))));
    }

    [Test]
    public void Every_operation_is_in_the_contract_and_needs_the_content_articles_policy()
    {
        var controller = "ContentArticles";
        var policy = MappedEndpoints.Policy(Map, controller);
        var operations = MappedEndpoints.Of(Map, controller)
            .Select(x => x.Name).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(policy, Is.EqualTo($"access:{AccessCatalog.ContentArticles}"));
            Assert.That(operations, Has.Length.EqualTo(7));
            Assert.That(Operations(), Is.EquivalentTo(operations));
        });
    }

    [Test]
    public void Page_is_in_access_rights_with_admin_full_and_manager_view()
    {
        var area = AccessCatalog.Find(AccessCatalog.ContentArticles)!;
        Assert.Multiple(() =>
        {
            Assert.That(area.Title, Is.EqualTo("Статті"));
            Assert.That(area.Actions.HasFlag(AccessAction.Create | AccessAction.Edit | AccessAction.Delete), Is.True);
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Admin)[AccessCatalog.ContentArticles], Is.EqualTo(area.Actions));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Manager)[AccessCatalog.ContentArticles], Is.EqualTo(AccessAction.View));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Accountant), Does.Not.ContainKey(AccessCatalog.ContentArticles));
        });
    }

    private static YamlMappingNode Root()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        using var reader = File.OpenText(Path.Combine(directory!.FullName, "docs/sdd/contracts/content/content-articles.openapi.yaml"));
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
