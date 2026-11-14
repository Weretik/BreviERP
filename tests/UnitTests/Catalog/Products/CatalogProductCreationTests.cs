using Ardalis.Result;
using Ardalis.Specification;
using Catalog.Domain.Pricing.Ppe;
using Catalog.Domain.Products;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Catalog.Application.Abstractions;
using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Reference;
using Catalog.Application.Features.Product.Commands.CreateCatalogProduct;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;
using Catalog.Application.Abstractions.Pricing.Ppe;
using Catalog.Application.Abstractions.Pricing.Ppe.Versions;
using Catalog.Application.Abstractions.Products;
using Catalog.Application.Abstractions.Reference.Items;

namespace UnitTests.Catalog.Products;

/// <summary>Creating a product from the pages «Новий ЗІЗ» and «Нова модель» (CAT-009).</summary>
public sealed class CatalogProductCreationTests
{
    internal static readonly DateTimeOffset Now = new(2026, 11, 3, 15, 0, 0, TimeSpan.Zero);
    internal const string RuleCode = "Одяг ЗІЗ — АВ-Центр";

    [Test]
    public async Task Sewing_model_without_code_gets_the_next_code_an_id_and_stays_inactive_when_asked()
    {
        var setup = new Setup();

        var result = await setup.Handler.Handle(Sewing(isActive: false), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True, string.Join(" ", result.ValidationErrors.Select(x => x.ErrorMessage)));
        var product = setup.Added!;
        Assert.Multiple(() =>
        {
            Assert.That(result.Value, Is.EqualTo(new CreatedCatalogProduct(301, "Sewing", "MOD-125", true)));
            Assert.That(product.Id.Value, Is.EqualTo(301));
            Assert.That(product.ModelCode, Is.EqualTo("MOD-125"));
            Assert.That(product.RuName, Is.EqualTo(product.Name));
            Assert.That(product.Slug.Value, Is.EqualTo("kurtka-verde"));
            Assert.That(product.CatalogProfile!.IsActive, Is.False);
            Assert.That(product.CatalogProfile.SectionId, Is.EqualTo(2));
            Assert.That(product.CatalogProfile.Season, Is.EqualTo("Літо"));
        });
        setup.Offers.VerifyNoOtherCalls();
    }

    [Test]
    public async Task First_product_gets_id_one()
    {
        var setup = new Setup(withExisting: false);

        var result = await setup.Handler.Handle(Sewing(), CancellationToken.None);

        Assert.That(result.Value.Id, Is.EqualTo(1));
        Assert.That(result.Value.Code, Is.EqualTo("MOD-001"));
    }

    [Test]
    public async Task Taken_slug_gets_the_code_and_dots_from_the_generator_become_hyphens()
    {
        var setup = new Setup();
        setup.Slugs.Setup(x => x.GenerateFromUkrainianName("Куртка «Верде»")).Returns("kostium-verde");
        setup.Slugs.Setup(x => x.GenerateFromUkrainianName("Рукавиці 5.5")).Returns("rukavytsi-5.5");

        await setup.Handler.Handle(Sewing(), CancellationToken.None);
        Assert.That(setup.Added!.Slug.Value, Is.EqualTo("kostium-verde-mod-125"));

        await setup.Handler.Handle(Sewing(name: "Рукавиці 5.5", code: "MOD-200"), CancellationToken.None);
        Assert.That(setup.Added!.Slug.Value, Is.EqualTo("rukavytsi-5-5"));
    }

    [Test]
    public async Task Ppe_product_gets_supplier_fields_and_its_first_price_version()
    {
        var setup = new Setup();
        PpeOfferValues? written = null;
        setup.Offers.Setup(x => x.AppendAsync(301, null, It.IsAny<Guid>(), It.IsAny<PpeOfferValues>(), "manager",
                It.IsAny<CancellationToken>()))
            .Callback<int, Guid?, Guid, PpeOfferValues, string, CancellationToken>((_, _, _, values, _, _) => written = values)
            .ReturnsAsync(Result.Success(new PpeOfferVersion(Guid.NewGuid(), 301,
                new PpeOfferValues(RuleCode, 518.64m, null, null, "x", "03.11.2026"), true, "manager", Now, Now)));

        var result = await setup.Handler.Handle(Ppe(), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True, string.Join(" ", result.ValidationErrors.Select(x => x.ErrorMessage)));
        var profile = setup.Added!.CatalogProfile!;
        Assert.Multiple(() =>
        {
            Assert.That(result.Value, Is.EqualTo(new CreatedCatalogProduct(301, "Ppe", "18", true)));
            Assert.That(profile.SaleSku, Is.EqualTo("18"));
            Assert.That(profile.SupplierId?.Value, Is.EqualTo(5));
            Assert.That(profile.SupplierPrice, Is.EqualTo(518.64m));
            Assert.That(profile.PricingRuleCode, Is.EqualTo(RuleCode));
            Assert.That(profile.Unit, Is.EqualTo("шт."));
            Assert.That(written!.Source, Is.EqualTo(CreateCatalogProductHandler.OfferSource));
            Assert.That(written.Source, Is.EqualTo("Створення ЗІЗ"));
            Assert.That(written.SupplierCost, Is.EqualTo(518.64m));
            Assert.That(written.RuleCode, Is.EqualTo(RuleCode));
            Assert.That(written.SourceDate, Is.EqualTo("03.11.2026"));
        });
    }

    [Test]
    public async Task Price_not_saved_is_reported_but_the_product_stays()
    {
        var setup = new Setup();
        setup.Offers.Setup(x => x.AppendAsync(It.IsAny<int>(), It.IsAny<Guid?>(), It.IsAny<Guid>(),
                It.IsAny<PpeOfferValues>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PpeOfferVersion>.Conflict());

        var result = await setup.Handler.Handle(Ppe(), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.PriceSaved, Is.False);
        setup.Repository.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Taken_code_and_taken_name_are_conflicts()
    {
        var setup = new Setup();

        var code = await setup.Handler.Handle(Ppe(code: "17"), CancellationToken.None);
        var sewingCode = await setup.Handler.Handle(Sewing(code: "MOD-124"), CancellationToken.None);
        var name = await setup.Handler.Handle(Sewing(name: "Костюм «Верде»"), CancellationToken.None);
        var ruName = await setup.Handler.Handle(Sewing(name: "Костюм «Верде» RU"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(code.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(sewingCode.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(name.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(ruName.Status, Is.EqualTo(ResultStatus.Conflict));
        });
        setup.Repository.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Wrong_section_unknown_supplier_and_unknown_rule_are_invalid()
    {
        var setup = new Setup();

        var section = await setup.Handler.Handle(Ppe(sectionId: 2), CancellationToken.None);
        var missingSection = await setup.Handler.Handle(Sewing(sectionId: 99), CancellationToken.None);
        var supplier = await setup.Handler.Handle(Ppe(supplierId: 99), CancellationToken.None);
        var rule = await setup.Handler.Handle(Ppe(rule: "Невідоме правило"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(section.Status, Is.EqualTo(ResultStatus.Invalid));
            Assert.That(missingSection.Status, Is.EqualTo(ResultStatus.Invalid));
            Assert.That(supplier.ValidationErrors.Single().Identifier, Is.EqualTo("Ppe.SupplierId"));
            Assert.That(rule.ValidationErrors.Single().Identifier, Is.EqualTo("Ppe.RuleCode"));
        });
        setup.Repository.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>The handler over an in-memory product table: specifications are evaluated against it.</summary>
    private sealed class Setup
    {
        public Mock<ICatalogRepository<Product>> Repository { get; } = new();
        public Mock<IPpeOfferVersionWriter> Offers { get; } = new();
        public Mock<IProductSlugGenerator> Slugs { get; } = new();
        public Product? Added { get; private set; }
        public CreateCatalogProductHandler Handler { get; }

        public Setup(bool withExisting = true)
        {
            var products = withExisting ? Existing() : [];
            Repository.Setup(x => x.AnyAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ISpecification<Product> spec, CancellationToken _) => spec.Evaluate(products).Any());
            Repository.Setup(x => x.ListAsync(It.IsAny<ISpecification<Product, string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ISpecification<Product, string> spec, CancellationToken _) => spec.Evaluate(products).ToList());
            Repository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ISpecification<Product, ProductId>>(), It.IsAny<CancellationToken>()))
                // ProductId is ordered by its database value; in memory it is not comparable.
                .ReturnsAsync(() => products.Select(x => x.Id).OrderByDescending(x => x.Value).FirstOrDefault());
            Repository.Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
                .Callback<Product, CancellationToken>((product, _) => Added = product)
                .ReturnsAsync((Product product, CancellationToken _) => product);

            var sections = new Mock<IProductSectionReader>();
            sections.Setup(x => x.FindAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(new ProductSectionItem(2, "Sewing", "Літній одяг", true));
            sections.Setup(x => x.FindAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(new ProductSectionItem(7, "Ppe", "Захисний одяг", true));
            var references = new Mock<IProductReferenceReader>();
            references.Setup(x => x.GetSnapshotAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new ProductReferenceData(
                new Dictionary<int, ReferenceItem> { [5] = new(5, "АВ-Центр") },
                new Dictionary<int, PricedReferenceItem>(), new Dictionary<int, PricedReferenceItem>(),
                new Dictionary<int, OperationReferenceItem>(), new Dictionary<int, AdditionalReferenceItem>()));
            var inputs = new Mock<IPpePricingInputReader>();
            inputs.Setup(x => x.ReadRuleAsync(RuleCode, It.IsAny<CancellationToken>())).ReturnsAsync(Rule());
            Slugs.Setup(x => x.GenerateFromUkrainianName(It.IsAny<string>())).Returns("kurtka-verde");

            Handler = new CreateCatalogProductHandler(Repository.Object, sections.Object, references.Object,
                inputs.Object, Offers.Object, Slugs.Object, new FakeTimeProvider(Now));
        }
    }

    internal static PpeRuleVersion Rule(bool approved = true)
        => new(Guid.NewGuid(), new PpeRuleValues(RuleCode, new PpeMarkups(0.5m, 0.3m, 0.2m), "pricing-parameters",
            "03.11.2026"), approved, "x", Now, Now);

    /// <summary>A sewing model MOD-124 (ID 300) and a PPE article 17 (ID 120).</summary>
    internal static List<Product> Existing()
    {
        var sewing = Product.Create(ProductId.Create(300), "Костюм «Верде»", "Костюм «Верде» RU",
            ProductSlug.Create("kostium-verde").Value, ProductType.Sewing, Now).Value;
        sewing.SetCode("MOD-124", Now);
        var ppe = Product.Create(ProductId.Create(120), "Костюм ВОЛОГОЗАХИСНИЙ", "Костюм ВОЛОГОЗАХИСНИЙ RU",
            ProductSlug.Create("kostyum-vologozahysnyj").Value, ProductType.Ppe, Now).Value;
        ppe.SetCode("17", Now);
        return [sewing, ppe];
    }

    internal static CreateCatalogProductCommand Sewing(
        string name = "Куртка «Верде»", string? code = null, int sectionId = 2, bool isActive = true)
        => new("Sewing", code, name, sectionId, "Літо", isActive, null, null, null, "manager");

    internal static CreateCatalogProductCommand Ppe(
        string code = "18", int sectionId = 7, int supplierId = 5, string rule = RuleCode)
        => new("Ppe", code, "Костюм CARINA", sectionId, null, true, null, "Опис для КП",
            new CreatePpeSupplierInput(supplierId, "CARINA", "https://avcentr.com.ua/ua/kostyum-carina", 518.64m,
                false, rule, "шт."), "manager");
}
