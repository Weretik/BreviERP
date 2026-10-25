using Catalog.Api.Contracts.Pricing.Ppe;
using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Domain.Pricing.Ppe;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Text.Json;
using Catalog.Api.Contracts.Pricing.Sewing.Calculations;
using Catalog.Api.Contracts.Pricing.Sewing.Settings;
using Catalog.Application.Contracts.Pricing;
using Catalog.Application.Features.Pricing.Sewing.Preview;
using Catalog.Domain.Pricing.Sewing;
using YamlDotNet.RepresentationModel;

namespace UnitTests;

public sealed class PpePricingContractTests
{
    [TestCase(typeof(PpeMarkups))]
    [TestCase(typeof(PpePriceTiers))]
    [TestCase(typeof(PpePricingBlocker))]
    [TestCase(typeof(PpePriceCalculation))]
    [TestCase(typeof(PpeRuleValues))]
    [TestCase(typeof(PpeOfferValues))]
    [TestCase(typeof(PpeRuleVersion))]
    [TestCase(typeof(CreatePpeRuleVersionRequest))]
    [TestCase(typeof(PpeOfferVersion))]
    [TestCase(typeof(CreatePpeOfferVersionRequest))]
    [TestCase(typeof(PpeProductPricing))]
    public void Serialized_properties_match_the_machine_contract(Type type)
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        Assert.That(directory, Is.Not.Null);
        using var reader = File.OpenText(Path.Combine(directory!.FullName, "docs/sdd/contracts/catalog/ppe-pricing.openapi.yaml"));
        var yaml = new YamlStream();
        yaml.Load(reader);
        var root = (YamlMappingNode)yaml.Documents[0].RootNode;
        var schemas = (YamlMappingNode)((YamlMappingNode)root["components"])["schemas"];
        var schemaName = type.Name;
        var properties = (YamlMappingNode)((YamlMappingNode)schemas[schemaName])["properties"];
        Assert.That(properties.Children.Keys.Cast<YamlScalarNode>().Select(x => x.Value),
            Is.EquivalentTo(type.GetProperties().Select(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name))));
    }

    [TestCase("getPpeProductPrices", "CatalogRead")]
    [TestCase("getPpePricingRule", "CatalogRead")]
    [TestCase("createPpeRuleVersion", "CanManageProducts")]
    [TestCase("createPpeOfferVersion", "CanManageProducts")]
    public void Pricing_operation_exists_and_requires_the_expected_policy(string operation, string policy)
    {
        var methods = typeof(Catalog.Api.Controllers.SewingArticlesController).Assembly.GetTypes()
            .SelectMany(type => type.GetMethods())
            .Where(method => method.GetCustomAttributes(typeof(HttpMethodAttribute), false)
                .Cast<HttpMethodAttribute>().Any(attribute => attribute.Name == operation)).ToArray();
        Assert.That(methods, Has.Length.EqualTo(1), $"Missing pricing operation {operation}.");
        Assert.That(methods[0].GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>().Select(attribute => attribute.Policy), Does.Contain(policy));
        Assert.That(methods[0].IsDefined(typeof(AllowAnonymousAttribute), true), Is.False);
    }
}
