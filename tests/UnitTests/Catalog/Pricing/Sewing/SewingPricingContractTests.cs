using UnitTests.TestSupport;
using Catalog.Api.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Text.Json;
using Catalog.Domain.Pricing.Sewing;
using YamlDotNet.RepresentationModel;
using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Features.Pricing.Queries.GetSewingArticleSummary;
using Catalog.Api.Endpoints.Sewing.CreateSewingSettingsVersion;
using Catalog.Api.Endpoints.Sewing.SaveSewingCalculation;

namespace UnitTests.Catalog.Pricing.Sewing;

public sealed class SewingPricingContractTests
{
    [TestCase(typeof(SavedSewingCalculation))]
    [TestCase(typeof(PublishedSewingPrice))]
    [TestCase(typeof(SewingPublishedArticlePrice))]
    [TestCase(typeof(SewingPublishedProductPrices))]
    [TestCase(typeof(SewingArticlePricePreview))]
    [TestCase(typeof(SewingArticleCalculation))]
    [TestCase(typeof(SewingCostBreakdown))]
    [TestCase(typeof(SewingPriceTiers))]
    [TestCase(typeof(SewingPricingBlocker))]
    [TestCase(typeof(SewingPricingSettingsSelection))]
    [TestCase(typeof(SewingCalculationSettings))]
    [TestCase(typeof(SewingArticlePricingSnapshot))]
    [TestCase(typeof(SewingPricingMaterialRow))]
    [TestCase(typeof(SewingPricingOperationRow))]
    [TestCase(typeof(PricingReferenceValue))]
    [TestCase(typeof(SaveSewingCalculationRequest))]
    [TestCase(typeof(CreateSewingSettingsVersionRequest))]
    [TestCase(typeof(SewingSettingsValuesRequest))]
    [TestCase(typeof(SewingArticleSummary))]
    [TestCase(typeof(SewingArticleSummaryRow))]
    [TestCase(typeof(SewingArticlePublishedSummary))]
    [TestCase(typeof(SewingCardMaterial))]
    [TestCase(typeof(SewingCardOperation))]
    public void Serialized_properties_match_the_machine_contract(Type type)
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        Assert.That(directory, Is.Not.Null);
        using var reader = File.OpenText(Path.Combine(directory!.FullName, "docs/sdd/contracts/catalog/sewing-pricing.openapi.yaml"));
        var yaml = new YamlStream();
        yaml.Load(reader);
        var root = (YamlMappingNode)yaml.Documents[0].RootNode;
        var schemas = (YamlMappingNode)((YamlMappingNode)root["components"])["schemas"];
        var schemaName = type == typeof(SewingSettingsValuesRequest) ? nameof(SewingCalculationSettings) : type.Name;
        var properties = (YamlMappingNode)((YamlMappingNode)schemas[schemaName])["properties"];
        Assert.That(properties.Children.Keys.Cast<YamlScalarNode>().Select(x => x.Value),
            Is.EquivalentTo(type.GetProperties().Select(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name))));
    }

    [TestCase("saveSewingCalculation", "access:products")]
    [TestCase("getSewingCalculation", "CatalogRead")]
    [TestCase("publishSewingCalculation", "access:products")]
    [TestCase("getPublishedSewingPrice", "CatalogRead")]
    [TestCase("createSewingSettingsVersion", "access:pricing-parameters")]
    [TestCase("getSewingSettings", "CatalogRead")]
    [TestCase("getPublishedSewingProducts", "CatalogRead")]
    [TestCase("getSewingArticleSummary", "CatalogRead")]
    public void Pricing_operation_exists_and_requires_the_expected_policy(string operation, string policy)
    {
        var endpoint = MappedEndpoints.Named(app => app.MapCatalogEndpoints(), operation);
        Assert.That(endpoint.Policies, Does.Contain(policy));
        Assert.That(endpoint.Anonymous, Is.False);
    }
}
