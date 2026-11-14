using System.Text.Json;
using Ardalis.Result;
using Ardalis.Specification;
using Catalog.Api.Contracts.Products.Overview;
using Catalog.Domain.Pricing.Ppe;
using Catalog.Domain.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using YamlDotNet.RepresentationModel;
using static UnitTests.CatalogProductCreationTests;
using Catalog.Application.Abstractions;
using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Features.Pricing.Queries.PreviewPpePrices;
using Catalog.Application.Features.Product.Commands.CreateCatalogProduct;
using Catalog.Application.Features.Product.Queries.GetProductCodeCheck;
using Catalog.Application.Features.Product.Shared.Overview;

namespace UnitTests;

/// <summary>New product inputs (CAT-009): sewing codes, validation, code check, PPE preview and the contract.</summary>
public sealed class CatalogProductCreationInputTests
{
    [Test]
    public void Next_sewing_code_is_one_more_than_the_largest_mod_number()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SewingModelCodes.Next([]), Is.EqualTo("MOD-001"));
            Assert.That(SewingModelCodes.Next(["MOD-009", "MOD-124", "Костюм", "MOD-12a", "XMOD-900"]), Is.EqualTo("MOD-125"));
            Assert.That(SewingModelCodes.Next([" mod-041 "]), Is.EqualTo("MOD-042"));
            Assert.That(SewingModelCodes.Next(["MOD-1200"]), Is.EqualTo("MOD-1201"));
        });
    }

    [Test]
    public void Validator_accepts_a_sheet_row_of_each_type()
    {
        var validator = new CreateCatalogProductValidator();
        Assert.That(validator.Validate(Sewing()).IsValid, Is.True);
        Assert.That(validator.Validate(Ppe()).IsValid, Is.True);
    }

    [TestCase("ppe-without-code")]
    [TestCase("ppe-without-block")]
    [TestCase("sewing-with-block")]
    [TestCase("bad-site-url")]
    [TestCase("bad-supplier-url")]
    [TestCase("three-decimals")]
    [TestCase("ppe-season")]
    [TestCase("unknown-season")]
    public void Validator_rejects_wrong_input(string mistake)
    {
        var ppe = Ppe();
        var command = mistake switch
        {
            "ppe-without-code" => ppe with { Code = " " },
            "ppe-without-block" => ppe with { Ppe = null },
            "sewing-with-block" => Sewing() with { Ppe = ppe.Ppe },
            "bad-site-url" => Sewing() with { CurrentSiteUrl = "brevi.com.ua/kurtka" },
            "bad-supplier-url" => ppe with { Ppe = ppe.Ppe! with { SupplierUrl = "ftp://avcentr.com.ua/x" } },
            "three-decimals" => ppe with { Ppe = ppe.Ppe! with { SupplierPrice = 518.645m } },
            "ppe-season" => ppe with { Season = "Літо" },
            _ => Sewing() with { Season = "Осінь" },
        };

        Assert.That(new CreateCatalogProductValidator().Validate(command).IsValid, Is.False);
    }

    [Test]
    public async Task Code_check_reports_free_taken_and_suggestion_only()
    {
        var products = Existing();
        var repository = new Mock<ICatalogReadRepository<Product>>();
        repository.Setup(x => x.ListAsync(It.IsAny<ISpecification<Product, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<Product, string> spec, CancellationToken _) => spec.Evaluate(products).ToList());
        repository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<Product> spec, CancellationToken _) => spec.Evaluate(products).FirstOrDefault());
        var handler = new GetProductCodeCheckHandler(repository.Object);

        var free = await handler.Handle(new GetProductCodeCheckQuery("Ppe", " 18 "), CancellationToken.None);
        var taken = await handler.Handle(new GetProductCodeCheckQuery("Sewing", "MOD-124"), CancellationToken.None);
        var suggestion = await handler.Handle(new GetProductCodeCheckQuery("Sewing", null), CancellationToken.None);
        var unknown = new GetProductCodeCheckValidator().Validate(new GetProductCodeCheckQuery("Fabric", null));

        Assert.Multiple(() =>
        {
            Assert.That(free.Value, Is.EqualTo(new ProductCodeCheck("18", true, null, null, null)));
            Assert.That(taken.Value, Is.EqualTo(new ProductCodeCheck("MOD-124", false, 300, "Костюм «Верде»", "MOD-125")));
            Assert.That(suggestion.Value, Is.EqualTo(new ProductCodeCheck(null, null, null, null, "MOD-125")));
            Assert.That(unknown.IsValid, Is.False);
        });
    }

    [Test]
    public async Task Preview_calculates_the_three_prices_with_the_rule_and_refuses_an_unknown_rule()
    {
        var inputs = new Mock<IPpePricingInputReader>();
        inputs.Setup(x => x.ReadRuleAsync(RuleCode, It.IsAny<CancellationToken>())).ReturnsAsync(Rule());
        var handler = new PreviewPpePricesHandler(inputs.Object);

        var prices = await handler.Handle(new PreviewPpePricesQuery($" {RuleCode} ", 100m), CancellationToken.None);
        var unknown = await handler.Handle(new PreviewPpePricesQuery("Невідоме правило", 100m), CancellationToken.None);
        var noPrice = new PreviewPpePricesValidator().Validate(new PreviewPpePricesQuery(RuleCode, 0m));

        Assert.Multiple(() =>
        {
            Assert.That(prices.Value.Prices, Is.EqualTo(new PpePriceTiers(150m, 130m, 120m)));
            Assert.That(prices.Value.Blockers, Is.Empty);
            Assert.That(unknown.Status, Is.EqualTo(ResultStatus.Invalid));
            Assert.That(noPrice.IsValid, Is.False);
        });
    }

    [Test]
    public async Task Preview_with_a_draft_rule_has_the_missing_rule_blocker()
    {
        var inputs = new Mock<IPpePricingInputReader>();
        inputs.Setup(x => x.ReadRuleAsync(RuleCode, It.IsAny<CancellationToken>())).ReturnsAsync(Rule(approved: false));

        var result = await new PreviewPpePricesHandler(inputs.Object)
            .Handle(new PreviewPpePricesQuery(RuleCode, 100m), CancellationToken.None);

        Assert.That(result.Value.Blockers.Select(x => x.Code), Is.EqualTo(new[] { "MissingRule" }));
    }

    [TestCase(typeof(CreateCatalogProductRequest))]
    [TestCase(typeof(CreatePpeSupplierRequest))]
    [TestCase(typeof(CreatedCatalogProduct))]
    [TestCase(typeof(ProductCodeCheck))]
    public void Serialized_properties_match_the_product_overview_contract(Type type)
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        Assert.That(directory, Is.Not.Null);
        using var reader = File.OpenText(Path.Combine(directory!.FullName, "docs/sdd/contracts/catalog/product-overview.openapi.yaml"));
        var yaml = new YamlStream();
        yaml.Load(reader);
        var root = (YamlMappingNode)yaml.Documents[0].RootNode;
        var schemas = (YamlMappingNode)((YamlMappingNode)root["components"])["schemas"];
        var properties = (YamlMappingNode)((YamlMappingNode)schemas[type.Name])["properties"];
        Assert.That(properties.Children.Keys.Cast<YamlScalarNode>().Select(x => x.Value),
            Is.EquivalentTo(type.GetProperties().Select(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name))));
    }

    [TestCase("createCatalogProduct", "access:products")]
    [TestCase("getProductCodeCheck", "CatalogRead")]
    public void Creation_operation_exists_and_requires_the_expected_policy(string operation, string policy)
    {
        var methods = typeof(Catalog.Api.Controllers.ProductOverviewController).GetMethods()
            .Where(method => method.GetCustomAttributes(typeof(HttpMethodAttribute), false)
                .Cast<HttpMethodAttribute>().Any(attribute => attribute.Name == operation)).ToArray();
        Assert.That(methods, Has.Length.EqualTo(1), $"Missing operation {operation}.");
        Assert.That(methods[0].GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>().Select(attribute => attribute.Policy), Does.Contain(policy));
        Assert.That(methods[0].IsDefined(typeof(AllowAnonymousAttribute), true), Is.False);
    }
}
