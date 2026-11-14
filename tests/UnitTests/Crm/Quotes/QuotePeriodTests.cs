using Crm.Application.Features.Quotes.Queries.GetQuotePeriod;
using Crm.Application.Features.Quotes.Shared.Access;

namespace UnitTests.Crm.Quotes;

/// <summary>«КП за місяць» of the director's «Робочий стіл» (DESK-001 stage 5).</summary>
public sealed class QuotePeriodTests
{
    [Test]
    public void Counts_created_and_sent_quotes_in_total_and_per_manager()
    {
        var from = new DateOnly(2026, 11, 1);
        var to = new DateOnly(2026, 11, 11);
        var period = QuotePeriodCounter.Count(from, to,
        [
            new QuotePeriodRow(2, "Марина", Created: true, Sent: true),
            new QuotePeriodRow(2, "Марина", Created: true, Sent: false),
            new QuotePeriodRow(3, "Олег", Created: false, Sent: true),
        ]);

        Assert.Multiple(() =>
        {
            Assert.That((period.From, period.To), Is.EqualTo((from, to)));
            Assert.That(period.Created, Is.EqualTo(2));
            Assert.That(period.Sent, Is.EqualTo(2));
            Assert.That(period.Managers.Select(x => (x.Manager.Name, x.Created, x.Sent)),
                Is.EqualTo(new[] { ("Марина", 2, 1), ("Олег", 0, 1) }));
        });
    }

    [Test]
    public void A_period_ending_before_it_starts_is_refused()
    {
        var validator = new GetQuotePeriodValidator();
        var actor = new QuoteActor(1, CanSeeAll: true);

        Assert.Multiple(() =>
        {
            Assert.That(validator.Validate(new GetQuotePeriodQuery(actor, null, null)).IsValid, Is.True);
            Assert.That(validator.Validate(
                new GetQuotePeriodQuery(actor, new DateOnly(2026, 11, 2), new DateOnly(2026, 11, 1))).IsValid, Is.False);
        });
    }
}
