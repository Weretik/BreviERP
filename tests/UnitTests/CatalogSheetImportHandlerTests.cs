using System.Text.Json;
using Ardalis.Result;
using Catalog.Api.Contracts.Products.Overview;
using Catalog.Application.Contracts.Persistence.SheetImport;
using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Application.Contracts.Reference;
using Catalog.Application.Contracts.SlugGeneration;
using Catalog.Application.Features.Product.SheetImport;
using Catalog.Application.Features.Product.SheetImport.DTOs;
using Catalog.Application.Features.Product.SheetImport.Planning;
using Catalog.Application.Features.Product.SheetImport.Validators;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Time.Testing;
using Moq;
using YamlDotNet.RepresentationModel;
using static UnitTests.CatalogSheetImportFixtures;

namespace UnitTests;

/// <summary>Sheet product import (CAT-010): handler writes, validator, article replacement and the contract.</summary>
public sealed class CatalogSheetImportHandlerTests
{
    [Test]
    public async Task Dry_run_reports_and_never_writes()
    {
        var setup = new Setup();

        var result = await setup.Handler.Handle(Command(dryRun: true), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Applied, Is.False);
            Assert.That(result.Value.Sewing.Created, Is.EqualTo(1));
        });
        setup.Store.Verify(x => x.ApplyAsync(It.IsAny<SheetImportPlan>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task A_problem_stops_the_write_even_without_dry_run()
    {
        var setup = new Setup();

        var result = await setup.Handler.Handle(Command([Sewing() with { Section = "Невідомий" }], dryRun: false),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Value.Applied, Is.False);
            Assert.That(result.Value.Problems, Is.Not.Empty);
        });
        setup.Store.Verify(x => x.ApplyAsync(It.IsAny<SheetImportPlan>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task A_clean_import_is_written_once_with_the_actor_and_only_known_rules()
    {
        var setup = new Setup();

        var result = await setup.Handler.Handle(Command(ppe: [Ppe(), Ppe("4", "Інший ЗІЗ", "Одяг ЗІЗ") with { RowNumber = 3 }],
            dryRun: false), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Value.Applied, Is.True);
            Assert.That(result.Value.Warnings.Single().RowNumber, Is.EqualTo(3));
            Assert.That(setup.Written!.Writes, Has.Count.EqualTo(3));
        });
        setup.Store.Verify(x => x.ApplyAsync(It.IsAny<SheetImportPlan>(), "manager-1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task A_rejected_write_is_a_conflict()
    {
        var setup = new Setup(Result.Conflict("Нічого не збережено."));

        var result = await setup.Handler.Handle(Command(dryRun: false), CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.Conflict));
    }

    [Test]
    public void Validator_accepts_a_body_and_rejects_an_empty_one()
    {
        var validator = new ImportSheetProductsCommandValidator();

        Assert.Multiple(() =>
        {
            Assert.That(validator.Validate(Command()).IsValid, Is.True);
            Assert.That(validator.Validate(Command([], [])).IsValid, Is.False);
            Assert.That(validator.Validate(Command([Sewing() with { Materials = null! }])).IsValid, Is.False);
        });
    }

    [Test]
    public void Replace_articles_keeps_the_id_of_a_kept_sku_and_removes_the_rest_with_their_lines()
    {
        var product = Product.Create(ProductId.Create(5), "Модель", "Модель", ProductSlug.Create("model").Value, ProductType.Sewing, Now).Value;
        product.AddArticle(SewingArticle.Create(ArticleId.Create(1), product.Id, "246").Value, Now);
        product.AddArticle(SewingArticle.Create(ArticleId.Create(2), product.Id, "247").Value, Now);
        product.ReplaceComposition([SewingMaterialLine.Create(default, product.Id, ArticleId.Create(2),
            SewingMaterialKind.Fabric, FabricReferenceId.Create(23), null, null, 1, SewingMaterialUnit.Metre, 0).Value], [], Now);

        product.ReplaceArticles([SewingArticle.Create(default, product.Id, "248", true, 0).Value,
            SewingArticle.Create(default, product.Id, "246", false, 1).Value], Now);

        Assert.Multiple(() =>
        {
            Assert.That(product.Articles.Select(x => (x.SaleSku, x.IsActive, x.SortOrder)),
                Is.EquivalentTo(new[] { ("246", false, 1), ("248", true, 0) }));
            Assert.That(product.Articles.Single(x => x.SaleSku == "246").Id.Value, Is.EqualTo(1));
            Assert.That(product.MaterialLines, Is.Empty);
        });
    }

    [TestCase(typeof(ImportSheetProductsRequest))]
    [TestCase(typeof(SheetSewingRow))]
    [TestCase(typeof(SheetSewingArticleRow))]
    [TestCase(typeof(SheetSewingMaterialRow))]
    [TestCase(typeof(SheetSewingOperationRow))]
    [TestCase(typeof(SheetPpeRow))]
    [TestCase(typeof(SheetImportReport))]
    [TestCase(typeof(SheetImportCounts))]
    [TestCase(typeof(SheetImportDeletion))]
    [TestCase(typeof(SheetImportIssue))]
    public void Serialized_properties_match_the_sheet_import_contract(Type type)
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

    [Test]
    public void Sheet_import_operation_exists_and_requires_product_management()
    {
        var methods = typeof(Catalog.Api.Controllers.ProductOverviewController).GetMethods()
            .Where(method => method.GetCustomAttributes(typeof(HttpMethodAttribute), false)
                .Cast<HttpMethodAttribute>().Any(attribute => attribute.Name == "importSheetProducts")).ToArray();
        Assert.That(methods, Has.Length.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(methods[0].GetCustomAttributes(typeof(HttpMethodAttribute), false).Cast<HttpMethodAttribute>()
                .Single().Template, Is.EqualTo("sheet-import"));
            Assert.That(methods[0].GetCustomAttributes(typeof(AuthorizeAttribute), true)
                .Cast<AuthorizeAttribute>().Select(attribute => attribute.Policy), Does.Contain("access:products"));
            Assert.That(methods[0].IsDefined(typeof(AllowAnonymousAttribute), true), Is.False);
        });
    }

    private sealed class Setup
    {
        public Mock<ISheetProductImportStore> Store { get; } = new();
        public SheetImportPlan? Written { get; private set; }
        public ImportSheetProductsCommandHandler Handler { get; }

        public Setup(Result? write = null)
        {
            Store.Setup(x => x.LoadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SheetImportStoredState([], new HashSet<int>(), new HashSet<long>()));
            Store.Setup(x => x.ApplyAsync(It.IsAny<SheetImportPlan>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<SheetImportPlan, string, CancellationToken>((plan, _, _) => Written = plan)
                .ReturnsAsync(write ?? Result.Success());
            var codes = new Mock<IProductReferenceCodeReader>();
            codes.Setup(x => x.ReadAsync(It.IsAny<CancellationToken>())).ReturnsAsync(References());
            var inputs = new Mock<IPpePricingInputReader>();
            inputs.Setup(x => x.ReadRuleAsync(Rule, It.IsAny<CancellationToken>())).ReturnsAsync(new PpeRuleVersion(
                Guid.NewGuid(), new PpeRuleValues(Rule, new Catalog.Domain.Pricing.Ppe.PpeMarkups(0.5m, 0.3m, 0.2m), "x", "01.10.2026"),
                true, "x", Now, Now));
            var slugs = new Mock<IProductSlugGenerator>();
            slugs.Setup(x => x.GenerateFromUkrainianName(It.IsAny<string>())).Returns("tovar");
            Handler = new ImportSheetProductsCommandHandler(Store.Object, codes.Object, inputs.Object, slugs.Object,
                new FakeTimeProvider(Now));
        }
    }
}
