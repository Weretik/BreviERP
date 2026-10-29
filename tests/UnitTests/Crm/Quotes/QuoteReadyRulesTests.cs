using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Quotes;

namespace UnitTests.Crm.Quotes;

public sealed class QuoteReadyRulesTests
{
    [Test]
    public void Ready_pins_current_version_and_keeps_number_when_a_later_edit_is_reissued()
    {
        var quote = Quote.Create(null, null, "Buyer", new CrmUserId(17));
        quote.SetInitialVersion(1);

        quote.PrepareReady(1, "10/10/2026", 17, false);
        Assert.Multiple(() =>
        {
            Assert.That(quote.State, Is.EqualTo("ready"));
            Assert.That(quote.Number, Is.EqualTo("10/10/2026"));
            Assert.That(quote.IssuedVersionId, Is.EqualTo(1));
            Assert.That(quote.Revision, Is.EqualTo(2));
            Assert.That(quote.History.Single().Action, Is.EqualTo("readied"));
        });

        quote.AppendVersion(2, 2, 17);
        quote.PrepareReady(3, "10/10/2026", 17, false);
        Assert.That(quote.IssuedVersionId, Is.EqualTo(2));
        Assert.That(quote.Number, Is.EqualTo("10/10/2026"));
    }

    [Test]
    public void Ready_rejects_stale_revision_and_different_existing_number_without_partial_mutation()
    {
        var quote = Quote.Create(null, null, "Buyer", new CrmUserId(17));
        quote.SetInitialVersion(1);
        Assert.Throws<QuoteRevisionException>(() => quote.PrepareReady(2, "10/10/2026", 17, false));
        Assert.That(quote.State, Is.EqualTo("draft"));
        quote.PrepareReady(1, "10/10/2026", 17, false);
        quote.AppendVersion(2, 2, 17);
        Assert.Throws<QuoteValidationException>(() => quote.PrepareReady(3, "11/10/2026", 17, false));
        Assert.That(quote.State, Is.EqualTo("draft"));
        Assert.That(quote.IssuedVersionId, Is.EqualTo(1));
    }
}
