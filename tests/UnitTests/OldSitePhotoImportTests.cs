using Ardalis.Result;
using Ardalis.Specification;
using Catalog.Domain.Media;
using Catalog.Domain.Products;
using Mediator;
using Moq;
using Catalog.Application.Abstractions;
using Catalog.Application.Abstractions.Media;
using Catalog.Application.Abstractions.Reference;
using Catalog.Application.Features.Product.Commands.CreateCatalogProduct;
using Catalog.Application.Features.Product.Commands.ImportOldSitePhotos;
using Catalog.Application.Features.Product.Commands.SetProductPhotos;
using Catalog.Application.Features.Product.Queries.GetProductSkuIndex;
using Catalog.Application.Features.Product.Shared.Photos;

namespace UnitTests;

/// <summary>Photos and missing sewing models from the old site (CAT-016).</summary>
public sealed class OldSitePhotoImportTests
{
    private const string BaseUrl = "https://brevi.com.ua/image/";

    [Test]
    public async Task Dry_run_finds_products_by_sku_plans_new_models_and_downloads_nothing()
    {
        var setup = new Setup();
        setup.Imported["old-site:catalog/a1.jpg"] = 501;

        var result = await setup.Run(true,
            Old(1, ["758", "759"], ("catalog/a1.jpg", true), ("catalog/a2.jpg", false)),
            Old(2, ["РМ-01"], new OldSiteNewModel("Халат «РМ»", "Одяг ЛІТО", "Літо"), ("catalog/r.jpg", true)),
            Old(3, ["999"], ("catalog/x.jpg", true)));

        var report = result.Value;
        Assert.Multiple(() =>
        {
            Assert.That(report.Found, Is.EqualTo(1));
            Assert.That(report.ModelsToCreate, Is.EqualTo(1));
            Assert.That(report.NotFound, Is.EqualTo(1));
            Assert.That(report.FilesToDownload, Is.EqualTo(3), "a1 is already in the library");
            Assert.That(report.Lines[0].ProductId, Is.EqualTo(7));
            Assert.That(report.Lines[0].PhotosToAdd, Is.EqualTo(2), "a1 is in the library but not on the product yet");
        });
        setup.Source.Verify(x => x.DownloadAsync(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(),
            It.IsAny<CancellationToken>()), Times.Never);
        setup.Sender.Verify(x => x.Send(It.IsAny<CreateCatalogProductCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Real_run_creates_a_switched_off_model_downloads_photos_and_makes_the_old_main_photo_main()
    {
        var setup = new Setup();

        var result = await setup.Run(false,
            Old(2, ["РМ-01"], new OldSiteNewModel("Халат «РМ»", "Одяг ЛІТО", "Літо"),
                ("catalog/r2.jpg", false), ("catalog/r1.jpg", true)));

        var line = result.Value.Lines.Single();
        Assert.Multiple(() =>
        {
            Assert.That(line.Status, Is.EqualTo(ImportOldSitePhotosHandler.Created));
            Assert.That(line.ProductId, Is.EqualTo(300));
            Assert.That(line.PhotosAdded, Is.EqualTo(2));
            Assert.That(setup.Created!.IsActive, Is.False);
            Assert.That(setup.Created.SectionId, Is.EqualTo(4));
            Assert.That(setup.Saved!.MediaFileIds, Is.EqualTo(new[] { 1001, 1002 }), "main first, then the others");
            Assert.That(setup.Saved.MainMediaFileId, Is.EqualTo(1001));
            Assert.That(setup.Folders.First(), Is.EqualTo(new[] { "Старий сайт" }));
        });
    }

    [Test]
    public async Task Product_with_own_photos_keeps_them_first_and_keeps_its_main_photo()
    {
        var setup = new Setup();
        setup.ProductPhotos[7] = [40];
        setup.Imported["old-site:catalog/a1.jpg"] = 501;

        var result = await setup.Run(false, Old(1, ["758"], ("catalog/a1.jpg", true), ("catalog/bad.jpg", false)));

        var line = result.Value.Lines.Single();
        Assert.Multiple(() =>
        {
            Assert.That(setup.Saved!.MediaFileIds, Is.EqualTo(new[] { 40, 501 }));
            Assert.That(setup.Saved.MainMediaFileId, Is.Null);
            Assert.That(line.PhotosAdded, Is.EqualTo(1));
            Assert.That(line.Errors.Single(), Does.StartWith("catalog/bad.jpg"));
        });
    }

    [Test]
    public async Task Repeat_adds_nothing_when_every_photo_is_already_on_the_product()
    {
        var setup = new Setup();
        setup.ProductPhotos[7] = [501];
        setup.Imported["old-site:catalog/a1.jpg"] = 501;

        var result = await setup.Run(false, Old(1, ["758"], ("catalog/a1.jpg", true)));

        Assert.That(result.Value.Lines.Single().PhotosToAdd, Is.Zero);
        setup.Sender.Verify(x => x.Send(It.IsAny<SetProductPhotosCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void Real_run_takes_at_most_twenty_products_and_only_https()
    {
        var validator = new ImportOldSitePhotosValidator();
        var many = Enumerable.Range(1, 21).Select(x => Old(x, ["1"])).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(validator.Validate(new ImportOldSitePhotosCommand(false, BaseUrl, many, "u")).IsValid, Is.False);
            Assert.That(validator.Validate(new ImportOldSitePhotosCommand(true, BaseUrl, many, "u")).IsValid, Is.True);
            Assert.That(validator.Validate(new ImportOldSitePhotosCommand(true, "http://x/", many, "u")).IsValid, Is.False);
        });
    }

    private static OldSiteProduct Old(int id, string[] skus, params (string Path, bool Main)[] photos)
        => Old(id, skus, null, photos);

    private static OldSiteProduct Old(int id, string[] skus, OldSiteNewModel? model, params (string Path, bool Main)[] photos)
        => new(id, $"Old {id}", skus, photos.Select(x => new OldSitePhoto(x.Path, x.Main)).ToList(), model);

    private sealed class Setup
    {
        private int _nextFile = 1001;

        public Setup()
        {
            Sender.Setup(x => x.Send(It.IsAny<GetProductSkuIndexQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success<IReadOnlyList<ProductSkuIndexItem>>(
                    [new ProductSkuIndexItem(7, "Sewing", "Костюм «Агренто»", true, ["758", "759"])]));
            Sender.Setup(x => x.Send(It.IsAny<CreateCatalogProductCommand>(), It.IsAny<CancellationToken>()))
                .Callback<ICommand<Result<CreatedCatalogProduct>>, CancellationToken>((command, _) => Created = (CreateCatalogProductCommand)command)
                .ReturnsAsync(Result.Success(new CreatedCatalogProduct(300, "Sewing", "MOD-300", true)));
            Sender.Setup(x => x.Send(It.IsAny<SetProductPhotosCommand>(), It.IsAny<CancellationToken>()))
                .Callback<ICommand<Result<ProductPhotoList>>, CancellationToken>((command, _) => Saved = (SetProductPhotosCommand)command)
                .ReturnsAsync(Result.Success(new ProductPhotoList(1, [])));
            Sections.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([new ProductSectionItem(4, "Sewing", "Одяг ЛІТО", true), new ProductSectionItem(5, "Ppe", "Одяг ЛІТО", true)]);
            Source.Setup(x => x.FindImportedAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => Imported);
            Source.Setup(x => x.DownloadAsync(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Uri url, string _, IReadOnlyList<string> folder, CancellationToken _) =>
                {
                    Folders.Add(folder);
                    return url.AbsolutePath.EndsWith("bad.jpg", StringComparison.Ordinal)
                        ? new ProductPhotoDownload(null, "сайт відповів 404")
                        : new ProductPhotoDownload(_nextFile++, null);
                });
            Products.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ISpecification<Product> spec, CancellationToken _) =>
                {
                    var id = ProductPhotos.Keys.FirstOrDefault(key => spec.Evaluate([Build(key)]).Any());
                    return id == 0 ? null : Build(id);
                });
        }

        public Mock<ISender> Sender { get; } = new();
        public Mock<IProductSectionReader> Sections { get; } = new();
        public Mock<IProductPhotoSource> Source { get; } = new();
        public Mock<ICatalogReadRepository<Product>> Products { get; } = new();
        public Dictionary<string, int> Imported { get; } = [];
        public Dictionary<int, int[]> ProductPhotos { get; } = [];
        public List<IReadOnlyList<string>> Folders { get; } = [];
        public CreateCatalogProductCommand? Created { get; private set; }
        public SetProductPhotosCommand? Saved { get; private set; }

        public async Task<Result<OldSitePhotoImportReport>> Run(bool dryRun, params OldSiteProduct[] products)
            => await new ImportOldSitePhotosHandler(Sender.Object, Products.Object, Sections.Object, Source.Object)
                .Handle(new ImportOldSitePhotosCommand(dryRun, BaseUrl, products, "user-1"), CancellationToken.None);

        private Product Build(int id)
        {
            var now = DateTimeOffset.UtcNow;
            var product = Product.Create(ProductId.Create(id), "P", "P", ProductSlug.Create($"p-{id}").Value, ProductType.Sewing, now).Value;
            foreach (var (file, index) in ProductPhotos[id].Select((file, index) => (file, index)))
                product.AddPhoto(ProductPhotoId.Create(index + 1), MediaFileId.Create(file), now, sortOrder: index);
            return product;
        }
    }
}
