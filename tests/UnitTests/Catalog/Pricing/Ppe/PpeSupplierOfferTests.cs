using Ardalis.Result;
using Ardalis.Specification;
using Catalog.Domain.Pricing.Ppe;
using Catalog.Domain.Products;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Catalog.Application.Abstractions;
using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Reference;
using Catalog.Application.Features.Pricing.Commands.UpdatePpeSupplierOffer;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.Ppe;
using Catalog.Domain.Products.ValueObjects;
using Catalog.Application.Abstractions.Pricing.Ppe;
using Catalog.Application.Abstractions.Pricing.Ppe.Versions;
using Catalog.Application.Abstractions.Reference.Items;

namespace UnitTests.Catalog.Pricing.Ppe;

/// <summary>PPE card supplier fields and the offer version they create (CAT-008).</summary>
public sealed class PpeSupplierOfferTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 3, 15, 0, 0, TimeSpan.Zero);
    private static readonly Guid CurrentVersion = Guid.Parse("8b0a4f43-7f0e-4c6a-9f1e-2a2f0c9d1e11");

    [Test]
    public void Supplier_fields_go_to_the_profile_and_keep_the_quote_description()
    {
        var product = Create(ProductType.Ppe);
        product.SetOverview(new ProductOverviewDetails(3, null, true, null, "Опис для КП"), Now);

        product.SetPpeSupplier(Details(price: 518.64m), Now);

        var profile = product.CatalogProfile!;
        Assert.Multiple(() =>
        {
            Assert.That(profile.SupplierId?.Value, Is.EqualTo(5));
            Assert.That(profile.SupplierSku, Is.EqualTo("CARINA"));
            Assert.That(profile.SupplierPrice, Is.EqualTo(518.64m));
            Assert.That(profile.PricingRuleCode, Is.EqualTo("Одяг ЗІЗ — АВ-Центр"));
            Assert.That(profile.Unit, Is.EqualTo("шт."));
            Assert.That(profile.ShortDescription, Is.EqualTo("Опис для КП"));
        });
    }

    [Test]
    public void Sewing_product_and_bad_supplier_link_are_rejected()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Create(ProductType.Sewing).SetPpeSupplier(Details(), Now).IsFailure, Is.True);
            Assert.That(Create(ProductType.Ppe).SetPpeSupplier(Details(url: "avcentr.com.ua"), Now).IsFailure, Is.True);
        });
    }

    [TestCase(0, null)]
    [TestCase(5, "ftp://avcentr.com.ua/x")]
    public void Validator_rejects_missing_supplier_and_non_web_link(int supplierId, string? url)
    {
        var result = new UpdatePpeSupplierOfferValidator().Validate(Command(supplierId: supplierId, url: url));
        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void Validator_rejects_more_than_two_decimals()
        => Assert.That(new UpdatePpeSupplierOfferValidator().Validate(Command(price: 1.005m)).IsValid, Is.False);

    [Test]
    public async Task New_supplier_price_appends_an_approved_offer_version_with_the_insight_prices()
    {
        var setup = new Setup(Offer(400m));
        PpeOfferValues? written = null;
        setup.Offers.Setup(x => x.AppendAsync(101, CurrentVersion, It.IsAny<Guid>(), It.IsAny<PpeOfferValues>(),
                "manager", It.IsAny<CancellationToken>()))
            .Callback<int, Guid?, Guid, PpeOfferValues, string, CancellationToken>((_, _, _, values, _, _) => written = values)
            .ReturnsAsync(Result.Success(Offer(518.64m)));

        var result = await setup.Handler.Handle(Command(price: 518.64m), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True, string.Join(" ", result.Errors));
        Assert.Multiple(() =>
        {
            Assert.That(written!.SupplierCost, Is.EqualTo(518.64m));
            Assert.That(written.RuleCode, Is.EqualTo("Одяг ЗІЗ — АВ-Центр"));
            Assert.That(written.ExternalWholesale, Is.EqualTo(700m));
            Assert.That(written.Source, Is.EqualTo(UpdatePpeSupplierOfferHandler.Source));
            Assert.That(written.SourceDate, Is.EqualTo("03.11.2026"));
        });
        setup.Products.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Same_price_and_rule_only_save_the_product()
    {
        var setup = new Setup(Offer(518.64m));

        var result = await setup.Handler.Handle(Command(price: 518.64m, sku: "CARINA-2"), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        setup.Products.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        setup.Offers.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Stale_card_and_unknown_supplier_are_refused()
    {
        var setup = new Setup(Offer(518.64m));

        var stale = await setup.Handler.Handle(Command(expected: Guid.NewGuid()), CancellationToken.None);
        var unknown = await setup.Handler.Handle(Command(supplierId: 99), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(stale.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(unknown.Status, Is.EqualTo(ResultStatus.Invalid));
        });
    }

    private sealed class Setup
    {
        public Mock<ICatalogRepository<Product>> Products { get; } = new();
        public Mock<IPpeOfferVersionWriter> Offers { get; } = new();
        public UpdatePpeSupplierOfferHandler Handler { get; }

        public Setup(PpeOfferVersion offer)
        {
            var product = Create(ProductType.Ppe);
            Products.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);
            var references = new Mock<IProductReferenceReader>();
            references.Setup(x => x.GetSnapshotAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new ProductReferenceData(
                new Dictionary<int, ReferenceItem> { [5] = new(5, "АВ-Центр") },
                new Dictionary<int, PricedReferenceItem>(), new Dictionary<int, PricedReferenceItem>(),
                new Dictionary<int, OperationReferenceItem>(), new Dictionary<int, AdditionalReferenceItem>()));
            var rule = new PpeRuleVersion(Guid.NewGuid(), new PpeRuleValues("Одяг ЗІЗ — АВ-Центр",
                new PpeMarkups(0.5m, 0.3m, 0.2m), "pricing-parameters", "03.11.2026"), true, "x", Now, Now);
            var inputs = new Mock<IPpePricingInputReader>();
            inputs.Setup(x => x.ReadRuleAsync("Одяг ЗІЗ — АВ-Центр", It.IsAny<CancellationToken>())).ReturnsAsync(rule);
            inputs.Setup(x => x.ReadAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([new PpePricingProductInput(101, true, offer, rule)]);
            Handler = new UpdatePpeSupplierOfferHandler(Products.Object, references.Object, inputs.Object,
                Offers.Object, new FakeTimeProvider(Now));
        }
    }

    private static PpeOfferVersion Offer(decimal cost)
        => new(CurrentVersion, 101, new PpeOfferValues("Одяг ЗІЗ — АВ-Центр", cost, null, 700m, "sheet", "09.09.2026"),
            true, "x", Now, Now);

    private static UpdatePpeSupplierOfferCommand Command(
        int supplierId = 5, string? url = "https://avcentr.com.ua/ua/kostyum-carina", decimal? price = 518.64m,
        string? sku = "CARINA", Guid? expected = null)
        => new(101, supplierId, sku, url, price, false, "Одяг ЗІЗ — АВ-Центр", "шт.", expected ?? CurrentVersion, "manager");

    private static PpeSupplierDetails Details(decimal? price = 518.64m, string? url = "https://avcentr.com.ua/ua/kostyum-carina")
        => new(SupplierReferenceId.Create(5), "CARINA", url, price, false, "Одяг ЗІЗ — АВ-Центр", "шт.");

    private static Product Create(ProductType type)
        => Product.Create(ProductId.Create(101), "Костюм ВОЛОГОЗАХИСНИЙ", "Костюм ВОЛОГОЗАХИСНИЙ RU",
            ProductSlug.Create("kostyum-vologozahysnyj").Value, type, Now).Value;
}
