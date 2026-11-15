using UnitTests.TestSupport;
using Catalog.Api.DependencyInjection;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Features.Pricing.Commands.ImportPpeSupplierFiles;
using Catalog.Application.Features.Pricing.Queries.GetPpePriceSources;
using Catalog.Api.Endpoints.Ppe.ListPpePriceSources;
using Catalog.Api.Endpoints.Ppe.UpdatePpePriceSource;
using Catalog.Application.Abstractions.Pricing.PpeSupplierPrices;

namespace UnitTests.Catalog.Pricing.Ppe;

/// <summary>«Імпорт цін» types and operations match the generated contract docs/sdd/contracts/openapi.json (CAT-017).</summary>
public sealed class PpeSupplierPricesContractTests
{
    [TestCase("listPpePriceSources", "CatalogRead")]
    [TestCase("updatePpePriceSource", "RequireAdminRole")]
    [TestCase("importPpeSupplierPrices", "access:price-import")]
    [TestCase("refreshPpeSupplierPrices", "access:price-import")]
    public void Operation_exists_and_requires_the_expected_policy(string operation, string policy)
    {
        var endpoint = MappedEndpoints.Named(app => app.MapCatalogEndpoints(), operation);
        Assert.That(endpoint.Policies, Does.Contain(policy));
        Assert.That(endpoint.Anonymous, Is.False);
    }
}
