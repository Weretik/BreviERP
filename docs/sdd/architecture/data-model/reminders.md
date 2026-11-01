# Reminders schema

Code model reviewed 2026-11-01. `Reminders.Infrastructure.Persistence.RemindersDbContext` owns schema `reminders`; migration history is `reminders.__EFMigrationsHistory`. Migration `InitialReminders` creates all tables. It was applied only to a disposable local database.

| Table | Identity / relationships / purpose |
| --- | --- |
| Tasks | Generated long id. Title, Organization, schedule columns Periodicity, DueRule, DueDay, DueMonthOffset, LeadDays; nullable ChatId FK → Chats (restrict); ActiveFrom, IsEnabled, audit and soft-delete columns |
| Occurrences | Generated long id; TaskId FK → Tasks (restrict). Periodicity, PeriodStart, DueDate, Status, SnoozedUntil, ClosedBy*/ClosedAt, PostponedBy*/PostponedAt, CreatedAt. Unique (TaskId, PeriodStart); index (Status, DueDate) |
| OccurrenceActions | Generated long id; required OccurrenceId FK (cascade). Kind, Actor* columns, At, SnoozedUntil. Append-only journal |
| Chats | Generated long id; unique TelegramChatId. Kind, TelegramTitle, DisplayName, IsApproved, IsBotMember, DiscoveredAt, UpdatedAt |
| SentMessages | Generated long id; OccurrenceId FK (cascade), ChatId FK → Chats (restrict); TelegramChatId, TelegramMessageId, SentOn (Kyiv date), SentAt, HasButtons. Unique (OccurrenceId, SentOn) makes the daily run idempotent |
| NonWorkingDays | PK Date; Note |

Actor columns are values (name, Telegram user id, admin user id); there is no FK to Identity or any other module.
