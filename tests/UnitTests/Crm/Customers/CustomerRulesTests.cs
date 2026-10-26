using Crm.Domain.Customers;
using Crm.Domain.Customers.Contacts;
using Crm.Domain.Customers.Identifiers;

namespace UnitTests.Crm.Customers;

[TestFixture]
public sealed class CustomerRulesTests
{
    [Test]
    public void SelectingPrimaryContactDemotesPreviousPrimary()
    {
        var customer = NewCustomer();
        customer.SaveContact(1, null, "First", null, true, true, []);
        var selected = customer.SaveContact(2, null, "Second", null, true, true, []);
        Assert.That(customer.Contacts.Count(x => x.Active && x.IsPrimary), Is.EqualTo(1));
        Assert.That(selected.IsPrimary, Is.True);
    }

    [Test]
    public void DeactivatingContactClearsPrimaryAndRejectsStaleRevision()
    {
        var customer = NewCustomer();
        var contact = customer.SaveContact(1, null, "First", null, true, true, []);
        customer.SaveContact(2, contact.PublicId, "First", null, false, true, []);
        Assert.That(contact.IsPrimary, Is.False);
        Assert.Throws<CustomerRevisionException>(() => customer.Revise(2, "person", "Changed", null, null, true));
    }

    [TestCase("email", "not-mail")]
    [TestCase("phone", "abc")]
    public void InvalidChannelIsRejected(string kind, string value)
    {
        Assert.Throws<CustomerValidationException>(() => CustomerChannel.Create(kind, value));
    }

    [Test]
    public void InternationalPhoneRetainsDisplayAndNormalizesDigits()
    {
        var channel = CustomerChannel.Create("phone", "+44 (20) 1234-5678");
        Assert.That(channel.Value, Is.EqualTo("+44 (20) 1234-5678"));
        Assert.That(channel.NormalizedValue, Is.EqualTo("442012345678"));
    }

    [Test]
    public void SimilarNamesAreIndependentCustomersAndMergePreservesContactIdentity()
    {
        var survivor = NewCustomer();
        var victim = NewCustomer();
        var contact = victim.SaveContact(1, null, "Original", null, true, true, []);
        survivor.MergeFrom(victim, 1, 2);
        Assert.That(survivor.Contacts.Single().PublicId, Is.EqualTo(contact.PublicId));
        Assert.That(victim.Active, Is.False);
        Assert.That(victim.Contacts, Is.Empty);
    }

    private static Customer NewCustomer() => Customer.Create("person", "Synthetic customer", null, null, new CrmUserId(1));
}
