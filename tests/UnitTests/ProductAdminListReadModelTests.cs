using Ardalis.Specification;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Product.GetAdminList;
using Catalog.Application.Features.Product.GetAdminList.DTOs;
using Catalog.Application.Features.Product.Shared.DTOs;
using Catalog.Domain.Media.Entities;
using Catalog.Domain.ProductCategories.Entities;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.Enums;
using Moq;

namespace UnitTests;

public sealed class ProductAdminListReadModelTests
{
    [Test]
    public async Task Admin_list_preserves_metadata_and_main_photo_without_calculating_sale_prices()
    {
        var products = new Mock<ICatalogReadRepository<Product>>();
        var categories = new Mock<ICatalogReadRepository<ProductCategory>>();
        var media = new Mock<ICatalogReadRepository<MediaFile>>();
        products.Setup(x => x.CountAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>())).ReturnsAsync(2);
        products.Setup(x => x.ListAsync(It.IsAny<ISpecification<Product, ProductListItemReadModel>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new(1, "Sewing", "sewing", ProductType.Sewing, [], null, DateTimeOffset.UtcNow, null),
                new(2, "PPE", "ppe", ProductType.Ppe, [], 501, DateTimeOffset.UtcNow, null)]);
        media.Setup(x => x.ListAsync(It.IsAny<ISpecification<MediaFile, ProductMediaUrl>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new ProductMediaUrl(501, "https://cdn.example.test/501.jpg")]);
        var result = await new GetAdminProductsQueryHandler(products.Object, categories.Object, media.Object)
            .Handle(new GetAdminProductsQuery(), CancellationToken.None);
        Assert.That(result.Value.Select(x => x.MinimumWholesalePrice), Is.All.Zero);
        Assert.That(result.Value.Single(x => x.Id == 2).MainPhoto!.Url, Is.EqualTo("https://cdn.example.test/501.jpg"));
        Assert.That(result.PagedInfo.TotalRecords, Is.EqualTo(2));
    }

    [Test]
    public async Task Empty_admin_list_skips_media_enrichment()
    {
        var products = new Mock<ICatalogReadRepository<Product>>();
        var categories = new Mock<ICatalogReadRepository<ProductCategory>>();
        var media = new Mock<ICatalogReadRepository<MediaFile>>();
        products.Setup(x => x.CountAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>())).ReturnsAsync(0);
        products.Setup(x => x.ListAsync(It.IsAny<ISpecification<Product, ProductListItemReadModel>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var result = await new GetAdminProductsQueryHandler(products.Object, categories.Object, media.Object)
            .Handle(new GetAdminProductsQuery(), CancellationToken.None);
        Assert.That(result.Value, Is.Empty);
        media.VerifyNoOtherCalls();
    }
}
