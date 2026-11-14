using System.Text;
using Crm.Application.Features.Quotes.Commands.SendQuote;
using Crm.Application.Features.Quotes.Commands.StartMailboxConnect;
using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Features.Quotes.Shared.Sending;
using Crm.Application.Features.Quotes.Shared.Sending.Gmail;
using Crm.Application.Abstractions.Quotes;
using Crm.Infrastructure.Integrations.Gmail;
using Crm.Application.Abstractions.Quotes.Sending;

namespace UnitTests.Crm.Quotes.Sending;

public sealed class QuoteSendingTests
{
    private static readonly QuoteActor Manager = new(17, false);

    private static SendQuoteCommand Command(string to, string subject = "КП № 1-11-2026", string body = "Добрий день") =>
        new(Manager, 4, new SendQuoteInput(3, to, subject, body));

    [Test]
    public void RecipientsAreSplitByCommasSemicolonsAndSpaces() =>
        Assert.That(QuoteRecipients.Parse(" a@agro.test; b@agro.test,A@agro.test  c@agro.test "),
            Is.EqualTo(new[] { "a@agro.test", "b@agro.test", "c@agro.test" }));

    [Test]
    public void SendNeedsValidAddressesAOneLineSubjectAndText()
    {
        var validator = new SendQuoteValidator();
        Assert.That(validator.Validate(Command("client@agro.test, boss@agro.test")).IsValid, Is.True);
        Assert.That(validator.Validate(Command("client@agro")).IsValid, Is.False);
        Assert.That(validator.Validate(Command("")).IsValid, Is.False);
        Assert.That(validator.Validate(Command(string.Join(",", Enumerable.Range(0, 11).Select(x => $"c{x}@a.test"))))
            .IsValid, Is.False);
        Assert.That(validator.Validate(Command("client@agro.test", "КП\r\nBcc: x@evil.test")).IsValid, Is.False);
        Assert.That(validator.Validate(Command("client@agro.test", body: " ")).IsValid, Is.False);
    }

    [Test]
    public void ConnectReturnsOnlyToAdminCrmPages()
    {
        var validator = new StartMailboxConnectValidator();
        Assert.That(validator.Validate(new StartMailboxConnectCommand(Manager, 17, "/crm/quotes/4")).IsValid, Is.True);
        Assert.That(validator.Validate(new StartMailboxConnectCommand(Manager, 17, "https://evil.test/crm/")).IsValid,
            Is.False);
        Assert.That(validator.Validate(new StartMailboxConnectCommand(Manager, 17, "/crm//evil.test")).IsValid, Is.False);
    }

    [Test]
    public void LetterHasEncodedHeadersTheTextAndThePdf()
    {
        var letter = new GmailLetter("olena@brevi.test", "Олена Петренко", ["client@agro.test", "boss@agro.test"],
            "Комерційна пропозиція Brevi № 1-11-2026", "Добрий день!\nНадсилаю КП.",
            new GmailAttachment("КП 1-11-2026.pdf", "application/pdf", "%PDF-1.7"u8.ToArray()));

        var mime = Encoding.ASCII.GetString(GmailMimeMessage.Build(letter, "b1"));

        Assert.That(mime, Does.StartWith("From: =?UTF-8?B?"));
        Assert.That(mime, Does.Contain(" <olena@brevi.test>\r\nTo: client@agro.test, boss@agro.test\r\n"));
        Assert.That(mime, Does.Contain("Content-Type: multipart/mixed; boundary=\"b1\""));
        Assert.That(mime, Does.Contain("filename*=UTF-8''%D0%9A%D0%9F%201-11-2026.pdf"));
        Assert.That(mime, Does.Contain(Convert.ToBase64String("%PDF-1.7"u8.ToArray())));
        Assert.That(mime, Does.Contain(Convert.ToBase64String(Encoding.UTF8.GetBytes("Добрий день!\r\nНадсилаю КП."))));
        Assert.That(mime, Does.EndWith("--b1--\r\n"));
        Assert.That(mime.Split("\r\n").All(line => line.Length <= 998), Is.True);
    }

    [Test]
    public void LongSubjectIsFoldedIntoShortEncodedWords()
    {
        var encoded = GmailMimeMessage.EncodeWord(new string('Ї', 60));
        var words = encoded.Split("\r\n ");
        Assert.That(words, Has.Length.GreaterThan(1));
        Assert.That(words.All(x => x.Length <= 75), Is.True);
        var decoded = string.Concat(words.Select(x => Encoding.UTF8.GetString(Convert.FromBase64String(x[10..^2]))));
        Assert.That(decoded, Is.EqualTo(new string('Ї', 60)));
        Assert.That(GmailMimeMessage.EncodeWord("Brevi"), Is.EqualTo("Brevi"));
    }

    [Test]
    public void IdTokenGivesOnlyAVerifiedAddress()
    {
        static string Token(string json) =>
            $"h.{Convert.ToBase64String(Encoding.UTF8.GetBytes(json)).TrimEnd('=').Replace('+', '-').Replace('/', '_')}.s";

        Assert.That(GmailClient.IdTokenEmail(Token("""{"email":"olena@gmail.com","email_verified":true}""")),
            Is.EqualTo("olena@gmail.com"));
        Assert.That(GmailClient.IdTokenEmail(Token("""{"email":"olena@gmail.com","email_verified":false}""")), Is.Null);
        Assert.That(GmailClient.IdTokenEmail("not-a-token"), Is.Null);
    }

    [Test]
    public void ConsentAsksOnlyForSendingAndOfflineAccess()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new GmailOptions
        {
            ClientId = "client.apps.googleusercontent.com",
            ClientSecret = "secret",
            RedirectUri = "https://api.brevi.test/api/crm/mailboxes/google-callback",
        });
        var client = new GmailClient(new HttpClient(), options,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<GmailClient>.Instance);

        var url = client.ConsentUrl("signed state", "olena@brevi.test").AbsoluteUri;

        Assert.That(client.IsConfigured, Is.True);
        Assert.That(url, Does.StartWith("https://accounts.google.com/o/oauth2/v2/auth?"));
        Assert.That(url, Does.Contain("scope=openid%20email%20https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fgmail.send"));
        Assert.That(url, Does.Contain("access_type=offline"));
        Assert.That(url, Does.Contain("state=signed%20state"));
        Assert.That(url, Does.Contain("login_hint=olena%40brevi.test"));
        Assert.That(url, Does.Not.Contain("secret"));
    }
}
