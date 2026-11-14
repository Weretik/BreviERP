using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Reminders.Application.Abstractions;
using Reminders.Domain.Chats;
using Reminders.Domain.Schedules;
using Reminders.Domain.Tasks;
using Reminders.Infrastructure.Persistence;
using Reminders.Application.Features.Dispatch.Shared;
using Reminders.Application.Features.Telegram.Commands.HandleTelegramUpdate;
using Reminders.Application.Features.Telegram.Shared;
using Reminders.Application.Abstractions.Telegram;
using Reminders.Application.Features.Telegram.Shared.Chats;

namespace UnitTests.Reminders;

internal sealed class FakeTelegram : ITelegramGateway
{
    private long _nextMessageId = 100;
    public List<(long ChatId, long MessageId, string Text, TelegramKeyboard? Keyboard)> Sent { get; } = [];
    public List<(long ChatId, long MessageId, string Text, TelegramKeyboard? Keyboard)> Edited { get; } = [];
    public List<(long ChatId, long MessageId, TelegramKeyboard? Keyboard)> KeyboardEdits { get; } = [];
    public List<(string Id, string? Text)> Answers { get; } = [];
    public bool IsConfigured => true;

    public Task<long> SendMessageAsync(long chatId, string html, TelegramKeyboard? keyboard, CancellationToken cancellationToken)
    {
        var id = _nextMessageId++;
        Sent.Add((chatId, id, html, keyboard));
        return Task.FromResult(id);
    }

    public Task<long> SendTopicMessageAsync(long chatId, int? threadId, string html, CancellationToken cancellationToken)
    {
        var id = _nextMessageId++;
        Sent.Add((chatId, id, html, null));
        return Task.FromResult(id);
    }

    public Task EditMessageAsync(long chatId, long messageId, string html, TelegramKeyboard? keyboard, CancellationToken cancellationToken)
    {
        Edited.Add((chatId, messageId, html, keyboard));
        return Task.CompletedTask;
    }

    public Task EditKeyboardAsync(long chatId, long messageId, TelegramKeyboard? keyboard, CancellationToken cancellationToken)
    {
        KeyboardEdits.Add((chatId, messageId, keyboard));
        return Task.CompletedTask;
    }

    public Task AnswerCallbackAsync(string callbackQueryId, string? text, CancellationToken cancellationToken)
    {
        Answers.Add((callbackQueryId, text));
        return Task.CompletedTask;
    }

    public Task SetWebhookAsync(string url, string secretToken, CancellationToken cancellationToken) => Task.CompletedTask;
}

/// <summary>In-memory module with a fake clock in Kyiv business hours and a fake Telegram.</summary>
internal sealed class ReminderTestHarness
{
    private readonly SqliteConnection _connection;

    public const long GroupTelegramId = -1001;

    public ReminderTestHarness(DateTimeOffset now)
    {
        Time = new FakeTimeProvider(now);
        // SQLite in memory: real SQL, unique indexes and complex types, without a PostgreSQL server.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        Db = new RemindersDbContext(new DbContextOptionsBuilder<RemindersDbContext>().UseSqlite(_connection).Options);
        Db.Database.EnsureCreated();
        Clock = new ReminderClock(Time);
    }

    public FakeTimeProvider Time { get; }
    public RemindersDbContext Db { get; }
    public ReminderClock Clock { get; }
    public FakeTelegram Telegram { get; } = new();

    public ReminderDispatcher Dispatcher() => new(Db, Telegram, Clock, new ReminderOccurrenceGenerator(Db), NullLogger<ReminderDispatcher>.Instance);

    public ReminderButtonHandler Buttons() => new(Db, Telegram, Clock);

    public async Task<ReminderChat> ApprovedGroupAsync()
    {
        var chat = ReminderChat.Discover(GroupTelegramId, ReminderChatKind.Group, "Бухгалтерія", Clock.Now);
        chat.Configure(null, true, Clock.Now);
        Db.Chats.Add(chat);
        await Db.SaveChangesAsync();
        return chat;
    }

    public async Task<ReminderTask> TaskAsync(ReminderChat chat, DueRule due, DateOnly activeFrom, string title = "Реєстрація ПН")
    {
        var task = ReminderTask.Create(title, ReminderOrganization.PpBrevi,
            new ReminderSchedule(ReminderPeriodicity.Monthly, due, 5), chat.Id, activeFrom, true, Clock.Now).Value;
        Db.Tasks.Add(task);
        await Db.SaveChangesAsync();
        return task;
    }

    public static TelegramCallbackQuery Press(long messageId, string data, long userId = 42, string name = "Олена") =>
        new($"cb-{Guid.NewGuid():N}", new TelegramUser(userId, false, name, null, null),
            new TelegramMessage(messageId, new TelegramChat(GroupTelegramId, "supergroup", "Бухгалтерія", null, null, null), null, null), data);
}
