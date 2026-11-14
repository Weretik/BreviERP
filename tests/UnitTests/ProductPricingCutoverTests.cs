using Catalog.Domain.Products;
using Catalog.Application.Abstractions.Reference;
using Catalog.Application.Features.Product.Queries.GetAdminProductDetail;
using Catalog.Application.Features.Product.Shared;

namespace UnitTests;

public sealed class ProductPricingCutoverTests
{
    [Test]
    public void Metadata_mapping_does_not_calculate_legacy_ppe_sale_prices()
    {
        var references = new ProductReferenceData(
            new Dictionary<int, ReferenceItem> { [1] = new(1, "Supplier") },
            new Dictionary<int, PricedReferenceItem>(), new Dictionary<int, PricedReferenceItem>(),
            new Dictionary<int, OperationReferenceItem>(), new Dictionary<int, AdditionalReferenceItem>());
        var input = new PpeReadModel(1, 100, PricePercentSource.Custom, null, 50, PricePercentSource.Custom, null, 36);
        var product = new ProductAdminDetailReadModel(42, "PPE", "PPE RU", "ppe",
            Catalog.Domain.Products.ProductType.Ppe, "", "", [], [], [], [], null, input, DateTimeOffset.UtcNow, null);
        var mapped = ProductAdminDetailMapper.Map(product, references,
            new Dictionary<int, CategoryAdminDetail>(), new Dictionary<int, string>()).Ppe!;
        Assert.That(mapped.BasePrice, Is.EqualTo(100));
        Assert.That(mapped.RetailPrice, Is.Zero, "Deprecated metadata fields must not invoke another price engine.");
        Assert.That(mapped.WholesalePrice, Is.Zero);
    }
}
