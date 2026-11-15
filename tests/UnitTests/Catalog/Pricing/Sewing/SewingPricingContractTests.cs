using UnitTests.TestSupport;
using Catalog.Api.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Text.Json;
using Catalog.Domain.Pricing.Sewing;
using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Features.Pricing.Queries.GetSewingArticleSummary;
using Catalog.Api.Endpoints.Sewing.CreateSewingSettingsVersion;
using Catalog.Api.Endpoints.Sewing.SaveSewingCalculation;
using Catalog.Application.Abstractions.Pricing.Calculations;
using Catalog.Application.Abstractions.Pricing.Inputs;
using Catalog.Application.Abstractions.Pricing.Publications;
using Catalog.Application.Abstractions.Pricing.Settings;

namespace UnitTests.Catalog.Pricing.Sewing;

public sealed class SewingPricingContractTests
{
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
