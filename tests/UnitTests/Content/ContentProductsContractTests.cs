using System.Text.Json;
using Content.Api.ProductPages;
using Content.Application.Contracts;
using Content.Application.ProductPages.ImportOldSite;
using Content.Domain.ProductPages;
using Identity.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using YamlDotNet.RepresentationModel;

namespace UnitTests.Content;

/// <summary>«Контент → Товари» types and operations match docs/sdd/contracts/content/content-products.openapi.yaml (CNT-002).</summary>
public sealed class ContentProductsContractTests
{
    [TestCase(typeof(ContentProductRow), "ContentProductRow")]
    [TestCase(typeof(ContentProductPage), "ContentProductPage")]
    [TestCase(typeof(ContentProductTextResponse), "ContentProductTextResponse")]
    [TestCase(typeof(ContentProductVariantResponse), "ContentProductVariantResponse")]
    [TestCase(typeof(ContentPriceTiersResponse), "ContentPriceTiersResponse")]
    [TestCase(typeof(ContentProductResponse), "ContentProductResponse")]
    [TestCase(typeof(ContentProductVisibilityResponse), "ContentProductVisibilityResponse")]
    [TestCase(typeof(SaveContentProductRequest), "SaveContentProductRequest")]
    [TestCase(typeof(ContentProductVisibilityRequest), "ContentProductVisibilityRequest")]
    [TestCase(typeof(ImportOldSiteProductPagesRequest), "ImportOldSiteProductPagesRequest")]
    [TestCase(typeof(OldSiteProductPage), "OldSiteProductPage")]
    [TestCase(typeof(OldSiteImportNote), "OldSiteImportNote")]
    [TestCase(typeof(OldSiteImportResponse), "OldSiteImportResponse")]
    [TestCase(typeof(ProductPageTextInput), "ProductPageTextInput")]
    [TestCase(typeof(ProductFact), "ProductFact")]
    [TestCase(typeof(ProductTextBlock), "ProductTextBlock")]
    [TestCase(typeof(ProductSpecGroup), "ProductSpecGroup")]
    [TestCase(typeof(ProductSpecRow), "ProductSpecRow")]
    public void Serialized_properties_match_the_machine_contract(Type type, string schema)
    {
        var properties = (YamlMappingNode)((YamlMappingNode)Schemas()[schema])["properties"];
        Assert.That(properties.Children.Keys.Cast<YamlScalarNode>().Select(x => x.Value),
            Is.EquivalentTo(type.GetProperties().Select(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name))));
    }

    [Test]
    public void Block_kind_is_written_by_name_as_in_the_contract()
    {
        var values = (YamlSequenceNode)((YamlMappingNode)Schemas()["ProductTextBlockKind"])["enum"];
        Assert.Multiple(() =>
        {
            Assert.That(values.Children.Cast<YamlScalarNode>().Select(x => x.Value),
                Is.EqualTo(Enum.GetNames<ProductTextBlockKind>()));
            Assert.That(JsonSerializer.Serialize(ProductTextBlockKind.List), Is.EqualTo("\"List\""));
        });
    }

    [Test]
    public void Every_operation_is_in_the_contract_and_needs_the_content_products_policy()
    {
        var controller = typeof(ContentProductsController);
        var policy = controller.GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>().Single().Policy;
        var operations = controller.GetMethods()
            .SelectMany(x => x.GetCustomAttributes(typeof(HttpMethodAttribute), false).Cast<HttpMethodAttribute>())
            .Select(x => x.Name).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(policy, Is.EqualTo($"access:{AccessCatalog.ContentProducts}"));
            Assert.That(operations, Has.Length.EqualTo(6));
            Assert.That(Operations(), Is.EquivalentTo(operations));
        });
    }

    [Test]
    public void Page_is_in_access_rights_with_admin_full_and_manager_view()
    {
        var area = AccessCatalog.Find(AccessCatalog.ContentProducts)!;
        Assert.Multiple(() =>
        {
            Assert.That(area.Actions.HasFlag(AccessAction.Edit | AccessAction.Delete), Is.True);
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Admin)[AccessCatalog.ContentProducts], Is.EqualTo(area.Actions));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Manager)[AccessCatalog.ContentProducts], Is.EqualTo(AccessAction.View));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Accountant), Does.Not.ContainKey(AccessCatalog.ContentProducts));
        });
    }

    private static YamlMappingNode Root()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        using var reader = File.OpenText(Path.Combine(directory!.FullName, "docs/sdd/contracts/content/content-products.openapi.yaml"));
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
