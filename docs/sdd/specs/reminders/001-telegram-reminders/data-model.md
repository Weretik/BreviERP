# Data model

Schema `reminders`, `RemindersDbContext`, history `reminders.__EFMigrationsHistory`, migration `InitialReminders`. Current table details live in the [database map](../../../architecture/data-model/reminders.md).

| Table | Purpose | Keys and rules |
| --- | --- | --- |
| Tasks | Reminder task with schedule columns (Periodicity, DueRule, DueDay, DueMonthOffset, LeadDays) | Generated long id; FK ChatId → Chats (restrict); soft delete and audit columns |
| Occurrences | One period of a task: deadline, status, postpone date, who closed or postponed | Unique (TaskId, PeriodStart); index (Status, DueDate); FK TaskId → Tasks (restrict) |
| OccurrenceActions | Journal of done, postponed and reopened actions with actor and time | FK OccurrenceId (cascade) |
| Chats | Telegram chats that met the bot | Unique TelegramChatId; approval and bot membership flags |
| SentMessages | Cards sent to Telegram | Unique (OccurrenceId, SentOn); FKs to Occurrences (cascade) and Chats (restrict); HasButtons flag |
| NonWorkingDays | Extra days off | PK Date |

Actors are value columns (name, Telegram user id, admin user id). There are no FKs to Identity or any other module.

Baseline: the schema is new, so the migration only creates objects. Rollback before use: revert the migration; after use, keep the schema and disable the job.
