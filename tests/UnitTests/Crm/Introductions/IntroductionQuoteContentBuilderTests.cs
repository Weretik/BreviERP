using Crm.Application.Quotes.BuildIntroduction;
using Crm.Application.Quotes.Introductions;
using Crm.Application.Quotes.Pricing;
using Crm.Domain.Quotes.Introductions;

namespace UnitTests.Crm.Introductions;

public sealed class IntroductionQuoteContentBuilderTests
{
    [Test]
    public void Uses_exact_server_price_basis_for_each_season_slot()
    {
        var (draft, previews) = Fixture();
        var content = IntroductionQuoteContentBuilder.Build(draft, previews);
        Assert.That(content.Items, Has.Count.EqualTo(4));
        for (var index = 0; index < content.Items.Count; index++)
        {
            var item = content.Items[index];
            Assert.That(item.SortOrder, Is.EqualTo(index));
            Assert.That(item.Variants, Has.Count.EqualTo(1));
            Assert.That(item.Variants[0].BasisToken, Is.EqualTo(previews[index].BasisToken));
            Assert.That(item.Variants[0].TierVisibility.LargeWholesale, Is.True);
            Assert.That(item.Variants[0].TierVisibility.Retail, Is.False);
            Assert.That(item.Variants[0].TierVisibility.Wholesale, Is.False);
        }
    }

    [Test]
    public void Changed_price_or_source_blocks_pinning_a_different_public_price()
    {
        var (draft, previews) = Fixture();
        var changedPrice = previews.ToArray();
        changedPrice[2] = changedPrice[2] with { LargeWholesale = 999m };
        Assert.That(() => IntroductionQuoteContentBuilder.Build(draft, changedPrice),
            Throws.InvalidOperationException.With.Message.EqualTo("introduction_price_or_selection_changed"));

        var changedSource = previews.ToArray();
        changedSource[1] = changedSource[1] with
        {
            Source = changedSource[1].Source! with { SourceId = Guid.NewGuid() }
        };
        Assert.That(() => IntroductionQuoteContentBuilder.Build(draft, changedSource),
            Throws.InvalidOperationException.With.Message.EqualTo("introduction_price_or_selection_changed"));
    }

    private static (IntroductionPreparedDraft Draft, QuotePricePreview[] Previews) Fixture()
    {
        var slots = IntroductionSlot.Required(IntroductionLayout.Summer)
            .Select((slot, index) =>
            {
                var product = 101 + index;
                var article = 201L + index;
                var source = new QuoteCatalogPrice("sewing", product, article, Guid.NewGuid(),
                    null, null, null, null, [], "v1", 300m, 250m, 200m, []);
                return new IntroductionPreparedSlot(new IntroductionSelection(slot, product, article),
                    $"Модель {index}", "Тканина", "Бавовна", "220 г/м²",
                    "https://brevi.com.ua/product/test", 200m, source);
            }).ToArray();
        var draft = new IntroductionPreparedDraft(IntroductionLayout.Summer, Guid.NewGuid(),
            Guid.NewGuid(), slots);
        var previews = slots.Select(x => new QuotePricePreview(Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddMinutes(15),
            new QuotePriceIntent("sewing", x.Selection.ProductId, x.Selection.ArticleId,
                null, null), x.CurrentPrice, 300m, 250m, 200m, [], "UAH",
            "catalog_no_extra_vat")).ToArray();
        return (draft, previews);
    }
}
