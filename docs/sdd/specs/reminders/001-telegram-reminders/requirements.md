# Requirements and scenarios

## Rules

- R-01 Task. A reminder task has a title (1–200 characters), an organization (ПП «Бреві», ФОП Согоконь or none for personal tasks), a schedule, an optional target chat, an "active from" date and an enabled flag. Delete is a soft archive: history stays.
- R-02 Schedule. Periodicity is monthly, quarterly or yearly. The due rule is one of:
  - day N of the month (1–31, clamped to the month length), counted from the last month of the period plus a month offset 0–12;
  - the last working day of that month;
  - N days (1–366) after the period ends.
  Lead days 0–60 (default 5) open the reminder window before the deadline.
- R-03 Working days. Monday to Friday are working days unless listed as an extra day off. No messages are sent on a non-working day.
- R-04 Period. Each task has at most one period record per period start. A period is open or done. Periods are created when their window opens, never before "active from".
- R-05 Daily run. On a working day the run sends one card per open period whose window has started, unless it is postponed past today or the task has no approved chat. A period gets at most one card per day, even if the run is repeated.
- R-06 Card. The card names the organization (omitted for personal tasks), the task, the period label in Ukrainian and the deadline. It adds "термін сьогодні", "прострочено на N дн." or, on the last working day before a deadline that falls on a day off, a warning naming that weekday.
- R-07 Buttons. «✅ Виконано» closes the period. «⏰ Нагадати пізніше» shows «Завтра», «Через 2 дні», «У понеділок» and «↩ Назад». Postponing counts working days; it never moves past the last working day before the deadline while that day is still ahead.
- R-08 Who pressed. Done and postpone record the Telegram user (name, then @username, then id) and the time. The first "done" wins; a later press answers "Вже закрито: name, time".
- R-09 Yesterday's buttons. When a new card is sent for a period, older cards of that period lose their buttons. When a period is closed (Telegram or admin), every live card of it shows the closed status without buttons.
- R-10 Chats. The bot records any chat that adds it or sends /start. A chat receives cards only after an admin approves it. Admin can rename the chat, send a test message and assign it to all tasks of chosen organizations.
- R-11 Admin history. Admin can list periods by status and task, close a period by hand (recorded as the admin user) and reopen it.
- R-12 Security. The webhook accepts updates only with the configured secret header. Admin endpoints need the Admin role in normal mode; the temporary anonymous API mode applies to them like to every controller.
- R-13 Defaults. On an empty task table the seed creates the 25 tasks of the brief with lead 5, enabled, no chat, active from the seed day.

## Scenarios

| ID | Given | When | Then |
| --- | --- | --- | --- |
| SC-01 | Monthly task due on the 20th, lead 5 | The 15th is a working day | A card is sent with buttons |
| SC-02 | Same task | The run is repeated the same day | No second card |
| SC-03 | Saturday | The run starts | Nothing is sent, no period is created |
| SC-04 | Deadline falls on Sunday | The run on Friday | The card warns that the deadline is on Sunday |
| SC-05 | Open card | Someone presses «Виконано» | Period done, name and time recorded, buttons removed, no more cards |
| SC-06 | Closed period | Someone else presses «Виконано» on an older card | Answer "Вже закрито", nothing changes |
| SC-07 | Open card | «Нагадати пізніше» → «Через 2 дні» on Thursday | Next card on Monday; the postpone is recorded |
| SC-08 | Deadline tomorrow | «У понеділок» | Postpone is capped at the last working day before the deadline |
| SC-09 | Card sent yesterday | Today's card is sent | Yesterday's card has no buttons |
| SC-10 | New group adds the bot | Update arrives | Chat is listed as not approved; no cards until approved |
| SC-11 | Webhook call without the secret | Request arrives | 401, no change |
| SC-12 | Empty database | Host.Seed runs | 25 default tasks, all deadlines match the brief |
| SC-13 | Invalid input (empty title, day 40, unknown chat) | Admin saves | 400 with field errors |
