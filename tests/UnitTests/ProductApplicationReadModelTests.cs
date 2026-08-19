using Catalog.Application.Contracts.Admin;
using Catalog.Application.Contracts.Admin.Product;
using Catalog.Application.Contracts.Admin.Ppe;
using Catalog.Application.Contracts.Admin.Reference;
using Catalog.Application.Contracts.Admin.Sewing;
using Catalog.Application.Contracts.Reference;
using Catalog.Application.Features.Product.GetAdminDetail.DTOs;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;

namespace UnitTests;

public class ProductApplicationReadModelTests
{
    [Test]
    public void Admin_detail_sorts_primary_fabrics_before_additional_fabrics()
    {
        var detail = ProductAdminDetailMapper.Map(SewingReadModel(), SewingReferences(), Categories());

        Assert.Multiple(() =>
        {
            Assert.That(detail.Categories.Single(), Is.EqualTo(new CategoryAdminDetail(9, "Одяг", "Одежда", "odyah")));
            Assert.That(detail.Sewing!.Fabrics.Select(x => x.FabricId), Is.EqualTo(new[] { 1, 2, 3 }));
            Assert.That(detail.Sewing.Fabrics.Single(x => x.FabricId == 1).Name, Is.EqualTo("Тканина 1"));
            Assert.That(detail.Sewing.Accessories.Single(), Is.EqualTo(new AccessoryAdminDetail(4, "Блискавка", 25m, 2m, 0)));
            Assert.That(detail.Sewing.Operations.Single(), Is.EqualTo(new OperationAdminDetail(5, "Пошиття", 12m)));
            Assert.That(detail.Sewing.Prices!.Ranges.Price1To10.MinFabricId, Is.EqualTo(1));
            Assert.That(detail.Sewing.Prices.Ranges.Price1To10.MaxFabricId, Is.EqualTo(3));
        });
    }

    [Test]
    public void Admin_detail_keeps_ppe_percent_sources_mutually_exclusive()
    {
        var references = new ProductReferenceData(
            new Dictionary<int, ReferenceItem> { [1] = new(1, "Постачальник") },
            new Dictionary<int, PricedReferenceItem>(),
            new Dictionary<int, PricedReferenceItem>(),
            new Dictionary<int, OperationReferenceItem>(),
            new Dictionary<int, AdditionalReferenceItem> { [10] = new(10, "Роздріб", "retail", 12m, "%") });
        var detail = ProductAdminDetailMapper.Map(PpeReadModel(), references, new Dictionary<int, CategoryAdminDetail>());

        Assert.Multiple(() =>
        {
            Assert.That(detail.Ppe!.Supplier, Is.EqualTo(new NamedReferenceAdminDetail(1, "Постачальник")));
            Assert.That(detail.Ppe.RetailPercent.Reference, Is.EqualTo(new AdditionalReferenceAdminDetail(10, "Роздріб", "retail", 12m, "%")));
            Assert.That(detail.Ppe.RetailPercent.CustomPercent, Is.Null);
            Assert.That(detail.Ppe.WholesalePercent.Reference, Is.Null);
            Assert.That(detail.Ppe.WholesalePercent.CustomPercent, Is.EqualTo(15m));
        });
    }

    private static IReadOnlyDictionary<int, CategoryAdminDetail> Categories() => new Dictionary<int, CategoryAdminDetail>
    {
        [9] = new(9, "Одяг", "Одежда", "odyah")
    };

    private static ProductAdminDetailReadModel SewingReadModel() => new(
        101, "Куртка", "Куртка", "kurtka", ProductType.Sewing, "Опис", "Описание", [9], [], [], [],
        new SewingReadModel(1m,
            [new FabricReadModel(3, false, 0), new FabricReadModel(2, true, 3), new FabricReadModel(1, true, 2)],
            [new AccessoryReadModel(4, 2m, 0)], [5]),
        null, DateTimeOffset.UtcNow, null);

    private static ProductAdminDetailReadModel PpeReadModel() => new(
        102, "Каска", "Каска", "kaska", ProductType.Ppe, "Опис", "Описание", [], [], [], [], null,
        new PpeReadModel(1, 100m, PricePercentSource.Reference, 10, null, PricePercentSource.Custom, null, 15m),
        DateTimeOffset.UtcNow, null);

    private static ProductReferenceData SewingReferences()
    {
        var additional = new Dictionary<int, AdditionalReferenceItem>();
        foreach (var (key, value) in new Dictionary<string, decimal>
                 {
                     ["sr_zp_shvei"] = 100m, ["work_day"] = 20m, ["coefficient_seamstress_award"] = 1m,
                     ["coefficient_factor"] = 1m, ["coefficient_master"] = 1m, ["coefficient_foreman"] = 1m,
                     ["monthly_expenses"] = 100m, ["count_shvei"] = 1m, ["profit_10"] = 30m,
                     ["profit_10_40"] = 20m, ["profit_40"] = 10m
                 })
            additional[additional.Count + 1] = new AdditionalReferenceItem(additional.Count + 1, key, key, value, "");

        return new ProductReferenceData(
            new Dictionary<int, ReferenceItem>(),
            new Dictionary<int, PricedReferenceItem> { [1] = new(1, "Тканина 1", 100m), [2] = new(2, "Тканина 2", 150m), [3] = new(3, "Тканина 3", 200m) },
            new Dictionary<int, PricedReferenceItem> { [4] = new(4, "Блискавка", 25m) },
            new Dictionary<int, OperationReferenceItem> { [5] = new(5, "Пошиття", 12m) },
            additional);
    }
}
