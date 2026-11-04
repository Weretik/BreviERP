using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.CreateQuote;
using Crm.Application.Quotes.GetQuotes;
using Crm.Domain.Quotes;

namespace UnitTests.Crm.Quotes;

public sealed class QuoteRulesTests
{
    private static readonly QuoteActor Manager = new(17, false);

    private static SaveQuoteInput Input(params QuoteItemInput[] items) =>
        new(0, "ТОВ «Агро»", null, null, null, null, null, items);

    private static QuoteOptionInput Option(long? articleId = null, decimal? manual = null) =>
        new(null, articleId, false, new QuotePriceInput(true, manual), new QuotePriceInput(true, null),
            new QuotePriceInput(true, null), null);

    [Test]
    public void NumberIsSequenceMonthYear() =>
        Assert.That(new QuoteNumber(2026, 9, 37).ToString(), Is.EqualTo("37-9-2026"));

    [Test]
    public void ManualPriceCountsOnlyWhenItDiffersFromTheCatalog()
    {
        Assert.That(new QuotePrice(100, 100, true).IsManual, Is.False);
        Assert.That(new QuotePrice(100, 90, true).Amount, Is.EqualTo(90m));
        Assert.That(new QuotePrice(100, null, false).IsVisiblePositive, Is.False);
    }

    [Test]
    public void SewingPriceCannotBeTypedByHand()
    {
        var command = new CreateQuoteCommand(Manager,
            Input(new QuoteItemInput(null, "Sewing", 10, null, null, null, null, [Option(100, manual: 500)])));
        Assert.That(new CreateQuoteCommandValidator().Validate(command).IsValid, Is.False);
    }

    [Test]
    public void OneOffPpeNeedsANameAndNoProduct()
    {
        var noName = new CreateQuoteCommand(Manager,
            Input(new QuoteItemInput(null, "OneOffPpe", null, " ", null, null, null, [Option(manual: 100)])));
        var withProduct = new CreateQuoteCommand(Manager,
            Input(new QuoteItemInput(null, "OneOffPpe", 5, "Каска", null, null, null, [Option(manual: 100)])));
        Assert.That(new CreateQuoteCommandValidator().Validate(noName).IsValid, Is.False);
        Assert.That(new CreateQuoteCommandValidator().Validate(withProduct).IsValid, Is.False);
    }

    [Test]
    public void PriceOutsideLimitsIsRejected()
    {
        var command = new CreateQuoteCommand(Manager,
            Input(new QuoteItemInput(null, "OneOffPpe", null, "Каска", null, null, null, [Option(manual: -1)])));
        Assert.That(new CreateQuoteCommandValidator().Validate(command).IsValid, Is.False);
    }

    [Test]
    public void UnknownKindAndMissingNameAreRejected()
    {
        var command = new CreateQuoteCommand(Manager,
            new SaveQuoteInput(0, "", null, null, null, null, null,
                [new QuoteItemInput(null, "Gift", null, null, null, null, null, [Option()])]));
        var errors = new CreateQuoteCommandValidator().Validate(command).Errors.Select(x => x.PropertyName).ToArray();
        Assert.That(errors, Does.Contain("Input.InternalName"));
        Assert.That(errors.Any(x => x.EndsWith(".Kind", StringComparison.Ordinal)), Is.True);
    }

    [Test]
    public void ValidCreateCommandPasses()
    {
        var command = new CreateQuoteCommand(Manager,
            Input(new QuoteItemInput(null, "Sewing", 10, null, null, null, null, [Option(100)])));
        Assert.That(new CreateQuoteCommandValidator().Validate(command).IsValid, Is.True);
    }

    [Test]
    public void ListPageIsLimited() =>
        Assert.That(new GetQuotesQueryValidator().Validate(new GetQuotesQuery(Manager, null, null, null, 1, 101)).IsValid,
            Is.False);

    [Test]
    public void ManagerAccessesOnlyOwnQuote()
    {
        var quote = new Quote(new QuoteNumber(2026, 11, 1), QuoteKind.Request, new QuotePerson(5, "Інший"),
            new QuoteHeader("Клієнт", null, null, null, null, null), [], DateTimeOffset.UnixEpoch);
        Assert.That(Manager.CanAccess(quote), Is.False);
        Assert.That(new QuoteActor(1, true).CanAccess(quote), Is.True);
        Assert.That(new QuoteActor(5, false).CanAccess(quote), Is.True);
    }
}
