using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Validation;

namespace UnitTests.Crm.Quotes;

[TestFixture]
public sealed class QuoteEmbroideryTests
{
    private static QuoteItemInput Item(long? id, decimal? manual = null) =>
        new(id, "Embroidery", null, null, null, null, null,
            [new QuoteOptionInput(5, null, false, new(true, manual), new(true, null), new(true, null), null)]);

    private static bool Valid(QuoteItemInput item) => new QuoteItemInputValidator().Validate(item).IsValid;

    [Test]
    public void EditorKeepsOnlySavedEmbroideryWithoutTypedPrices()
    {
        Assert.That(Valid(Item(42)), Is.True);
        Assert.That(Valid(Item(null)), Is.False, "New embroidery comes only from «Додати в КП».");
        Assert.That(Valid(Item(42, 99m)), Is.False, "The price comes from the saved calculation.");
        Assert.That(Valid(Item(42) with { ProductId = 10 }), Is.False);
    }
}
