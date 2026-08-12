using Catalog.Application.Contracts.Admin;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;

namespace UnitTests;

public class ProductApplicationReadModelTests
{
    [Test]
    public void Admin_detail_sorts_primary_fabrics_before_additional_fabrics()
    {
        var id = ProductId.Create(101);
        var product = Product.Create(id, "Куртка", "Куртка", ProductSlug.Create("kurtka"), ProductType.Sewing, DateTimeOffset.UtcNow);
        product.SetDescriptions("Опис", "Описание", DateTimeOffset.UtcNow);
        product.ConfigureSewing(SewingProductDetails.Create(id, 1m,
            [new ProductFabric(id, 3, false, 0), new ProductFabric(id, 2, true, 3), new ProductFabric(id, 1, true, 2)], [], []), DateTimeOffset.UtcNow);

        var detail = ProductAdminDetailMapper.Map(product);

        Assert.That(detail.Sewing!.Fabrics.Select(x => x.FabricId), Is.EqualTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public void Admin_detail_keeps_ppe_percent_sources_mutually_exclusive()
    {
        var id = ProductId.Create(102);
        var product = Product.Create(id, "Каска", "Каска", ProductSlug.Create("kaska"), ProductType.Ppe, DateTimeOffset.UtcNow);
        product.SetDescriptions("Опис", "Описание", DateTimeOffset.UtcNow);
        product.ConfigurePpe(PpeProductDetails.Create(id, 1, 100m,
            RetailPricePercent.FromReference(10), WholesalePricePercent.FromCustom(15m)), DateTimeOffset.UtcNow);

        var detail = ProductAdminDetailMapper.Map(product);

        Assert.Multiple(() =>
        {
            Assert.That(detail.Ppe!.RetailReferenceId, Is.EqualTo(10));
            Assert.That(detail.Ppe.RetailCustomPercent, Is.Null);
            Assert.That(detail.Ppe.WholesaleReferenceId, Is.Null);
            Assert.That(detail.Ppe.WholesaleCustomPercent, Is.EqualTo(15m));
        });
    }
}
