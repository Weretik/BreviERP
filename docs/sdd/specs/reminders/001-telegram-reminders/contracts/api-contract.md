# API contract decisions

Machine-readable contract: [telegram-reminders.openapi.yaml](../../../../contracts/reminders/telegram-reminders.openapi.yaml), generated from the running Host.Api on 2026-11-01 and referenced from the aggregate [openapi.yaml](../../../../contracts/openapi.yaml).

- Consumer: Brevi Admin (`libs/admin/reminders` in brevi-web).
- Security: Admin role in normal mode; the temporary anonymous API mode opens them like every controller. The webhook is anonymous, protected by the secret header and not part of the contract.
- Codes in JSON are strings: organization `none | ppBrevi | fopSohokon`; periodicity `monthly | quarterly | yearly`; due rule `dayOfMonth | lastWorkingDayOfMonth | daysAfterPeriodEnd`; status `open | done`; chat kind `group | private`; action `done | postponed | reopened`.
- Errors: 400 with the standard validation error list (field identifiers such as `Input.Title`; unknown chat has code `unknown_chat`); 404 for unknown ids; 409 when closing a closed period, reopening an open one, adding an existing day off, sending a test when the bot is not configured or no longer in the chat, or connecting the bot without configuration.
- Idempotency: writes are not keyed. Close and reopen are naturally idempotent by state (second call is 409). Daily sends are idempotent per period and day on the server.

| Operation | Route |
| --- | --- |
| getReminderTasks, createReminderTask | GET, POST `/api/reminders/tasks` |
| updateReminderTask, archiveReminderTask | PUT, DELETE `/api/reminders/tasks/{id}` |
| previewReminderDueDates | POST `/api/reminders/tasks/preview` |
| assignReminderChat | POST `/api/reminders/tasks/assign-chat` |
| getReminderOccurrences | GET `/api/reminders/occurrences?status&taskId&page&pageSize` |
| closeReminderOccurrence, reopenReminderOccurrence | POST `/api/reminders/occurrences/{id}/close`, `/reopen` |
| getReminderChats, updateReminderChat | GET `/api/reminders/chats`, PUT `/api/reminders/chats/{id}` |
| sendReminderChatTest | POST `/api/reminders/chats/{id}/test-message` |
| getReminderNonWorkingDays, addReminderNonWorkingDay | GET, POST `/api/reminders/non-working-days` |
| removeReminderNonWorkingDay | DELETE `/api/reminders/non-working-days/{date}` |
| connectTelegramWebhook | POST `/api/reminders/telegram/connect` |

Compatibility: all operations are new; no existing contract changes.
