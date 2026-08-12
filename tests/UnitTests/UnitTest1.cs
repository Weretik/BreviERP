using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.Services;
using Catalog.Domain.Products.ValueObjects;

namespace UnitTests;

public class ProductDomainTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 11, 10, 0, 0, TimeSpan.Zero);

    [Test]
    public void Sewing_details_reject_more_than_two_primary_fabrics()
    {
        var action = () => SewingProductDetails.Create(ProductId.Create(1), 1m,
            [new ProductFabric(ProductId.Create(1), 1, true, 0), new ProductFabric(ProductId.Create(1), 2, true, 1), new ProductFabric(ProductId.Create(1), 3, true, 2)], [], []);

        Assert.That(action, Throws.TypeOf<DomainException>());
    }

    [Test]
    public void Sewing_details_reject_duplicate_operations()
    {
        var action = () => SewingProductDetails.Create(ProductId.Create(1), 1m, [], [], [new ProductOperation(ProductId.Create(1), 10), new ProductOperation(ProductId.Create(1), 10)]);

        Assert.That(action, Throws.TypeOf<DomainException>());
    }

    [Test]
    public void Pieces_per_shift_keeps_full_fractional_value()
    {
        var result = SewingCalculation.CalculatePiecesPerShift([15m]);

        Assert.That(result, Is.EqualTo(25.6m));
    }

    [Test]
    public void Pieces_per_shift_is_null_for_empty_or_zero_operations()
    {
        Assert.That(SewingCalculation.CalculatePiecesPerShift([]), Is.Null);
        Assert.That(SewingCalculation.CalculatePiecesPerShift([0m]), Is.Null);
    }

    [Test]
    public void Price_percent_has_exactly_one_source()
    {
        var reference = RetailPricePercent.FromReference(5);
        var custom = WholesalePricePercent.FromCustom(33m);

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
            RetailPricePercent.FromReference(1), WholesalePricePercent.FromCustom(10m));

        Assert.That(action, Throws.TypeOf<DomainException>());
    }

    [Test]
    public void Ppe_product_rejects_sewing_details_and_clears_ppe_details_after_type_change()
    {
        var product = Product.Create(ProductId.Create(1), "Каска", "Каска", ProductSlug.Create("kaska"), ProductType.Ppe, Now);
        var sewing = SewingProductDetails.Create(ProductId.Create(1), 1m, [], [], []);
        var ppe = PpeProductDetails.Create(ProductId.Create(1), 1, 100m, RetailPricePercent.FromCustom(10m), WholesalePricePercent.FromReference(2));

        Assert.That(() => product.ConfigureSewing(sewing, Now), Throws.TypeOf<DomainException>());

        product.ConfigurePpe(ppe, Now);
        product.Update("Каска", "Каска", ProductSlug.Create("kaska"), ProductType.Sewing, Now);

        Assert.That(product.PpeDetails, Is.Null);
    }

    [Test]
    public void Characteristic_table_owns_rows_created_after_table_identity_is_assigned()
    {
        var table = new ProductCharacteristicTable(ProductId.Create(1), "Матеріали", "Материалы", 0, []);

        table.AddRow("Тканина", "Ткань", "Бавовна", "Хлопок", 0);

        Assert.That(table.Rows.Single().TableId, Is.EqualTo(table.Id));
    }
}
