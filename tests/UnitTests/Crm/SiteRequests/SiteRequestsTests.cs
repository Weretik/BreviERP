using Ardalis.Result;
using Crm.Application.SiteRequests;
using Crm.Application.SiteRequests.Abstractions;
using Crm.Application.SiteRequests.Change;
using Crm.Application.SiteRequests.Contracts;
using Crm.Application.SiteRequests.GetList;
using Crm.Application.SiteRequests.Submit;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

namespace UnitTests.Crm.SiteRequests;

/// <summary>«Заявки з сайту»: the site form, the Telegram card and the work of managers (LEAD-002).</summary>
public sealed class SiteRequestsTests
{
    private static readonly CancellationToken None = CancellationToken.None;
    private readonly FakeTimeProvider time = new(new DateTimeOffset(2026, 11, 11, 10, 0, 0, TimeSpan.Zero));
    private readonly FakeSiteRequestFiles files = new();
    private readonly FakeSiteRequestNotifier notifier = new();
    private static readonly SiteRequestLinks Links = new("https://admin.test/");

    private SubmitSiteRequestCommandHandler Submit(Func<SiteRequestTestDb> open) =>
        new(open(), files, notifier, Links, time, NullLogger<SubmitSiteRequestCommandHandler>.Instance);

    private ChangeSiteRequestCommandHandler Change(Func<SiteRequestTestDb> open) =>
        new(open(), new FakeSiteRequestStaff(), time);

    private static SubmitSiteRequestCommand Form(string phone = "+38 (050) 123-45-67", SiteRequestFile? file = null) =>
        new(phone, " Костюми  <для> кухні ", "40", "Потрібно до березня", "ru", file);

    [Test]
    public async Task A_request_is_saved_with_its_file_and_posted_to_telegram()
    {
        var (connection, open) = SiteRequestTestDb.Create();
        using var _ = connection;
        var file = new SiteRequestFile(new MemoryStream(new byte[2048]), "ТЗ бриф.PDF", "application/pdf", 2048);

        var saved = await Submit(open).Handle(Form(file: file), None);
        await using var db = open();
        var request = db.SiteRequests.Single();

        Assert.Multiple(() =>
        {
            Assert.That(saved.Value.Id, Is.EqualTo(request.Id));
            Assert.That(request.Workwear, Is.EqualTo("Костюми <для> кухні"));
            Assert.That(request.Locale, Is.EqualTo("ru"));
            Assert.That(files.Uploads.Single().Key, Does.Match(@"^/site-requests/2026-11/[0-9a-f]{32}\.pdf$"));
            Assert.That(request.FileUrl, Is.EqualTo($"https://cdn.test{files.Uploads.Single().Key}"));
            Assert.That(request.FileName, Is.EqualTo("ТЗ бриф.PDF"));
            Assert.That(request.NotifiedAt, Is.EqualTo(time.GetUtcNow()));
            Assert.That(notifier.Messages.Single(), Does.Contain("+38 (050) 123-45-67"));
            Assert.That(notifier.Messages.Single(), Does.Contain("Костюми &lt;для&gt; кухні"));
            Assert.That(notifier.Messages.Single(), Does.Contain("https://admin.test/crm/site-requests"));
        });
    }

    [Test]
    public async Task A_telegram_failure_keeps_the_request_with_the_reason()
    {
        var (connection, open) = SiteRequestTestDb.Create();
        using var _ = connection;
        notifier.Error = "Не вибрано групу.";
        await Submit(open).Handle(Form(), None);
        notifier.Error = null;
        notifier.Throws = true;
        var second = await Submit(open).Handle(Form(), None);

        await using var db = open();
        var requests = db.SiteRequests.OrderBy(x => x.Id).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(second.IsSuccess, Is.True);
            Assert.That(requests.Select(x => x.NotifyError), Is.EqualTo(new[] { "Не вибрано групу.", "Telegram недоступний." }));
            Assert.That(requests.All(x => x.NotifiedAt is null), Is.True);
        });
    }

    [Test]
    public void The_form_needs_a_ukrainian_phone_and_an_allowed_small_file()
    {
        var validator = new SubmitSiteRequestCommandValidator();
        SiteRequestFile File(string name, long length) => new(Stream.Null, name, "application/octet-stream", length);

        Assert.Multiple(() =>
        {
            Assert.That(validator.TestValidate(Form("050 123 45 67")).IsValid, Is.True);
            Assert.That(validator.TestValidate(Form("+380501234567")).IsValid, Is.True);
            Assert.That(validator.TestValidate(Form("12345")).Errors.Select(x => x.ErrorMessage),
                Does.Contain(SiteRequestMessages.Phone));
            Assert.That(validator.TestValidate(Form(file: File("layout.exe", 10))).Errors.Select(x => x.ErrorMessage),
                Does.Contain(SiteRequestMessages.FileType));
            Assert.That(validator.TestValidate(Form(file: File("brief.pdf", 11 * 1024 * 1024))).Errors
                .Select(x => x.ErrorMessage), Does.Contain(SiteRequestMessages.FileTooLarge));
        });
    }

    [Test]
    public async Task Managers_take_close_and_reopen_with_a_version_check()
    {
        var (connection, open) = SiteRequestTestDb.Create();
        using var _ = connection;
        await Submit(open).Handle(Form(), None);

        var taken = await Change(open).Handle(new ChangeSiteRequestCommand(1, false, 1, new("Take", 1, null)), None);
        var stale = await Change(open).Handle(new ChangeSiteRequestCommand(2, false, 1, new("Take", 1, null)), None);
        var other = await Change(open).Handle(new ChangeSiteRequestCommand(2, false, 1, new("Take", 2, null)), None);
        var notYours = await Change(open).Handle(new ChangeSiteRequestCommand(2, false, 1, new("Done", 2, null)), None);
        var done = await Change(open).Handle(
            new ChangeSiteRequestCommand(1, false, 1, new("Done", 2, " Замовили 40 костюмів ")), None);
        var again = await Change(open).Handle(new ChangeSiteRequestCommand(1, false, 1, new("Spam", 3, null)), None);
        var reopened = await Change(open).Handle(new ChangeSiteRequestCommand(3, true, 1, new("Reopen", 3, null)), None);

        Assert.Multiple(() =>
        {
            Assert.That(taken.Value.Status, Is.EqualTo("InWork"));
            Assert.That(taken.Value.OwnerName, Is.EqualTo("Менеджер 1"));
            Assert.That(stale.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(other.Errors, Does.Contain(SiteRequestMessages.TakenBy("Менеджер 1")));
            Assert.That(notYours.Status, Is.EqualTo(ResultStatus.Forbidden));
            Assert.That(done.Value.Status, Is.EqualTo("Done"));
            Assert.That(done.Value.Note, Is.EqualTo("Замовили 40 костюмів"));
            Assert.That(again.Status, Is.EqualTo(ResultStatus.Invalid));
            Assert.That(reopened.Value.Status, Is.EqualTo("New"));
            Assert.That(reopened.Value.OwnerUserId, Is.Null);
        });
    }

    [Test]
    public async Task The_list_shows_open_requests_first_and_counts_the_tabs()
    {
        var (connection, open) = SiteRequestTestDb.Create();
        using var _ = connection;
        for (var i = 0; i < 3; i++) await Submit(open).Handle(Form(), None);
        await Change(open).Handle(new ChangeSiteRequestCommand(1, false, 2, new("Take", 1, null)), None);
        await Change(open).Handle(new ChangeSiteRequestCommand(2, false, 3, new("Spam", 1, null)), None);

        var handler = new GetSiteRequestsQueryHandler(open());
        var openList = (await handler.Handle(new GetSiteRequestsQuery(1, null), None)).Value;
        var mine = (await new GetSiteRequestsQueryHandler(open()).Handle(new GetSiteRequestsQuery(1, "Mine"), None)).Value;
        var closed = (await new GetSiteRequestsQueryHandler(open()).Handle(new GetSiteRequestsQuery(1, "Closed"), None)).Value;

        Assert.Multiple(() =>
        {
            Assert.That(openList.Items.Select(x => x.Id), Is.EqualTo(new long[] { 2, 1 }));
            Assert.That(openList.Counts, Is.EqualTo(new SiteRequestCountsResponse(1, 1, 1, 1)));
            Assert.That(mine.Items.Single().Id, Is.EqualTo(2));
            Assert.That(closed.Items.Single().Status, Is.EqualTo("Spam"));
        });
    }
}
