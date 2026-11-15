using UnitTests.TestSupport;
using Catalog.Api.DependencyInjection;
using Catalog.Domain.Pricing.Ppe;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Text.Json;
using Catalog.Domain.Pricing.Sewing;
using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Features.Pricing.Shared;
using Catalog.Api.Endpoints.Ppe.CreatePpeOfferVersion;
using Catalog.Api.Endpoints.Ppe.CreatePpeRuleVersion;
using Catalog.Api.Endpoints.Ppe.UpdatePpeSupplierOffer;
using Catalog.Application.Abstractions.Pricing.Ppe.Versions;

namespace UnitTests.Catalog.Pricing.Ppe;

public sealed class PpePricingContractTests
{
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
