using Ardalis.Specification;
using Catalog.Application.Contracts.Admin.Product;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Contracts.Reference;
using Catalog.Application.Features.Product.GetAdminDetail.DTOs;
using Catalog.Application.Features.Product.GetAdminList;
using Catalog.Application.Features.Product.GetAdminList.DTOs;
using Catalog.Application.Features.Product.Shared.DTOs;
using Catalog.Domain.Media.Entities;
using Catalog.Domain.ProductCategories.Entities;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;
using Moq;

namespace UnitTests;

public sealed class ProductAdminListReadModelTests
{
    [Test]
    public async Task Admin_list_enriches_only_main_photo_and_calculates_type_specific_minimum_price()
    {
        var productRepository = new Mock<ICatalogReadRepository<Product>>();
        var categoryRepository = new Mock<ICatalogReadRepository<ProductCategory>>();
        var mediaRepository = new Mock<ICatalogReadRepository<MediaFile>>();
        var pricingReferenceReader = new Mock<IProductListPricingReferenceReader>();
        var rows = new List<ProductListItemReadModel>
        {
            new(1, "Куртка", "kurtka", ProductType.Sewing, [], null, Sewing(), null, DateTimeOffset.UtcNow, null),
            new(2, "Каска", "kaska", ProductType.Ppe, [], 501, null, Ppe(), DateTimeOffset.UtcNow, null)
        };

        productRepository.Setup(x => x.CountAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);
        productRepository.Setup(x => x.ListAsync(
                It.IsAny<ISpecification<Product, ProductListItemReadModel>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(rows);
        mediaRepository.Setup(x => x.ListAsync(
                It.IsAny<ISpecification<MediaFile, ProductMediaUrl>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new ProductMediaUrl(501, "https://cdn.example.test/501.jpg")]);
        pricingReferenceReader.Setup(x => x.GetAsync(It.IsAny<ProductListPricingReferenceRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PricingReferences());
        var handler = new GetAdminProductsQueryHandler(
            productRepository.Object,
            categoryRepository.Object,
            mediaRepository.Object,
            pricingReferenceReader.Object);

        var result = await handler.Handle(new GetAdminProductsQuery(), CancellationToken.None);
        var sewing = result.Value.Single(x => x.Id == 1);
        var ppe = result.Value.Single(x => x.Id == 2);
        var expectedSewingMinimum = ProductAdminDetailMapper.CalculateSewingPrices(Sewing(), References())!.ByFabric
            .SelectMany(x => new[] { x.Price1To10, x.Price11To39, x.Price40Plus })
            .Min();

        Assert.Multiple(() =>
        {
            Assert.That(sewing.MainPhoto, Is.Null);
            Assert.That(sewing.MinimumWholesalePrice, Is.EqualTo(expectedSewingMinimum));
            Assert.That(ppe.MainPhoto, Is.EqualTo(new ProductMainPhoto(501, "https://cdn.example.test/501.jpg")));
            Assert.That(ppe.MinimumWholesalePrice, Is.EqualTo(115m));
        });
        pricingReferenceReader.Verify(x => x.GetAsync(
            It.Is<ProductListPricingReferenceRequest>(request =>
                request.FabricIds.Count == 1 && request.FabricIds.Contains(10) &&
                request.GarmentPartOperationIds.Count == 1 && request.GarmentPartOperationIds.Contains(20) &&
                request.AdditionalReferenceIds.Count == 1 && request.AdditionalReferenceIds.Contains(30)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Admin_list_returns_zero_when_price_cannot_be_calculated_or_main_photo_url_is_missing()
    {
        var productRepository = new Mock<ICatalogReadRepository<Product>>();
        var categoryRepository = new Mock<ICatalogReadRepository<ProductCategory>>();
        var mediaRepository = new Mock<ICatalogReadRepository<MediaFile>>();
        var pricingReferenceReader = new Mock<IProductListPricingReferenceReader>();
        productRepository.Setup(x => x.CountAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        productRepository.Setup(x => x.ListAsync(
                It.IsAny<ISpecification<Product, ProductListItemReadModel>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new ProductListItemReadModel(1, "Чернетка", "chern", ProductType.Sewing, [], 501,
                new SewingReadModel(1m, [new FabricReadModel(99, true, 0)], [], []), null, DateTimeOffset.UtcNow, null)]);
        mediaRepository.Setup(x => x.ListAsync(
                It.IsAny<ISpecification<MediaFile, ProductMediaUrl>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        pricingReferenceReader.Setup(x => x.GetAsync(It.IsAny<ProductListPricingReferenceRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductListPricingReferenceData(
                new Dictionary<int, decimal>(),
                new Dictionary<int, decimal>(),
                new Dictionary<int, decimal>(),
                new Dictionary<int, ProductListAdditionalReference>()));
        var handler = new GetAdminProductsQueryHandler(
            productRepository.Object,
            categoryRepository.Object,
            mediaRepository.Object,
            pricingReferenceReader.Object);

        var result = await handler.Handle(new GetAdminProductsQuery(), CancellationToken.None);
        var item = result.Value.Single();

        Assert.Multiple(() =>
        {
            Assert.That(item.MainPhoto, Is.Null);
            Assert.That(item.MinimumWholesalePrice, Is.Zero);
        });
        pricingReferenceReader.Verify(x => x.GetAsync(It.IsAny<ProductListPricingReferenceRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static SewingReadModel Sewing() => new(
        1m,
        [new FabricReadModel(10, true, 0)],
        [],
        [20]);

    private static PpeReadModel Ppe() => new(
        1, 100m, PricePercentSource.Reference, 30, null, PricePercentSource.Custom, null, 15m);

    private static ProductReferenceData References()
    {
        var additional = new Dictionary<int, AdditionalReferenceItem>
        {
            [30] = new(30, "Оптовий", "wholesale", 10m, "%")
        };
        foreach (var (key, value) in new Dictionary<string, decimal>
                 {
                     ["sr_zp_shvei"] = 100m, ["work_day"] = 20m, ["coefficient_seamstress_award"] = 1m,
                     ["coefficient_factor"] = 1m, ["coefficient_master"] = 1m, ["coefficient_foreman"] = 1m,
                     ["monthly_expenses"] = 100m, ["count_shvei"] = 1m, ["profit_10"] = 30m,
                     ["profit_10_40"] = 20m, ["profit_40"] = 10m
                 })
            additional[additional.Count + 100] = new AdditionalReferenceItem(additional.Count + 100, key, key, value, "");

        return new ProductReferenceData(
            new Dictionary<int, ReferenceItem> { [1] = new(1, "Постачальник") },
            new Dictionary<int, PricedReferenceItem> { [10] = new(10, "Тканина", 100m) },
            new Dictionary<int, PricedReferenceItem>(),
            new Dictionary<int, OperationReferenceItem> { [20] = new(20, "Пошиття", 10m) },
            additional);
    }

    private static ProductListPricingReferenceData PricingReferences()
    {
        var references = References();
        return new ProductListPricingReferenceData(
            references.Fabrics.ToDictionary(x => x.Key, x => x.Value.Price),
            references.GarmentAccessories.ToDictionary(x => x.Key, x => x.Value.Price),
            references.GarmentPartOperations.ToDictionary(x => x.Key, x => x.Value.Minutes),
            references.AdditionalReferences.ToDictionary(
                x => x.Key,
                x => new ProductListAdditionalReference(x.Value.Id, x.Value.Key, x.Value.Value, x.Value.Unit)));
    }
}
