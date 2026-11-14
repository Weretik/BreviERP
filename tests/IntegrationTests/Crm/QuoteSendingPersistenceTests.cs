using Ardalis.Result;
using Crm.Domain.Quotes;
using Microsoft.Extensions.Logging.Abstractions;
using Crm.Application.Features.Quotes.Commands.CompleteMailboxConnect;
using Crm.Application.Features.Quotes.Commands.SendQuote;
using Crm.Application.Features.Quotes.Commands.StartMailboxConnect;
using Crm.Application.Features.Quotes.Queries.GetQuoteSendDraft;
using Crm.Application.Features.Quotes.Queries.GetQuoteSettings;
using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Features.Quotes.Shared.Pdf.Composing;
using Crm.Application.Features.Quotes.Shared.Sending.Gmail;
using Crm.Application.Abstractions.Quotes;

namespace IntegrationTests.Crm;

/// <summary>Gmail per manager and sending a КП over the real crm schema (CRM-003 stage 3, R-15).</summary>
[NonParallelizable]
public sealed class QuoteSendingPersistenceTests : CrmDatabaseFixture
{
    private static readonly QuoteActor Director = new(1, true);
    private static readonly QuoteActor Olena = new(17, false);
    private readonly QuoteTestStaff _staff = new();
    private readonly QuoteTestClock _clock = new(new DateTimeOffset(2026, 11, 4, 10, 0, 0, TimeSpan.Zero));
    private FakeGmail _gmail = new();
    private FakeProtector _protector = new();
    private static readonly QuoteMailOptions Mail = new("https://office.brevi.test");

    /// <summary>NUnit shares the fixture instance between tests.</summary>
    [SetUp]
    public void NewFakes()
    {
        _gmail = new FakeGmail();
        _protector = new FakeProtector();
    }

    private async Task<string> StartConnect(QuoteActor actor, int userId)
    {
        await using var db = Context();
        var result = await new StartMailboxConnectHandler(_staff, _gmail, _protector)
            .Handle(new StartMailboxConnectCommand(actor, userId, "/crm/quote-settings"), default);
        Assert.That(result.IsSuccess, Is.True, string.Join(",", result.Errors));
        return _protector.LastState!;
    }

    private async Task<string> Callback(string? code, string? state, string? error = null)
    {
        await using var db = Context();
        var result = await new CompleteMailboxConnectHandler(db, _gmail, _protector, Mail, _clock,
            NullLogger<CompleteMailboxConnectHandler>.Instance).Handle(
            new CompleteMailboxConnectCommand(code, state, error), default);
        return result.Value;
    }

    private async Task<Result<QuoteResponse>> Send(QuoteActor actor, long id, int version)
    {
        await using var db = Context();
        return await new SendQuoteHandler(db, new QuotePdfComposer(db, _staff, new QuoteTestRenderer(), new QuoteTestNoCatalog()), _gmail,
                _protector, _clock, NullLogger<SendQuoteHandler>.Instance)
            .Handle(new SendQuoteCommand(actor, id, new SendQuoteInput(version, "client@agro.test; boss@agro.test",
                "КП № 1-11-2026", "Добрий день")), default);
    }

    private async Task<long> AddQuote()
    {
        await using var db = Context();
        var option = new QuoteItemOption(0, null, null, "Ціна", null, false, new QuotePrice(100m, null, true),
            new QuotePrice(90m, null, true), new QuotePrice(80m, null, true), _clock.GetUtcNow());
        var quote = Quote.Create(new QuoteNumber(2026, 11, 1), QuoteKind.Request, new QuotePerson(17, "Олена"),
            new QuoteHeader("Агро", null, null, "ТОВ «Агро»", null, null),
            [new QuoteItem(0, QuoteItemKind.OneOffPpe, null, new QuoteItemTexts(null, "Каска", null, null, null, null, null, null), [option])],
            _clock.GetUtcNow()).Value;
        db.Quotes.Add(quote);
        await db.SaveChangesAsync();
        return quote.Id;
    }

    [Test]
    public async Task DirectorConnectsAManagersGmailAndTheSettingsShowIt()
    {
        var state = await StartConnect(Director, 17);
        var back = await Callback("code-1", state);

        Assert.That(back, Is.EqualTo("https://office.brevi.test/crm/quote-settings?gmail=connected"));
        await using var db = Context();
        var mailbox = db.StaffMailboxes.Single();
        Assert.That(mailbox.UserId, Is.EqualTo(17));
        Assert.That(mailbox.Email, Is.EqualTo("olena.manager@brevi.test"));
        Assert.That(mailbox.ConnectedByName, Is.EqualTo("Директор"));
        Assert.That(mailbox.ProtectedRefreshToken, Is.EqualTo("protected:refresh-1"));

        var settings = (await new GetQuoteSettingsHandler(db, _staff).Handle(new GetQuoteSettingsQuery(), default)).Value;
        Assert.That(settings.StaffContacts.Single(x => x.UserId == 17).MailboxEmail, Is.EqualTo("olena.manager@brevi.test"));
        Assert.That(settings.StaffContacts.Single(x => x.UserId == 1).MailboxEmail, Is.Null);
    }

    [Test]
    public async Task ManagerConnectsOnlyTheirOwnGmailAndADeniedConsentStoresNothing()
    {
        await using (var db = Context())
        {
            var other = await new StartMailboxConnectHandler(_staff, _gmail, _protector)
                .Handle(new StartMailboxConnectCommand(Olena, 1, "/crm/quote-settings"), default);
            Assert.That(other.Status, Is.EqualTo(ResultStatus.Forbidden));
        }

        var state = await StartConnect(Olena, 17);
        Assert.That(await Callback(null, state, "access_denied"),
            Is.EqualTo("https://office.brevi.test/crm/quote-settings?gmail=error&reason=denied"));
        Assert.That(await Callback("code-1", "forged"),
            Is.EqualTo("https://office.brevi.test/crm/quote-settings?gmail=error&reason=expired"));
        await using var check = Context();
        Assert.That(check.StaffMailboxes.Count(), Is.Zero);
    }

    [Test]
    public async Task SendingAttachesTheSavedPdfFromTheSendersGmailAndMarksTheQuoteSent()
    {
        var id = await AddQuote();
        Assert.That((await Send(Olena, id, 1)).Status, Is.EqualTo(ResultStatus.Conflict), "no Gmail yet");

        await Callback("code-1", await StartConnect(Olena, 17));
        var sent = await Send(Olena, id, 1);

        Assert.That(sent.IsSuccess, Is.True, string.Join(",", sent.Errors));
        Assert.That(sent.Value.Status, Is.EqualTo("Sent"));
        Assert.That(sent.Value.LastSentAt, Is.EqualTo(_clock.GetUtcNow()));
        Assert.That(sent.Value.Version, Is.EqualTo(2));
        var letter = _gmail.Sent.Single();
        Assert.That(letter.Token, Is.EqualTo("refresh-1"));
        Assert.That(letter.Letter.FromEmail, Is.EqualTo("olena.manager@brevi.test"));
        Assert.That(letter.Letter.To, Is.EqualTo(new[] { "client@agro.test", "boss@agro.test" }));
        Assert.That(letter.Letter.Attachment.FileName, Is.EqualTo("КП 1-11-2026.pdf"));

        await using var db = Context();
        var log = db.QuoteSendings.Single();
        Assert.That((log.QuoteVersion, log.SenderName, log.To), Is.EqualTo((1, "Олена", "client@agro.test, boss@agro.test")));
    }

    [Test]
    public async Task AGmailFailureLeavesTheDraftAndAStaleVersionIsRejected()
    {
        var id = await AddQuote();
        await Callback("code-1", await StartConnect(Olena, 17));

        Assert.That((await Send(Olena, id, 7)).Status, Is.EqualTo(ResultStatus.Conflict));
        _gmail.Fail = "Gmail не прийняв лист. Спробуйте ще раз.";
        var failed = await Send(Olena, id, 1);
        Assert.That(failed.Errors, Is.EqualTo(new[] { "Gmail не прийняв лист. Спробуйте ще раз." }));

        await using var db = Context();
        var quote = db.Quotes.Single();
        Assert.That((quote.Status, quote.Version, quote.LastSentAt), Is.EqualTo((QuoteStatus.Draft, 1, (DateTimeOffset?)null)));
        Assert.That(db.QuoteSendings.Count(), Is.Zero);
    }

    [Test]
    public async Task SendDraftFillsTheMarksWithTheSendersContact()
    {
        var id = await AddQuote();
        await using var db = Context();
        var draft = (await new GetQuoteSendDraftHandler(db, new QuotePdfComposer(db, _staff, new QuoteTestRenderer(), new QuoteTestNoCatalog()), _gmail)
            .Handle(new GetQuoteSendDraftQuery(Olena, id), default)).Value;

        Assert.That(draft.FromEmail, Is.Null);
        Assert.That(draft.FileName, Is.EqualTo("КП 1-11-2026.pdf"));
        Assert.That(draft.Letters[0].Subject, Does.Contain("1-11-2026"));
        Assert.That(draft.Letters[0].Body, Does.Not.Contain("{managerName}"));
        Assert.That(draft.Letters.Count, Is.GreaterThan(1), "letter variants follow the template");
    }

    private sealed class FakeGmail : IGmailClient
    {
        public List<(string Token, GmailLetter Letter)> Sent { get; } = [];
        public string? Fail { get; set; }
        public bool IsConfigured => true;

        public Uri ConsentUrl(string state, string? loginHint) => new($"https://accounts.test/consent?hint={loginHint}");

        public Task<Result<GmailGrant>> ExchangeCodeAsync(string code, CancellationToken cancellationToken) =>
            Task.FromResult(Result.Success(new GmailGrant("Olena.Manager@brevi.test", code.Replace("code", "refresh"))));

        public Task<Result<string>> SendAsync(string refreshToken, GmailLetter letter, CancellationToken cancellationToken)
        {
            if (Fail is not null) return Task.FromResult(Result<string>.Error(Fail));
            Sent.Add((refreshToken, letter));
            return Task.FromResult(Result.Success("gmail-1"));
        }
    }

    private sealed class FakeProtector : IMailboxProtector
    {
        private readonly Dictionary<string, MailboxConnectState> _states = [];
        public string? LastState { get; private set; }

        public string ProtectToken(string refreshToken) => $"protected:{refreshToken}";

        public string? UnprotectToken(string protectedToken) =>
            protectedToken.StartsWith("protected:", StringComparison.Ordinal) ? protectedToken[10..] : null;

        public string ProtectState(MailboxConnectState state, TimeSpan lifetime)
        {
            LastState = $"state-{_states.Count}";
            _states[LastState] = state;
            return LastState;
        }

        public MailboxConnectState? UnprotectState(string state) => _states.GetValueOrDefault(state);
    }
}
