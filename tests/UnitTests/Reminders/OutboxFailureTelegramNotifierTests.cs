using BuildingBlocks.Application.Outbox;
using Microsoft.Extensions.Logging.Abstractions;
using Reminders.Contracts.Telegram;
using Reminders.Infrastructure.Integrations.Telegram;

namespace UnitTests.Reminders;

/// <summary>EVT-07: an outbox message that gave up is posted to the «Заявки з сайту» groups.</summary>
public sealed class OutboxFailureTelegramNotifierTests
{
    private static readonly OutboxFailure Failure = new(Guid.Parse("01900000-0000-7000-8000-000000000001"),
        "crm.site-request.notify", OutboxMessage.MaxAttempts, "Bad <request>", new DateTimeOffset(2026, 11, 17, 9, 5, 0, TimeSpan.Zero));

    [Test]
    public async Task Posts_the_failure_to_every_site_request_group()
    {
        var bot = new FakeBot(isConfigured: true, chats: [-100, -200]);

        await new OutboxFailureTelegramNotifier(bot, NullLogger<OutboxFailureTelegramNotifier>.Instance)
            .NotifyGaveUpAsync(Failure, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(bot.Sent.Select(x => x.ChatId), Is.EqualTo(new long[] { -100, -200 }));
            Assert.That(bot.Sent[0].Html, Does.Contain("Не вдалося виконати фонову дію"));
            Assert.That(bot.Sent[0].Html, Does.Contain("crm.site-request.notify"));
            Assert.That(bot.Sent[0].Html, Does.Contain("Bad &lt;request&gt;"));
            Assert.That(bot.Sent[0].Html, Does.Contain("2026-11-17 09:05 UTC"));
        });
    }

    [Test]
    public async Task Does_nothing_when_the_bot_is_not_configured()
    {
        var bot = new FakeBot(isConfigured: false, chats: [-100]);

        await new OutboxFailureTelegramNotifier(bot, NullLogger<OutboxFailureTelegramNotifier>.Instance)
            .NotifyGaveUpAsync(Failure, CancellationToken.None);

        Assert.That(bot.Sent, Is.Empty);
    }

    [Test]
    public void A_long_error_is_cut()
    {
        var text = OutboxFailureTelegramNotifier.Text(Failure with { LastError = new string('x', 2000) });

        Assert.That(text, Does.Contain(new string('x', OutboxFailureTelegramNotifier.ErrorPreviewLength) + "…"));
        Assert.That(text, Does.Not.Contain(new string('x', OutboxFailureTelegramNotifier.ErrorPreviewLength + 1)));
    }

    private sealed class FakeBot(bool isConfigured, IReadOnlyList<long> chats) : ITelegramBot
    {
        public List<(long ChatId, string Html)> Sent { get; } = [];
        public bool IsConfigured => isConfigured;

        public Task<long> SendAsync(long chatId, int? threadId, string html, CancellationToken cancellationToken)
        {
            Sent.Add((chatId, html));
            return Task.FromResult((long)Sent.Count);
        }

        public Task<IReadOnlyList<TelegramGroup>> ListGroupsAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<TelegramGroup>>([]);

        public Task<IReadOnlyList<long>> SiteRequestChatIdsAsync(CancellationToken cancellationToken) =>
            Task.FromResult(chats);
    }
}
