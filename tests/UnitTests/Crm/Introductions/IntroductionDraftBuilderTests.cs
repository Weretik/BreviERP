using Ardalis.Result;
using Crm.Application.Quotes.BuildIntroduction;
using Crm.Application.Quotes.Pricing;
using Crm.Domain.Quotes.Introductions;

namespace UnitTests.Crm.Introductions;

public sealed class IntroductionDraftBuilderTests
{
    private static readonly FeaturedModelMapping Mapping = new(Guid.NewGuid(), 101, 201);

    [Test]
    public async Task Resolves_current_approved_prices_without_storing_prices_in_template()
    {
        var snapshot = Snapshot(IntroductionLayout.Summer);
        var reader = new FakeReader((product, fabric) => Offer(product, fabric,
            IntroductionSeason.Summer, 200m));
        var result = await new IntroductionDraftBuilder(reader).BuildAsync(snapshot, Mapping, default);

        Assert.That(result.IsSuccess, Is.True, string.Join(",", result.Errors));
        Assert.That(result.Value.Slots, Has.Count.EqualTo(4));
        Assert.That(result.Value.Slots.All(x => x.LargeWholesale == 200m), Is.True);
        Assert.That(reader.Calls, Is.EqualTo(4));
    }

    [Test]
    public async Task Changed_mapping_and_missing_current_offer_block_reuse()
    {
        var snapshot = Snapshot(IntroductionLayout.Summer);
        var reader = new FakeReader((_, _) => null);
        var changed = await new IntroductionDraftBuilder(reader).BuildAsync(snapshot,
            Mapping with { VersionId = Guid.NewGuid() }, default);
        Assert.That(changed.Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(reader.Calls, Is.Zero);

        var missing = await new IntroductionDraftBuilder(reader).BuildAsync(snapshot, Mapping, default);
        Assert.That(missing.Status, Is.EqualTo(ResultStatus.Invalid));
        Assert.That(missing.ValidationErrors.Select(x => x.ErrorMessage),
            Does.Contain("selection_unavailable"));
    }

    [Test]
    public async Task Wrong_season_metadata_url_and_large_tier_are_visible_blockers()
    {
        var snapshot = Snapshot(IntroductionLayout.Summer);
        var reader = new FakeReader((product, fabric) => Offer(product, fabric,
            IntroductionSeason.Winter, 0m) with
            {
                FabricComposition = "",
                ProductUrl = "https://evil.example/coat"
            });
        var result = await new IntroductionDraftBuilder(reader).BuildAsync(snapshot, Mapping, default);
        Assert.That(result.Status, Is.EqualTo(ResultStatus.Invalid));
        var codes = result.ValidationErrors.Select(x => x.ErrorMessage).ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(codes, Does.Contain("model_unavailable_or_wrong_season"));
            Assert.That(codes, Does.Contain("fabric_metadata_missing"));
            Assert.That(codes, Does.Contain("product_url_invalid"));
            Assert.That(codes, Does.Contain("large_wholesale_price_unavailable"));
        });
    }

    [Test]
    public async Task Null_public_metadata_returns_validation_instead_of_throwing()
    {
        var snapshot = Snapshot(IntroductionLayout.Summer);
        var reader = new FakeReader((product, article) => Offer(product, article,
            IntroductionSeason.Summer, 200m) with
            {
                ModelName = null!,
                FabricName = null!,
                FabricComposition = null!,
                FabricDensity = null!
            });

        var result = await new IntroductionDraftBuilder(reader).BuildAsync(snapshot, Mapping, default);
        Assert.That(result.Status, Is.EqualTo(ResultStatus.Invalid));
        Assert.That(result.ValidationErrors.Select(x => x.ErrorMessage),
            Does.Contain("fabric_metadata_missing"));
    }

    private static IntroductionSelectionSnapshot Snapshot(IntroductionLayout layout)
        => IntroductionSelectionSnapshot.Create(layout, Mapping, Guid.NewGuid(),
            IntroductionSlot.Required(layout).Select(slot => new IntroductionSelection(slot,
                slot.Position == 0 ? Mapping.For(slot.Season) : slot.Uid + 10,
                slot.Uid + 1000)));

    private static IntroductionCatalogOffer Offer(int product, long fabric,
        IntroductionSeason season, decimal price) => new(season, "made", true, true,
        "Модель", "Тканина", "Бавовна", "220 г/м²", "https://brevi.com.ua/product/coat",
        new QuoteCatalogPrice("sewing", product, fabric, Guid.NewGuid(), null,
            Guid.NewGuid(), 1, 1, [], "v1", 300m, 250m, price, []));

    private sealed class FakeReader(Func<int, long, IntroductionCatalogOffer?> resolve)
        : IIntroductionCatalogReader
    {
        public int Calls { get; private set; }

        public Task<IntroductionCatalogOffer?> ReadAsync(int productId, long articleId,
            CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult(resolve(productId, articleId));
        }
    }
}
