using Ardalis.Result;
using Ardalis.Specification;
using Catalog.Application.Contracts.Admin;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Contracts.Reference;
using Catalog.Application.Contracts.SlugGeneration;
using Catalog.Application.Features.Product.Create.DTOs;
using Catalog.Application.Features.Product.GetAdminDetail.DTOs;
using Catalog.Application.Features.Product.Sewing;
using Catalog.Application.Features.Product.Update;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;
using Moq;
using ProductCategory = Catalog.Domain.ProductCategories.Entities.ProductCategory;

namespace UnitTests;

public sealed class SewingCompositionHandlerTests
{
    [Test]
    public async Task Stale_article_or_composition_version_does_not_read_references_or_save()
    {
        var fixture = new Fixture();
        fixture.Product.AddArticle(SewingArticle.Create(ArticleId.Create(1), fixture.Product.Id, "SKU-1"), DateTimeOffset.UtcNow);

        var result = await fixture.CompositionHandler.Handle(new ReplaceSewingCompositionCommand(101,
            new(null, [], [], 0), "manager-1"), CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.Conflict));
        fixture.Store.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
        fixture.ReferenceReader.Verify(x => x.GetSnapshotAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Stale_article_create_does_not_check_sku_or_save()
    {
        var fixture = new Fixture();
        fixture.Product.AddArticle(SewingArticle.Create(ArticleId.Create(1), fixture.Product.Id, "SKU-1"), DateTimeOffset.UtcNow);

        var result = await fixture.ArticleHandler.Handle(new CreateSewingArticleCommand(101,
            new("SKU-2", true, 0, 0)), CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.Conflict));
        fixture.Store.Verify(x => x.SaleSkuExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        fixture.Store.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Missing_reference_returns_not_found_without_saving()
    {
        var fixture = new Fixture();
        fixture.ReferenceReader.Setup(x => x.GetSnapshotAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(References());

        var result = await fixture.CompositionHandler.Handle(new ReplaceSewingCompositionCommand(101,
            new(null, [FabricInput(999)], [], 0), "manager-1"), CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.NotFound));
        fixture.Store.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Invalid_quantity_or_unit_returns_indexed_validation_error_before_reference_read()
    {
        var fixture = new Fixture();
        var result = await fixture.CompositionHandler.Handle(new ReplaceSewingCompositionCommand(101,
            new(null, [FabricInput(10) with { Quantity = 0, Unit = "Piece" }], [], 0), "manager-1"), CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.Invalid));
        Assert.That(result.ValidationErrors.Select(x => x.Identifier), Does.Contain("materials[0].quantity"));
        Assert.That(result.ValidationErrors.Select(x => x.Identifier), Does.Contain("materials[0].unit"));
        fixture.ReferenceReader.Verify(x => x.GetSnapshotAsync(It.IsAny<CancellationToken>()), Times.Never);
        fixture.Store.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Duplicate_sale_sku_returns_conflict_without_save()
    {
        var fixture = new Fixture();
        fixture.Store.Setup(x => x.SaleSkuExistsAsync("246", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await fixture.ArticleHandler.Handle(new CreateSewingArticleCommand(101,
            new("246", true, 0, 0)), CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.Conflict));
        fixture.Store.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Article_composition_for_another_product_returns_not_found_without_save()
    {
        var fixture = new Fixture();
        fixture.Product.AddArticle(SewingArticle.Create(ArticleId.Create(21), fixture.Product.Id, "SKU-21"), DateTimeOffset.UtcNow);
        fixture.Store.Setup(x => x.LoadAsync(101, It.IsAny<CancellationToken>())).ReturnsAsync(fixture.Product);

        var result = await fixture.ArticleCompositionHandler.Handle(new ReplaceSewingArticleCompositionCommand(101, 22,
            new([], fixture.Product.CompositionVersion), "manager-1"), CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.NotFound));
        fixture.ReferenceReader.Verify(x => x.GetSnapshotAsync(It.IsAny<CancellationToken>()), Times.Never);
        fixture.Store.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Common_and_article_fabric_conflict_leaves_existing_composition_unchanged()
    {
        var fixture = new Fixture();
        var article = SewingArticle.Create(ArticleId.Create(21), fixture.Product.Id, "SKU-21");
        fixture.Product.AddArticle(article, DateTimeOffset.UtcNow);
        var existing = SewingMaterialLine.Create(CompositionLineId.Create(4), fixture.Product.Id, article.Id,
            SewingMaterialKind.Fabric, FabricReferenceId.Create(10), null, null, 1m, SewingMaterialUnit.Metre, true, 0);
        fixture.Product.ReplaceComposition([existing], [], DateTimeOffset.UtcNow);
        fixture.Store.Setup(x => x.LoadAsync(101, It.IsAny<CancellationToken>())).ReturnsAsync(fixture.Product);
        fixture.ReferenceReader.Setup(x => x.GetSnapshotAsync(It.IsAny<CancellationToken>())).ReturnsAsync(References());

        var result = await fixture.CompositionHandler.Handle(new ReplaceSewingCompositionCommand(101,
            new(null, [FabricInput(10)], [], fixture.Product.CompositionVersion), "manager-1"), CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(fixture.Product.MaterialLines, Has.Count.EqualTo(1));
        Assert.That(fixture.Product.MaterialLines.Single().Id, Is.EqualTo(existing.Id));
        fixture.Store.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Reviewed_operation_records_authenticated_actor_and_server_time()
    {
        var fixture = new Fixture();
        fixture.ReferenceReader.Setup(x => x.GetSnapshotAsync(It.IsAny<CancellationToken>())).ReturnsAsync(References(includeOperation: true));
        fixture.Store.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        fixture.Store.Setup(x => x.ReadCompositionAsync(101, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SewingComposition(101, null, null, fixture.Product.CompositionVersion, [], [], []));

        var result = await fixture.CompositionHandler.Handle(new ReplaceSewingCompositionCommand(101,
            new(null, [], [new(5, 1m, null, 0, true)], 0), "authenticated-manager"), CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.Ok));
        var line = fixture.Product.OperationLines.Single();
        Assert.That(line.IsReviewed, Is.True);
        Assert.That(line.ReviewedBy, Is.EqualTo("authenticated-manager"));
        Assert.That(line.ReviewedAt, Is.Not.Null);
    }

    [Test]
    public async Task Legacy_norm_edit_is_rejected_when_composition_exists_but_same_norms_allow_other_edits()
    {
        var fixture = new ReplaceFixture();
        var product = Fixture.NewSewingProduct();
        var legacy = SewingProductDetails.Create(product.Id, 1m, [new ProductFabric(product.Id, 10, true, 0)], [], []);
        product.ConfigureSewing(legacy, DateTimeOffset.UtcNow);
        var article = SewingArticle.Create(ArticleId.Create(24), product.Id, "SKU-24");
        product.AddArticle(article, DateTimeOffset.UtcNow);
        product.ReplaceComposition([SewingMaterialLine.Create(CompositionLineId.Create(10), product.Id, null,
            SewingMaterialKind.Fabric, FabricReferenceId.Create(10), null, null, 2m, SewingMaterialUnit.Metre, true, 0)], [], DateTimeOffset.UtcNow);
        fixture.LoadProduct(product);

        var changedNorm = await fixture.Handler.Handle(new ReplaceProductCommand(101,
            Request("New name", meters: 2m)), CancellationToken.None);
        Assert.That(changedNorm.Status, Is.EqualTo(ResultStatus.Conflict));
        fixture.Repository.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);

        var ordinaryEdit = await fixture.Handler.Handle(new ReplaceProductCommand(101,
            Request("New name")), CancellationToken.None);
        Assert.That(ordinaryEdit.Status, Is.EqualTo(ResultStatus.Ok));
        Assert.That(product.Articles, Has.Count.EqualTo(1));
        Assert.That(product.MaterialLines, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Legacy_replace_cannot_change_product_to_ppe_when_articles_exist()
    {
        var fixture = new ReplaceFixture();
        var product = Fixture.NewSewingProduct();
        product.AddArticle(SewingArticle.Create(ArticleId.Create(24), product.Id, "SKU-24"), DateTimeOffset.UtcNow);
        fixture.LoadProduct(product);

        var result = await fixture.Handler.Handle(new ReplaceProductCommand(101, PpeRequest()), CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(product.Type, Is.EqualTo(ProductType.Sewing));
        fixture.Repository.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static SewingMaterialInput FabricInput(int id) => new("Fabric", id, null, 1m, "Metre", true, 0, false);

    private static ProductReferenceData References(bool includeOperation = false) => new(
        new Dictionary<int, ReferenceItem>(), new Dictionary<int, PricedReferenceItem> { [10] = new(10, "Fabric", 10m) },
        new Dictionary<int, PricedReferenceItem>(), includeOperation
            ? new Dictionary<int, OperationReferenceItem> { [5] = new(5, "Operation", 1m) }
            : new Dictionary<int, OperationReferenceItem>(), new Dictionary<int, AdditionalReferenceItem>());

    private sealed class Fixture
    {
        public Product Product { get; } = NewSewingProduct();
        public Mock<ISewingCompositionStore> Store { get; } = new();
        public Mock<IProductReferenceReader> ReferenceReader { get; } = new();
        public CreateSewingArticleHandler ArticleHandler { get; }
        public ReplaceSewingCompositionHandler CompositionHandler { get; }
        public ReplaceSewingArticleCompositionHandler ArticleCompositionHandler { get; }

        public Fixture()
        {
            Store.Setup(x => x.LoadAsync(101, It.IsAny<CancellationToken>())).ReturnsAsync(Product);
            Store.Setup(x => x.SaleSkuExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
            ArticleHandler = new(Store.Object);
            CompositionHandler = new(Store.Object, ReferenceReader.Object);
            ArticleCompositionHandler = new(Store.Object, ReferenceReader.Object);
        }

        public static Product NewSewingProduct() => Product.Create(ProductId.Create(101), "Old", "Old RU",
            ProductSlug.Create("old"), ProductType.Sewing, DateTimeOffset.UtcNow);
    }

    private sealed class ReplaceFixture
    {
        public Mock<ICatalogRepository<Product>> Repository { get; } = new();
        private readonly Mock<ICatalogReadRepository<Catalog.Domain.Media.Entities.MediaFile>> _media = new();
        private readonly Mock<IProductReferenceReader> _references = new();
        private readonly Mock<ICatalogReadRepository<ProductCategory>> _categories = new();
        private readonly Mock<IProductSlugGenerator> _slug = new();
        public ReplaceProductCommandHandler Handler { get; }

        public ReplaceFixture()
        {
            _references.Setup(x => x.GetSnapshotAsync(It.IsAny<CancellationToken>())).ReturnsAsync(References());
            _categories.Setup(x => x.ListAsync(It.IsAny<ISpecification<ProductCategory>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
            _slug.Setup(x => x.GenerateFromUkrainianName(It.IsAny<string>())).Returns("new-name");
            Repository.Setup(x => x.AnyAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
            Repository.Setup(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>())).ReturnsAsync(1);
            Repository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);
            Repository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ISpecification<Product, ProductAdminDetailReadModel>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProductAdminDetailReadModel(101, "New name", "New name", "new-name", ProductType.Sewing,
                    "Description", "Description", [], [], [], [], new SewingReadModel(1m, [new FabricReadModel(10, true, 0)], [], []),
                    null, DateTimeOffset.UtcNow, null));
            Handler = new(Repository.Object, _media.Object, _references.Object, _categories.Object, _slug.Object);
        }

        public void LoadProduct(Product product) => Repository.Setup(x => x.FirstOrDefaultAsync(
            It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>())).ReturnsAsync(product);
    }

    private static CreateProductCommandRequest Request(string name, decimal meters = 1m) => new(101, name, name,
        ProductType.Sewing, "Description", "Description", [], [], [], [],
        new SewingProductRequest(meters, [new FabricRequest(10, true, 0)], [], []), null);

    private static CreateProductCommandRequest PpeRequest() => new(101, "PPE", "PPE", ProductType.Ppe,
        "Description", "Description", [], [], [], [], null,
        new PpeProductRequest(1, 10m, new(PricePercentSource.Custom, null, 10m), new(PricePercentSource.Custom, null, 5m)));
}
