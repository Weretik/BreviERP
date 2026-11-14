using Catalog.Domain.Products;
using Catalog.Domain.Products.Productivity;

namespace UnitTests.Catalog.Products;

public class ProductDomainTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 11, 10, 0, 0, TimeSpan.Zero);

    [Test]
    public void Sewing_details_reject_more_than_two_primary_fabrics()
    {
        var action = () => SewingProductDetails.Create(ProductId.Create(1), 1m,
            [ProductFabric.Create(ProductId.Create(1), 1, true, 0).Value, ProductFabric.Create(ProductId.Create(1), 2, true, 1).Value, ProductFabric.Create(ProductId.Create(1), 3, true, 2).Value], [], []);

        Assert.That(action().IsFailure, Is.True);
    }

    [Test]
    public void Sewing_details_reject_duplicate_operations()
    {
        var action = () => SewingProductDetails.Create(ProductId.Create(1), 1m, [], [], [ProductOperation.Create(ProductId.Create(1), 10).Value, ProductOperation.Create(ProductId.Create(1), 10).Value]);

        Assert.That(action().IsFailure, Is.True);
    }

    [Test]
    public void Sewing_details_reject_duplicate_fabrics_and_accessories()
    {
        var duplicateFabrics = () => SewingProductDetails.Create(ProductId.Create(1), 1m,
            [ProductFabric.Create(ProductId.Create(1), 10, true, 0).Value, ProductFabric.Create(ProductId.Create(1), 10, false, 1).Value], [], []);
        var duplicateAccessories = () => SewingProductDetails.Create(ProductId.Create(1), 1m, [],
            [ProductAccessory.Create(ProductId.Create(1), 20, 1m, 0).Value, ProductAccessory.Create(ProductId.Create(1), 20, 2m, 1).Value], []);

        Assert.Multiple(() =>
        {
            Assert.That(duplicateFabrics().IsFailure, Is.True);
            Assert.That(duplicateAccessories().IsFailure, Is.True);
        });
    }

    [Test]
    public void Pieces_per_shift_keeps_full_fractional_value()
    {
        var result = SewingProductivity.CalculatePiecesPerShift([15m]);

        Assert.That(result, Is.EqualTo(25.6m));
    }

    [Test]
    public void Pieces_per_shift_is_null_for_empty_or_zero_operations()
    {
        Assert.That(SewingProductivity.CalculatePiecesPerShift([]), Is.Null);
        Assert.That(SewingProductivity.CalculatePiecesPerShift([0m]), Is.Null);
    }

    [Test]
    public void Price_percent_has_exactly_one_source()
    {
        var reference = RetailPricePercent.FromReference(5).Value;
        var custom = WholesalePricePercent.FromCustom(33m).Value;

        Assert.Multiple(() =>
        {
            Assert.That(reference.AdditionalReferenceId, Is.EqualTo(5));
            Assert.That(reference.CustomPercent, Is.Null);
            Assert.That(custom.AdditionalReferenceId, Is.Null);
            Assert.That(custom.CustomPercent, Is.EqualTo(33m));
        });
    }

    [Test]
    public void Ppe_details_require_positive_base_price()
    {
        var action = () => PpeProductDetails.Create(ProductId.Create(1), 1, 0m,
            RetailPricePercent.FromReference(1).Value, WholesalePricePercent.FromCustom(10m).Value);

        Assert.That(action().IsFailure, Is.True);
    }

    [Test]
    public void Ppe_product_rejects_sewing_details_and_clears_ppe_details_after_type_change()
    {
        var product = Product.Create(ProductId.Create(1), "Каска", "Каска", ProductSlug.Create("kaska").Value, ProductType.Ppe, Now).Value;
        var sewing = SewingProductDetails.Create(ProductId.Create(1), 1m, [], [], []).Value;
        var ppe = PpeProductDetails.Create(ProductId.Create(1), 1, 100m, RetailPricePercent.FromCustom(10m).Value, WholesalePricePercent.FromReference(2).Value).Value;

        Assert.That(product.ConfigureSewing(sewing, Now).IsFailure, Is.True);

        product.ConfigurePpe(ppe, Now);
        product.Update("Каска", "Каска", ProductSlug.Create("kaska").Value, ProductType.Sewing, Now);

        Assert.That(product.PpeDetails, Is.Null);
    }

    [Test]
    public void Sewing_product_rejects_ppe_details()
    {
        var product = Product.Create(ProductId.Create(1), "Куртка", "Куртка", ProductSlug.Create("kurtka").Value, ProductType.Sewing, Now).Value;
        var ppe = PpeProductDetails.Create(ProductId.Create(1), 1, 100m,
            RetailPricePercent.FromReference(1).Value, WholesalePricePercent.FromCustom(10m).Value).Value;

        Assert.That(product.ConfigurePpe(ppe, Now).IsFailure, Is.True);
    }

    [Test]
    public void Characteristic_table_owns_rows_created_after_table_identity_is_assigned()
    {
        var table = ProductCharacteristicTable.Create(ProductId.Create(1), "Матеріали", "Материалы", 0).Value;

        table.AddRow("Тканина", "Ткань", "Бавовна", "Хлопок", 0);

        Assert.That(table.Rows.Single().TableId, Is.EqualTo(table.Id));
    }
}
