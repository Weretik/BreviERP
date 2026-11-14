using Catalog.Domain.Products;
using Catalog.Application.Features.Product.Queries.GetProductOverviewPage;
using Catalog.Domain.Products.Enums;
using Catalog.Application.Features.Product.Queries.GetProductOverviewPage.Results;

namespace UnitTests.Catalog.Products;

/// <summary>Search, filters, sort and page of the «Товари» list on the server (CAT-013).</summary>
public sealed class ProductOverviewListingTests
{
    private static readonly ProductOverviewItem[] Items =
    [
        Item(1, "MOD-10", "Халат", 1, 300m, "Габардин"),
        Item(2, "MOD-2", "Куртка", 1, 500m, "Бязь"),
        Item(3, "MOD-3", "Брюки", null, null, "Саржа"),
        Item(4, null, "Без коду", 2, 100m, null)
    ];

    [Test]
    public void Code_sort_is_natural_and_rows_without_a_code_come_last()
        => Assert.That(Sorted(ProductOverviewSort.Code, false), Is.EqualTo(new[] { 2, 3, 1, 4 }));

    [Test]
    public void Price_sort_keeps_rows_without_a_price_last_in_both_directions()
    {
        Assert.That(Sorted(ProductOverviewSort.Price, false), Is.EqualTo(new[] { 4, 1, 2, 3 }));
        Assert.That(Sorted(ProductOverviewSort.Price, true), Is.EqualTo(new[] { 2, 1, 4, 3 }));
    }

    [Test]
    public void Search_finds_code_name_variant_sku_and_fabric_name_ignoring_case()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Filtered(Query() with { Search = "mod-2" }), Is.EqualTo(new[] { 2 }));
            Assert.That(Filtered(Query() with { Search = "халат" }), Is.EqualTo(new[] { 1 }));
            Assert.That(Filtered(Query() with { Search = "sku-3" }), Is.EqualTo(new[] { 3 }));
            Assert.That(Filtered(Query() with { Search = "БЯЗЬ" }), Is.EqualTo(new[] { 2 }));
        });
    }

    [Test]
    public void Section_without_section_and_without_price_filters()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Filtered(Query() with { SectionId = 1 }), Is.EquivalentTo(new[] { 1, 2 }));
            Assert.That(Filtered(Query() with { WithoutSection = true }), Is.EqualTo(new[] { 3 }));
            Assert.That(Filtered(Query() with { WithoutPrice = true }), Is.EqualTo(new[] { 3 }));
            Assert.That(ProductOverviewListing.CountSections(Items), Is.EqualTo(new[]
            {
                new ProductSectionCount(1, 2), new ProductSectionCount(2, 1), new ProductSectionCount(null, 1)
            }));
        });
    }

    [Test]
    public void Page_size_and_page_are_validated()
    {
        var validator = new GetProductOverviewPageValidator();
        Assert.Multiple(() =>
        {
            Assert.That(validator.Validate(Query()).IsValid, Is.True);
            Assert.That(validator.Validate(Query() with { PageSize = 101 }).IsValid, Is.False);
            Assert.That(validator.Validate(Query() with { Page = 0 }).IsValid, Is.False);
            Assert.That(validator.Validate(Query() with { Search = new string('а', 201) }).IsValid, Is.False);
        });
    }

    private static GetProductOverviewPageQuery Query()
        => new(ProductType.Sewing, null, null, false, false, ProductOverviewSort.Code, false, 1, 25);

    private static int[] Sorted(ProductOverviewSort sort, bool descending)
        => ProductOverviewListing.Sort(Items, sort, descending).Select(x => x.Id).ToArray();

    [Test]
    public void Active_filter_shows_only_active_or_only_switched_off_rows()
    {
        var items = Items.Select(x => x.Id == 3 ? x with { IsActive = false } : x).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(ProductOverviewListing.Filter(items, Query() with { Active = false }).Select(x => x.Id), Is.EqualTo(new[] { 3 }));
            Assert.That(ProductOverviewListing.Filter(items, Query() with { Active = true }).Select(x => x.Id), Is.EqualTo(new[] { 1, 2, 4 }));
            Assert.That(ProductOverviewListing.Filter(items, Query()).Count(), Is.EqualTo(4));
        });
    }

    private static int[] Filtered(GetProductOverviewPageQuery query)
        => ProductOverviewListing.Filter(Items, query).Select(x => x.Id).ToArray();

    private static ProductOverviewItem Item(int id, string? code, string name, int? section, decimal? retail, string? fabric)
    {
        var prices = retail is { } value ? new ProductPriceTiers(value, value - 10, value - 20) : null;
        return new ProductOverviewItem(id, "Sewing", code, name, section, null, true, null, null,
            DateTimeOffset.UnixEpoch.AddDays(id), prices, null, [],
            [new ProductOverviewVariant(id * 10, $"SKU-{id}", true, fabric is null ? null : id, fabric, prices, null, [])]);
    }
}
