using Microsoft.AspNetCore.Routing;
using UnitTests.TestSupport;
using Content.Api.DependencyInjection;
using System.Text.Json;
using Identity.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using YamlDotNet.RepresentationModel;
using Content.Application.Features.SiteGalleries.Commands.SaveSiteGallery;
using Content.Application.Features.SiteGalleries.Commands.TransferSiteGallery;
using Content.Application.Features.SiteGalleries.Shared;
using Content.Api.Endpoints.SiteGalleries.Shared;
using Identity.Domain.Authorization.Roles;

namespace UnitTests.Content;

/// <summary>
/// «Наші роботи», «Сертифікати» and their public pictures match docs/sdd/contracts/content/content-galleries.openapi.yaml
/// and content-public-galleries.openapi.yaml (CNT-011).
/// </summary>
public sealed class SiteGalleriesContractTests
{
    private static readonly Action<IEndpointRouteBuilder> Map = app => app.MapContentEndpoints();

    private const string Admin = "content-galleries.openapi.yaml";
    private const string Public = "content-public-galleries.openapi.yaml";

    [TestCase(typeof(SiteGalleryResponse), Admin, "SiteGalleryResponse")]
    [TestCase(typeof(SiteGalleryItemResponse), Admin, "SiteGalleryItemResponse")]
    [TestCase(typeof(SiteGalleryItemInput), Admin, "SiteGalleryItemInput")]
    [TestCase(typeof(SaveSiteGalleryRequest), Admin, "SaveSiteGalleryRequest")]
    [TestCase(typeof(SiteGalleryTransferResponse), Admin, "SiteGalleryTransferResponse")]
    [TestCase(typeof(GalleryText), Admin, "GalleryText")]
    [TestCase(typeof(PublicSiteGalleriesResponse), Public, "PublicSiteGalleriesResponse")]
    [TestCase(typeof(PublicGalleryItem), Public, "PublicGalleryItem")]
    [TestCase(typeof(GalleryText), Public, "GalleryText")]
    public void Serialized_properties_match_the_machine_contract(Type type, string file, string schema)
    {
        var properties = (YamlMappingNode)((YamlMappingNode)Schemas(file)[schema])["properties"];
        Assert.That(properties.Children.Keys.Cast<YamlScalarNode>().Select(x => x.Value),
            Is.EquivalentTo(type.GetProperties().Select(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name))));
    }

    [Test]
    public void Every_operation_is_in_the_contract_and_needs_its_page_policy()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Policy("SiteWorks"), Is.EqualTo($"access:{AccessCatalog.ContentWorks}"));
            Assert.That(Policy("SiteCertificates"), Is.EqualTo($"access:{AccessCatalog.ContentCertificates}"));
            Assert.That(Operations(Admin), Is.EquivalentTo(
                OperationsOf("SiteWorks").Concat(OperationsOf("SiteCertificates"))));
            Assert.That(Operations(Public), Is.EquivalentTo(OperationsOf("PublicSiteGalleries")));
        });
    }

    [TestCase(AccessCatalog.ContentWorks, "Наші роботи")]
    [TestCase(AccessCatalog.ContentCertificates, "Сертифікати")]
    public void Page_is_in_access_rights_with_admin_edit_and_manager_view(string page, string title)
    {
        var area = AccessCatalog.Find(page)!;
        Assert.Multiple(() =>
        {
            Assert.That(area.Title, Is.EqualTo(title));
            Assert.That(area.Actions, Is.EqualTo(AccessAction.View | AccessAction.Edit));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Admin)[page], Is.EqualTo(area.Actions));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Manager)[page], Is.EqualTo(AccessAction.View));
        });
    }

    private static string? Policy(string controller) =>
        MappedEndpoints.Policy(Map, controller);

    private static string[] OperationsOf(string controller) => MappedEndpoints.Of(Map, controller)
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
