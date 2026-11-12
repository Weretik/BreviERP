using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Settings.Contracts;
using Crm.Application.Quotes.Settings.SaveSettings;
using Crm.Application.Quotes.Staff;
using Crm.Domain.Quotes.Settings;

namespace UnitTests.Crm.Quotes;

[TestFixture]
public sealed class QuoteSettingsTests
{
    private static SaveQuoteSettingsInput DefaultInput()
    {
        var response = ((QuoteSettings?)null).ToResponse([new QuoteStaffMember(5, "Юлія", "julia@brevi.test", "+380")],
            new Dictionary<int, string>());
        return new SaveQuoteSettingsInput(response.Version, response.Company, response.Email, response.Pdf,
            response.Introduction, response.LetterVariants,
            response.StaffContacts.Select(x => new QuoteStaffContactInput(x.UserId, x.Name, x.Phone, x.Email)).ToArray());
    }

    private static bool Valid(SaveQuoteSettingsInput input) =>
        new SaveQuoteSettingsCommandValidator().Validate(new SaveQuoteSettingsCommand(new QuoteActor(1, true), input)).IsValid;

    [Test]
    public void DefaultsAreValidAndTakeTheAccountContact()
    {
        var input = DefaultInput();
        Assert.That(Valid(input), Is.True);
        Assert.That(input.StaffContacts.Single(), Is.EqualTo(new QuoteStaffContactInput(5, "Юлія", "+380", "julia@brevi.test")));
        Assert.That(input.Company.Edrpou, Is.EqualTo("35521636"));
        Assert.That(input.Introduction.Advantages, Has.Count.EqualTo(5));
    }

    [Test]
    public void RejectsBrokenRequisitesAndLinks()
    {
        var input = DefaultInput();
        Assert.That(Valid(input with { Company = input.Company with { Edrpou = "123" } }), Is.False);
        Assert.That(Valid(input with { Company = input.Company with { LogoUrl = "/brand/logo.svg" } }), Is.False);
        Assert.That(Valid(input with { Company = input.Company with { Name = " " } }), Is.False);
        Assert.That(Valid(input with { Introduction = input.Introduction with { AccentColor = "orange" } }), Is.False);
    }

    [Test]
    public void NeedsFiveAdvantagesAndOneContactPerPerson()
    {
        var input = DefaultInput();
        Assert.That(Valid(input with { Introduction = input.Introduction with { Advantages = input.Introduction.Advantages.Take(4).ToArray() } }),
            Is.False);
        Assert.That(Valid(input with { StaffContacts = [.. input.StaffContacts, .. input.StaffContacts] }), Is.False);
    }

    [Test]
    public void SavedContactWinsOverTheAccount()
    {
        var settings = new QuoteSettings(DefaultInput().ToContent() with
        {
            StaffContacts = [new QuoteStaffContact(5, "Олена Петренко", "+38 050 000 00 00", "office@brevi.test")],
        }, "Директор", DateTimeOffset.UnixEpoch);

        var response = settings.ToResponse([new QuoteStaffMember(5, "Юлія", "julia@brevi.test"), new QuoteStaffMember(6, "Вадим", "v@brevi.test", "+381")],
            new Dictionary<int, string> { [5] = "julia.brevi@gmail.com" });

        Assert.That(settings.Version, Is.EqualTo(1));
        Assert.That(response.StaffContacts[0].Email, Is.EqualTo("office@brevi.test"));
        Assert.That(response.StaffContacts[0].AccountEmail, Is.EqualTo("julia@brevi.test"));
        Assert.That(response.StaffContacts[1].Phone, Is.EqualTo("+381"));
        Assert.That(response.StaffContacts.Select(x => x.MailboxEmail), Is.EqualTo(new[] { "julia.brevi@gmail.com", null }));
    }
}
