using Ardalis.Result;
using Ardalis.Specification;
using Catalog.Application.Contracts.Admin;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Contracts.Reference;
using Catalog.Application.Contracts.SlugGeneration;
using Catalog.Application.Features.Product.Create;
using Catalog.Application.Features.Product.Create.DTOs;
using Catalog.Application.Features.Product.GetAdminDetail.DTOs;
using Catalog.Application.Features.Product.Update;
using Catalog.Domain.Media.Entities;
using Catalog.Domain.Media.ValueObjects;
using Catalog.Domain.ProductCategories.Entities;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;
using Moq;

namespace UnitTests;

public sealed class ProductCommandHandlerTests
{
    [Test]
    public async Task Create_succeeds_with_valid_references()
    {
        var fixture = new Fixture();
        fixture.ProductRepository.Setup(x => x.AnyAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        fixture.ProductRepository.Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>())).ReturnsAsync((Product product, CancellationToken _) => product);
        fixture.CategoryRepository.Setup(x => x.ListAsync(It.IsAny<ISpecification<ProductCategory>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var result = await fixture.CreateHandler.Handle(new CreateProductCommand(SewingRequest()), CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.Ok));
        fixture.ProductRepository.Verify(x => x.AddAsync(It.Is<Catalog.Domain.Products.Entities.Product>(p => p.Id == ProductId.Create(101)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Create_returns_conflict_for_duplicate_product()
    {
        var fixture = new Fixture();
        fixture.ProductRepository.Setup(x => x.AnyAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await fixture.CreateHandler.Handle(new CreateProductCommand(SewingRequest()), CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.Conflict));
        fixture.ProductRepository.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Create_returns_conflict_for_missing_reference_or_unavailable_media()
    {
        var missingReference = new Fixture();
        missingReference.ProductRepository.Setup(x => x.AnyAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        missingReference.ReferenceReader.Setup(x => x.GetSnapshotAsync(It.IsAny<CancellationToken>())).ReturnsAsync(References() with { Fabrics = new Dictionary<int, PricedReferenceItem>() });

        var missingReferenceResult = await missingReference.CreateHandler.Handle(new CreateProductCommand(SewingRequest()), CancellationToken.None);

        var unavailableMedia = new Fixture();
        unavailableMedia.ProductRepository.Setup(x => x.AnyAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var unavailableMediaResult = await unavailableMedia.CreateHandler.Handle(
            new CreateProductCommand(SewingRequest() with { Photos = [new ProductPhotoRequest(77, null, true, true, 0)] }), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(missingReferenceResult.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(unavailableMediaResult.Status, Is.EqualTo(ResultStatus.Conflict));
        });
    }

    [Test]
    public async Task Create_returns_conflict_for_missing_category_before_writing_product()
    {
        var fixture = new Fixture();
        fixture.ProductRepository.Setup(x => x.AnyAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await fixture.CreateHandler.Handle(
            new CreateProductCommand(SewingRequest() with { CategoryIds = [9] }), CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.Conflict));
        fixture.ProductRepository.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Create_assigns_deterministic_unique_photo_ids_within_product()
    {
        var fixture = new Fixture();
        Product? savedProduct = null;
        fixture.ProductRepository.Setup(x => x.AnyAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        fixture.ProductRepository.Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((product, _) => savedProduct = product)
            .ReturnsAsync((Product product, CancellationToken _) => product);
        fixture.MediaRepository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ISpecification<MediaFile>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ReadyMediaFile(77));

        var result = await fixture.CreateHandler.Handle(new CreateProductCommand(SewingRequest() with
        {
            Photos = [new ProductPhotoRequest(77, null, true, true, 0), new ProductPhotoRequest(78, null, true, false, 1)]
        }), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(ResultStatus.Ok));
            Assert.That(savedProduct!.Photos.Select(x => x.Id.Value), Is.EqualTo([1, 2]));
        });
    }

    [Test]
    public async Task Replace_returns_not_found_conflict_missing_reference_unavailable_media_and_succeeds()
    {
        var notFound = new Fixture();
        notFound.ProductRepository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);
        var notFoundResult = await notFound.ReplaceHandler.Handle(new ReplaceProductCommand(101, SewingRequest()), CancellationToken.None);

        var conflict = new Fixture();
        conflict.ProductRepository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>())).ReturnsAsync(NewProduct());
        conflict.ProductRepository.Setup(x => x.AnyAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var conflictResult = await conflict.ReplaceHandler.Handle(new ReplaceProductCommand(101, SewingRequest()), CancellationToken.None);

        var missingReference = new Fixture();
        missingReference.ProductRepository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>())).ReturnsAsync(NewProduct());
        missingReference.ProductRepository.Setup(x => x.AnyAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        missingReference.ReferenceReader.Setup(x => x.GetSnapshotAsync(It.IsAny<CancellationToken>())).ReturnsAsync(References() with { Fabrics = new Dictionary<int, PricedReferenceItem>() });
        var missingReferenceResult = await missingReference.ReplaceHandler.Handle(new ReplaceProductCommand(101, SewingRequest()), CancellationToken.None);

        var unavailableMedia = new Fixture();
        unavailableMedia.ProductRepository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>())).ReturnsAsync(NewProduct());
        unavailableMedia.ProductRepository.Setup(x => x.AnyAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var unavailableMediaResult = await unavailableMedia.ReplaceHandler.Handle(
            new ReplaceProductCommand(101, SewingRequest() with { Photos = [new ProductPhotoRequest(77, null, true, true, 0)] }), CancellationToken.None);

        var success = new Fixture();
        success.ProductRepository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>())).ReturnsAsync(NewProduct());
        success.ProductRepository.Setup(x => x.AnyAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        success.ProductRepository.Setup(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>())).ReturnsAsync(1);
        success.CategoryRepository.Setup(x => x.ListAsync(It.IsAny<ISpecification<ProductCategory>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var successResult = await success.ReplaceHandler.Handle(new ReplaceProductCommand(101, SewingRequest()), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(notFoundResult.Status, Is.EqualTo(ResultStatus.NotFound));
            Assert.That(conflictResult.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(missingReferenceResult.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(unavailableMediaResult.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(successResult.Status, Is.EqualTo(ResultStatus.Ok));
        });
    }

    private static CreateProductCommandRequest SewingRequest() => new(101, "Куртка", "Куртка", ProductType.Sewing, "Опис", "Описание", [], [], [], [], new SewingProductRequest(1m, [new FabricRequest(10, true, 0)], [], []), null);

    private static Product NewProduct() => Product.Create(ProductId.Create(101), "Стара куртка", "Старая куртка", ProductSlug.Create("stara-kurtka"), ProductType.Sewing, DateTimeOffset.UtcNow);

    private static ProductReferenceData References() => new(
        new Dictionary<int, ReferenceItem>(), new Dictionary<int, PricedReferenceItem> { [10] = new(10, "Тканина", 100m) },
        new Dictionary<int, PricedReferenceItem>(), new Dictionary<int, OperationReferenceItem>(), new Dictionary<int, AdditionalReferenceItem>());

    private static MediaFile ReadyMediaFile(int id)
    {
        var media = MediaFile.CreatePending(MediaFileId.Create(id), "photo.jpg", "image/jpeg", 1, "test", "catalog", $"products/{id}.jpg");
        media.MarkUploaded($"https://example.test/{id}.jpg", null, null);
        return media;
    }

    private sealed class Fixture
    {
        public Mock<ICatalogRepository<Product>> ProductRepository { get; } = new();
        public Mock<ICatalogReadRepository<MediaFile>> MediaRepository { get; } = new();
        public Mock<IProductReferenceReader> ReferenceReader { get; } = new();
        public Mock<ICatalogReadRepository<ProductCategory>> CategoryRepository { get; } = new();
        private Mock<IProductSlugGenerator> SlugGenerator { get; } = new();
        public CreateProductCommandHandler CreateHandler { get; }
        public ReplaceProductCommandHandler ReplaceHandler { get; }

        public Fixture()
        {
            ReferenceReader.Setup(x => x.GetSnapshotAsync(It.IsAny<CancellationToken>())).ReturnsAsync(References());
            ProductRepository.Setup(x => x.FirstOrDefaultAsync(
                    It.IsAny<ISpecification<Product, ProductAdminDetailReadModel>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ReadModel());
            CategoryRepository.Setup(x => x.ListAsync(It.IsAny<ISpecification<ProductCategory>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
            SlugGenerator.Setup(x => x.GenerateFromUkrainianName(It.IsAny<string>())).Returns("kurtka");
            CreateHandler = new(ProductRepository.Object, MediaRepository.Object, ReferenceReader.Object, CategoryRepository.Object, SlugGenerator.Object);
            ReplaceHandler = new(ProductRepository.Object, MediaRepository.Object, ReferenceReader.Object, CategoryRepository.Object, SlugGenerator.Object);
        }

        private static ProductAdminDetailReadModel ReadModel() => new(
            101, "Куртка", "Куртка", "kurtka", ProductType.Sewing, "Опис", "Описание", [], [], [], [],
            new SewingReadModel(1m, [new FabricReadModel(10, true, 0)], [], []), null, DateTimeOffset.UtcNow, null);
    }
}
