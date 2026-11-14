using Crm.Domain.Quotes;
using Crm.Application.Features.Quotes.Commands.CreateQuote;
using Crm.Application.Features.Quotes.Queries.GetQuotes;
using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Domain.Quotes.Enums;
using Crm.Domain.Quotes.Items;
using Crm.Domain.Quotes.ValueObjects;

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
        Assert.That(new CreateQuoteValidator().Validate(command).IsValid, Is.False);
    }

    [Test]
    public void OneOffPpeNeedsANameAndNoProduct()
    {
        var noName = new CreateQuoteCommand(Manager,
            Input(new QuoteItemInput(null, "OneOffPpe", null, " ", null, null, null, [Option(manual: 100)])));
        var withProduct = new CreateQuoteCommand(Manager,
            Input(new QuoteItemInput(null, "OneOffPpe", 5, "Каска", null, null, null, [Option(manual: 100)])));
        Assert.That(new CreateQuoteValidator().Validate(noName).IsValid, Is.False);
        Assert.That(new CreateQuoteValidator().Validate(withProduct).IsValid, Is.False);
    }

    [Test]
    public void PriceOutsideLimitsIsRejected()
    {
        var command = new CreateQuoteCommand(Manager,
            Input(new QuoteItemInput(null, "OneOffPpe", null, "Каска", null, null, null, [Option(manual: -1)])));
        Assert.That(new CreateQuoteValidator().Validate(command).IsValid, Is.False);
    }

    [Test]
    public void UnknownKindAndMissingNameAreRejected()
    {
        var command = new CreateQuoteCommand(Manager,
            new SaveQuoteInput(0, "", null, null, null, null, null,
                [new QuoteItemInput(null, "Gift", null, null, null, null, null, [Option()])]));
        var errors = new CreateQuoteValidator().Validate(command).Errors.Select(x => x.PropertyName).ToArray();
        Assert.That(errors, Does.Contain("Input.InternalName"));
        Assert.That(errors.Any(x => x.EndsWith(".Kind", StringComparison.Ordinal)), Is.True);
    }

    [Test]
    public void ValidCreateCommandPasses()
    {
        var command = new CreateQuoteCommand(Manager,
            Input(new QuoteItemInput(null, "Sewing", 10, null, null, null, null, [Option(100)])));
        Assert.That(new CreateQuoteValidator().Validate(command).IsValid, Is.True);
    }

    [Test]
    public void ListPageIsLimited() =>
        Assert.That(new GetQuotesValidator().Validate(new GetQuotesQuery(Manager, null, null, null, 1, 101)).IsValid,
            Is.False);

    [Test]
    public void ClientFilterNeedsAPositiveId() =>
        Assert.That(new GetQuotesValidator().Validate(new GetQuotesQuery(Manager, null, null, null, CustomerId: 0))
            .IsValid, Is.False);

    [Test]
    public void ManagerAccessesOnlyOwnQuote()
    {
        var quote = Quote.Create(new QuoteNumber(2026, 11, 1), QuoteKind.Request, new QuotePerson(5, "Інший"),
            new QuoteHeader("Клієнт", null, null, null, null, null), [], DateTimeOffset.UnixEpoch).Value;
        Assert.That(Manager.CanAccess(quote), Is.False);
        Assert.That(new QuoteActor(1, true).CanAccess(quote), Is.True);
        Assert.That(new QuoteActor(5, false).CanAccess(quote), Is.True);
    }

    [Test]
    public void CopyKeepsManualPricesAndCompositionButNotTheStatus()
    {
        var option = new QuoteItemOption(0, null, 5, "Саржа", null, true, new QuotePrice(100m, 120m, true),
            QuotePrice.Empty, QuotePrice.Empty, DateTimeOffset.UnixEpoch).WithFabrics([new QuoteItemOptionFabric(0, 5, "Саржа", 1.5m)]);
        option.SetManualAudit("Знижка", new QuotePerson(5, "Олена"), DateTimeOffset.UnixEpoch);
        var item = new QuoteItem(0, QuoteItemKind.CustomSewing, null, new QuoteItemTexts(null, "Фартух", null, null,
            null, null, null, null), [option], [new QuoteItemLine(0, QuoteItemLineKind.Operation, 155, "Шов", "хв", 1m, 12m)]);
        var source = Quote.Create(new QuoteNumber(2026, 11, 1), QuoteKind.Request, new QuotePerson(5, "Олена"),
            new QuoteHeader("Клієнт", 7, "Іван", "Агро", null, null), [item], DateTimeOffset.UnixEpoch).Value;
        source.MarkSent(DateTimeOffset.UnixEpoch);

        var copy = source.CopyAs(new QuoteNumber(2026, 11, 2), new QuotePerson(1, "Директор"), DateTimeOffset.UnixEpoch);

        var copied = copy.Items.Single();
        Assert.Multiple(() =>
        {
            Assert.That(copy.Status, Is.EqualTo(QuoteStatus.Draft));
            Assert.That(copy.Number, Is.EqualTo("2-11-2026"));
            Assert.That(copy.ResponsibleName, Is.EqualTo("Директор"));
            Assert.That(copy.CustomerId, Is.EqualTo(7));
            Assert.That(copied, Is.Not.SameAs(item));
            Assert.That(copied.Options.Single().Retail.Amount, Is.EqualTo(120m));
            Assert.That(copied.Options.Single().ManualReason, Is.EqualTo("Знижка"));
            Assert.That(copied.Options.Single().FabricMeters, Is.EqualTo(1.5m));
            Assert.That(copied.Lines.Single().ManualMinutes, Is.EqualTo(12m));
            Assert.That(copy.Events.Single().Details, Is.EqualTo("Копія КП 1-11-2026"));
        });
    }
}
