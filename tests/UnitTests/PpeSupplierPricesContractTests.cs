using System.Text.Json;
using Catalog.Api.Contracts.Pricing.Ppe.SupplierPrices;
using Catalog.Application.Contracts.Pricing.Ppe.SupplierPrices;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Import;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.List;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using YamlDotNet.RepresentationModel;

namespace UnitTests;

/// <summary>«Імпорт цін» types and operations match docs/sdd/contracts/catalog/ppe-supplier-prices.openapi.yaml (CAT-017).</summary>
public sealed class PpeSupplierPricesContractTests
{
    [TestCase(typeof(PpePriceSourceView))]
    [TestCase(typeof(PpePriceImportRun))]
    [TestCase(typeof(PpePriceImportProblem))]
    [TestCase(typeof(UpdatePpePriceSourceRequest))]
    [TestCase(typeof(PpePriceSourcesResponse))]
    [TestCase(typeof(PpePriceImportResult))]
    [TestCase(typeof(PpePriceImportRow))]
    public void Serialized_properties_match_the_machine_contract(Type type)
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        using var reader = File.OpenText(Path.Combine(directory!.FullName, "docs/sdd/contracts/catalog/ppe-supplier-prices.openapi.yaml"));
        var yaml = new YamlStream();
        yaml.Load(reader);
        var schemas = (YamlMappingNode)((YamlMappingNode)((YamlMappingNode)yaml.Documents[0].RootNode)["components"])["schemas"];
        var properties = (YamlMappingNode)((YamlMappingNode)schemas[type.Name])["properties"];
        Assert.That(properties.Children.Keys.Cast<YamlScalarNode>().Select(x => x.Value),
            Is.EquivalentTo(type.GetProperties().Select(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name))));
    }

    [TestCase("listPpePriceSources", "CatalogRead")]
    [TestCase("updatePpePriceSource", "RequireAdminRole")]
    [TestCase("importPpeSupplierPrices", "CanManageProducts")]
    [TestCase("refreshPpeSupplierPrices", "CanManageProducts")]
    public void Operation_exists_and_requires_the_expected_policy(string operation, string policy)
    {
        var methods = typeof(Catalog.Api.Controllers.SewingArticlesController).Assembly.GetTypes()
            .SelectMany(type => type.GetMethods())
            .Where(method => method.GetCustomAttributes(typeof(HttpMethodAttribute), false)
                .Cast<HttpMethodAttribute>().Any(attribute => attribute.Name == operation)).ToArray();
        Assert.That(methods, Has.Length.EqualTo(1));
        Assert.That(methods[0].GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>().Select(attribute => attribute.Policy), Does.Contain(policy));
    }
}
