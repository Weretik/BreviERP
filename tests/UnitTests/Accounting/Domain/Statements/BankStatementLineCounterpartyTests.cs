using Accounting.Domain.Counterparties;
using Accounting.Domain.Statements;

namespace UnitTests.Accounting.Domain.Statements;

/// <summary>«Провести виписку»: the new counterparty of a party nobody was recognised for (Domain rule).</summary>
public sealed class BankStatementLineCounterpartyTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 14, 9, 0, 0, TimeSpan.Zero);

    [TestCase("ТОВ Ромашка", "12345678", "UA00", "code:12345678")]
    [TestCase("ТОВ Ромашка", "", "UA00", "iban:UA00")]
    [TestCase("ТОВ Ромашка", "", "", "name:тов ромашка")]
    public void Party_key_prefers_edrpou_then_iban_then_name(string bank, string edrpou, string iban, string key) =>
        Assert.That(Line(bank, edrpou, iban).PartyKey, Is.EqualTo(key));

    [TestCase("ТОВ Ромашка", "12345678", "ТОВ Ромашка")]
    [TestCase("", "12345678", "ЄДРПОУ 12345678")]
    [TestCase("", "", "IBAN UA00")]
    public void New_counterparty_is_named_after_bank_name_then_codes(string bank, string edrpou, string name)
    {
        var made = Line(bank, edrpou, "UA00").NewCounterparty(FinanceCounterpartyId.New(), "april.xlsx", Now);

        Assert.That(made.IsSuccess, Is.True);
        Assert.That(made.Value.Name, Is.EqualTo(name));
        Assert.That(made.Value.Iban, Is.EqualTo("UA00"));
    }

    [Test]
    public void A_line_without_any_party_needs_no_counterparty()
    {
        Assert.That(Line("", "", "").NeedsNewCounterparty, Is.False);
        Assert.That(Line("ТОВ Ромашка", "", "").NeedsNewCounterparty, Is.True);
    }

    private static BankStatementLine Line(string bank, string edrpou, string iban) =>
        BankStatementLine.Create(BankStatementId.New(), new BankLineData(1, new DateOnly(2026, 11, 1), null, null, 1000,
            "UAH", bank, edrpou, iban, "Оплата", "", null, null, "", "{}"));
}
