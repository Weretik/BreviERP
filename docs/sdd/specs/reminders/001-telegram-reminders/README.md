# Reminders 001: Telegram deadline reminders

Date: 2026-11-01. Owner: new `Reminders` module (schema `reminders`). Status: implemented and locally verified; not deployed. The user asked to replace the Make scenario of the bot that reminds the accountant and the owner's family about tax, payroll and personal deadlines.

## Goal

Admin staff keep a list of recurring deadline tasks for ПП «Бреві», ФОП Согоконь and personal tasks. On working days at 09:00 Kyiv time, a Telegram bot posts one card per task whose reminder window is open. Anyone in the chat can mark the period done or postpone it. The system records who pressed the button and when, and stops reminding once a period is done.

## Documents

- [Requirements and scenarios](requirements.md)
- [Design and decisions](design.md), including the new-module decision and rejected alternatives
- [Data model](data-model.md)
- [API contract decisions](contracts/api-contract.md) and the machine-readable [OpenAPI contract](../../../contracts/reminders/telegram-reminders.openapi.yaml)
- [Tasks](tasks.md) and [traceability and evidence](traceability.md)
- Operations: [Telegram reminders runbook](../../../operations/jobs/telegram-reminders.md)
- Feature card: [Telegram reminders](../../../features/reminders/telegram-reminders.md)

## User decisions (2026-10-31)

| # | Question | Decision |
| --- | --- | --- |
| 1 | What does "Нагадати пізніше" do | Shows a choice: tomorrow, in 2 days, on Monday |
| 2 | Send time | 09:00 Kyiv, working days only |
| 3 | Brief item 12 (ЄСВ) | Two tasks: payment by the 20th monthly, report 40 days after the quarter |
| 4 | Items 19 and 21 | Due on the 18th and the 12th |
| 5 | Who may press buttons | Anyone in the chat |
| 6 | Message language | Ukrainian |
| 7 | Bot | Existing @brevibuhtime_bot; the owner has BotFather access; Make is dropped |
| 8 | Message layout | Variant A: one message per task with its own buttons; yesterday's buttons are removed |
| 9 | Admin access to personal tasks | Not a concern now: one person uses the open admin |

## Scope boundaries

- In scope: task CRUD, due-date rules, working calendar with extra days off, daily dispatch job, Telegram buttons webhook, chat discovery and approval, period history, the 25 default tasks from the brief.
- Out of scope: per-user visibility of personal tasks (decision 9), automatic Ukrainian holiday calendar (martial law keeps holidays as working days; extra days off are entered by hand), non-Telegram channels, reminders at other times of day.
