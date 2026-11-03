using BuildingBlocks.Domain.Exceptions;
using Catalog.Application.Contracts.Pricing;
using Catalog.Application.Features.Pricing.Sewing.ArticleSummary;
using Catalog.Application.Features.Product.Sewing;
using Catalog.Domain.Pricing.Sewing;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;
using Moq;

namespace UnitTests;

/// <summary>Sewing product card: live Article prices and line comments (CAT-007).</summary>
public sealed class SewingArticleSummaryTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 3, 10, 0, 0, TimeSpan.Zero);

    private sealed class FixedTime : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private static GetSewingArticleSummaryQueryHandler Handler(SewingArticleList? list,
        IReadOnlyList<SewingPublishedProductPrices>? published = null)
    {
        var composition = new Mock<ISewingCompositionReadStore>();
        composition.Setup(x => x.ReadArticlesAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(list);
        var inputs = new Mock<ISewingPricingInputReader>();
        inputs.Setup(x => x.ReadAsync(7, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int productId, long articleId, CancellationToken _) =>
                new SewingArticlePricingSnapshot(productId, articleId, $"SKU-{articleId}", 3, articleId != 12, [], []));
        var settings = new Mock<ISewingPricingSettingsReader>();
        settings.Setup(x => x.ReadCandidatesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var references = new Mock<IPricingReferenceVersionReader>();
        references.Setup(x => x.ReadAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<DateTimeOffset>(),
            It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var prices = new Mock<ISewingPublishedPriceReader>();
        prices.Setup(x => x.ReadAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(published ?? []);
        return new GetSewingArticleSummaryQueryHandler(composition.Object, inputs.Object, settings.Object,
            references.Object, prices.Object, new FixedTime());
    }

    [Test]
    public async Task Every_article_gets_its_blockers_and_the_last_published_prices()
    {
        var tiers = new SewingPriceTiers(500m, 450m, 420m);
        var handler = Handler(
            new SewingArticleList(7, "MOD-025", 3,
                [new SewingArticleView(11, 7, "602", true, 0, 4, 1.4m), new SewingArticleView(12, 7, "603", false, 1, 5, 1.4m)]),
            [new SewingPublishedProductPrices(7, 2,
                [new SewingPublishedArticlePrice(11, "602", Guid.NewGuid(), Guid.NewGuid(), Now.AddDays(-1), tiers)])]);

        var result = (await handler.Handle(new GetSewingArticleSummaryQuery(7), CancellationToken.None)).Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.ModelCode, Is.EqualTo("MOD-025"));
            Assert.That(result.CalculatedAt, Is.EqualTo(Now));
            Assert.That(result.Articles.Select(x => x.SaleSku), Is.EqualTo(new[] { "602", "603" }));
            Assert.That(result.Articles.All(x => !x.IsCalculated && x.Prices is null), Is.True);
            Assert.That(result.Articles[0].Blockers.Select(x => x.Code), Is.EqualTo(new[] { "MissingSettings" }));
            Assert.That(result.Articles[1].Blockers.Select(x => x.Code),
                Is.EqualTo(new[] { "InactiveArticle", "MissingSettings" }));
            Assert.That(result.Articles[0].Published?.Prices, Is.EqualTo(tiers));
            Assert.That(result.Articles[1].Published, Is.Null);
        });
    }

    [Test]
    public async Task Unknown_or_not_sewing_product_is_not_found()
    {
        var result = await Handler(null).Handle(new GetSewingArticleSummaryQuery(7), CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo(Ardalis.Result.ResultStatus.NotFound));
    }

    [Test]
    public void Line_comment_is_trimmed_and_limited()
    {
        var productId = ProductId.Create(7);
        var operation = SewingOperationLine.Create(CompositionLineId.Create(1), productId, null,
            OperationReferenceId.Create(3), 1, null, 0, comment: "  Комір  ");
        var empty = SewingMaterialLine.Create(CompositionLineId.Create(2), productId, null, SewingMaterialKind.Fabric,
            FabricReferenceId.Create(4), null, null, 1.4m, SewingMaterialUnit.Metre, 0, comment: " ");

        Assert.Multiple(() =>
        {
            Assert.That(operation.Comment, Is.EqualTo("Комір"));
            Assert.That(empty.Comment, Is.Null);
            Assert.Throws<DomainException>(() => SewingOperationLine.Create(CompositionLineId.Create(1), productId,
                null, OperationReferenceId.Create(3), 1, null, 0, comment: new string('я', 501)));
        });
    }
}
