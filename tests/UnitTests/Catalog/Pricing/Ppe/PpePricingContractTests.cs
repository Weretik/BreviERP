using UnitTests.TestSupport;
using Catalog.Api.DependencyInjection;
using Catalog.Domain.Pricing.Ppe;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Text.Json;
using Catalog.Domain.Pricing.Sewing;
using YamlDotNet.RepresentationModel;
using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Features.Pricing.Shared;
using Catalog.Api.Pricing.Ppe;

namespace UnitTests.Catalog.Pricing.Ppe;

public sealed class PpePricingContractTests
{
    [TestCase(typeof(PpeMarkups))]
    [TestCase(typeof(PpeMarkupsRequest))]
    [TestCase(typeof(PpeRuleValuesRequest))]
    [TestCase(typeof(PpeOfferValuesRequest))]
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
    [TestCase(typeof(global::Catalog.Application.Features.Pricing.Shared.Ppe.SupplierOffer.PpeSupplierOffer))]
    [TestCase(typeof(UpdatePpeSupplierOfferRequest))]
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
        var schemaName = type.Name is "PpeMarkupsRequest" or "PpeRuleValuesRequest" or "PpeOfferValuesRequest" ? type.Name[..^7] : type.Name;
        var properties = (YamlMappingNode)((YamlMappingNode)schemas[schemaName])["properties"];
        Assert.That(properties.Children.Keys.Cast<YamlScalarNode>().Select(x => x.Value),
            Is.EquivalentTo(type.GetProperties().Select(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name))));
    }

    [TestCase("getPpeProductPrices", "CatalogRead")]
    [TestCase("getPpePricingRule", "CatalogRead")]
    [TestCase("createPpeRuleVersion", "access:pricing-parameters")]
    [TestCase("createPpeOfferVersion", "access:products")]
    [TestCase("getPpeSupplierOffer", "CatalogRead")]
    [TestCase("updatePpeSupplierOffer", "access:products")]
    [TestCase("previewPpePrices", "CatalogRead")]
    public void Pricing_operation_exists_and_requires_the_expected_policy(string operation, string policy)
    {
        var endpoint = MappedEndpoints.Named(app => app.MapCatalogEndpoints(), operation);
        Assert.That(endpoint.Policies, Does.Contain(policy));
        Assert.That(endpoint.Anonymous, Is.False);
    }
}
