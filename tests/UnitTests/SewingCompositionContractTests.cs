using System.Text.Json;
using Catalog.Application.Features.Product.Sewing;
using YamlDotNet.RepresentationModel;

namespace UnitTests;

public sealed class SewingCompositionContractTests
{
    [TestCase(typeof(CreateSewingArticleRequest), "CreateSewingArticleRequest")]
    [TestCase(typeof(ReplaceSewingCompositionRequest), "ReplaceSewingCompositionRequest")]
    [TestCase(typeof(ReplaceSewingArticleCompositionRequest), "ReplaceSewingArticleCompositionRequest")]
    [TestCase(typeof(SewingMaterialInput), "SewingMaterialInput")]
    [TestCase(typeof(SewingOperationInput), "SewingOperationInput")]
    [TestCase(typeof(SewingArticleView), "SewingArticle")]
    [TestCase(typeof(SewingArticleList), "SewingArticleList")]
    [TestCase(typeof(SewingComposition), "SewingComposition")]
    [TestCase(typeof(SewingMaterialView), "SewingMaterial")]
    [TestCase(typeof(SewingOperationView), "SewingOperation")]
    public void Schema_properties_match_serialized_contract(Type type, string schemaName)
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln"))) directory = directory.Parent;
        Assert.That(directory, Is.Not.Null);
        using var reader = File.OpenText(Path.Combine(directory!.FullName, "docs/sdd/contracts/catalog/sewing-composition.openapi.yaml"));
        var yaml = new YamlStream(); yaml.Load(reader);
        var root = (YamlMappingNode)yaml.Documents[0].RootNode;
        var schemas = (YamlMappingNode)((YamlMappingNode)root["components"])["schemas"];
        var schema = (YamlMappingNode)schemas[schemaName];
        var properties = (YamlMappingNode)schema["properties"];
        var expected = type.GetProperties().Select(p => JsonNamingPolicy.CamelCase.ConvertName(p.Name)).ToArray();
        Assert.That(properties.Children.Keys.Cast<YamlScalarNode>().Select(p => p.Value), Is.EquivalentTo(expected));
    }
}
