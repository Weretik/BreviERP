# LEAD-001 — «Ліди»: calling the companies the AI finds

**Status:** stage 1 (backend) implemented 2026-11-08; Admin stages 2–3 in brevi-web. **Date:** 2026-11-08. **Module:** Crm («Менеджер сервісу»), schema `crm`. Feature card: [leads](../../../features/crm/leads.md). Contract: [leads.openapi.yaml](../../../contracts/crm/leads.openapi.yaml).

## Why

The owner asked on 2026-11-08 (project thread «Сделать страницу «Ліди»») for a page where managers call the companies an AI writes into the Google sheet «Лиды Brevi» (25 columns: ID, date, category A/B/C, company, region, industry, signal, why 50+ units, what to offer, contacts, whom to call, call reason, …). Today managers mark calls in the sheet, which is hard to read. The owner chose, from three concepts ([canvas](https://claude.ai/artifact/V6B1tNfxWfoQbDicuB2PuM)):

- concept C «Мій день» as the main tab: today's calls in a queue, one lead large with what to say and big result buttons;
- a tab «Усі ліди» where managers take leads for themselves;
- the old sheet tabs «Сводка» and «Справочники» go to a separate page in «Адміністрування»;
- the sheet keeps being filled by the AI; Brevi syncs it every weekday morning in the daily job, and «Синхронізувати зараз» is for Director and Admin by default;
- a call with a next contact puts a task into «Мої завдання» by itself.

## Decision: inside Crm, not a new module

Leads are the start of the client work of «Менеджер сервісу» and create manager tasks, so they live in Crm next to clients, КП and tasks, and share its DbContext (one transaction for a call and its task). The sheet is read with Application Default Credentials like the purchase register of Purchasing, without a shared library: modules do not reference each other.

## Rules

- **R-01 Access.** Page `leads` («Менеджер сервісу › Ліди», view + edit): defaults Admin and Manager. Page `lead-stats` («Адміністрування › Ліди: зведення, довідники, синхронізація», view + create + edit): default Admin; create is «Синхронізувати зараз», edit is «Довідники». The director always passes. A role made on «Права доступу» is treated like a manager.
- **R-02 Sheet fields.** Columns are found by header (Russian names of the sheet), so the AI may add or move columns; «ID» and «Предприятие» are required. A row without a whole-number ID or company is skipped. The AI may repeat an ID, so the ID is only the number managers search by (owner, 2026-11-08); a company written on several rows keeps its first row and the repeats are listed as the sync warning. The sheet owns the company fields: every sync overwrites them (long texts are cut, never refused). Dates read as `yyyy-MM-dd` or `dd.MM.yyyy`; anything else is empty. The manager columns of the sheet («Кто взял в работу», «Статус» other than «Свободно», «Результат звонка», «Следующий контакт», «Комментарий») are kept once, on the first sync, as «З таблиці» notes. Since 2026-11-08 (owner: managers already took ~960 leads in the sheet) the manager work is brought in too, each new sheet value once (`Lead.SheetOwner`, `Lead.SheetResult`), so a release or a call in Brevi is not undone by the next sync:
  - «Кто взял в работу» takes a free open lead for the active Manager whose name has the first word of the sheet name (Russian and Ukrainian spellings compared as one: «Анастасия» = «Анастасія»); no or two matching managers leave the lead free and name the value in the sync warning.
  - «Результат звонка» becomes a call of that manager on that manager's lead, with «Комментарий» as the comment; the call is dated «Дата добавления» 09:00 Kyiv (the sheet has no call date). The result is a «Довідники» result with the same name, else the default result for the Russian sheet value (by outcome when renamed): «Не дозвонились», «Вышли на ответственного» → «Вийшли на відповідального», «Узнать позже» → «Передзвонити», «Отправить предложение» / «отправили на почту» / «Нужен расчёт» / «Потенциальный заказ» → «Надіслали пропозицію», «Отказ» / «Потребности сейчас нет» → «Відмова», «Нецелевое предприятие» → «Нецільове підприємство», «ПЕРЕНЕСЕН в CRM» → «Перенесено в CRM»; a mapped value keeps «У таблиці: «…».» in the comment. «Не звонили» is no call; an unknown value is listed in the warning and applies once such a result exists.
  - «Следующий контакт» (09:00 Kyiv) is the next contact of that call and makes a «Подзвонити» task; a retry without a date gets no date (no task flood).
  - «Статус» is not used: the sheet has only «Свободно».
- **R-03 Sync.** Step `leads` of the daily brevierp-sync job (Mon–Fri 09:00 Kyiv) and «Синхронізувати зараз» run the same code. The whole sheet is read and checked first, then applied in one transaction under a PostgreSQL advisory lock: a failure changes nothing and is saved as the last error; a parallel run gets 409. Rows are matched by the company name with case, quotes and extra spaces ignored («ТОВ "Ромашка"» = «тов ромашка»), not by «ID» (owner, 2026-11-08); a renamed company becomes a new lead and the old one «немає в таблиці»; a lead whose row disappeared stays with its calls and is shown «немає в таблиці». New category codes are added to «Довідники» without a description. The tab is found by id (default gid 2104296700 of spreadsheet `1KRW3OeGi2fBTcGCdDWuww7qveGT1vbntgXt17brd2FQ`, config `Crm:LeadSheet`); the runtime service account must be a viewer of the sheet.
- **R-04 Taking.** Everyone with the page sees every lead. «Взяти собі» makes a free open lead the actor's (`New` → `InWork`); a lead somebody has is 409 with their name. «Повернути у вільні»: the owner, or Director/Admin for anybody's lead; the next contact is cleared, the open «Подзвонити» task deleted, the stage goes back to `New` (no calls) or `InWork`.
- **R-05 Stages.** `New` → `InWork` → `Talking` → `Offer` → `Client` or `Closed`. Only a recorded call changes the stage after taking. `Client` and `Closed` end the work: no taking, no calls.
- **R-06 Call results.** A result has a name (the director's text) and an outcome: `Retry` (next try in `RetryDays`, default 1, unless a date is given), `Callback`, `Talking`, `Offer` (a next contact is required), `Client`, `Closed` (no next contact). Seeded: «Не дозвонились» (retry 1 day), «Не відповідають» (retry 2 days), «Передзвонити», «Вийшли на відповідального», «Надіслали пропозицію», «Перенесено в CRM», «Відмова», «Нецільове підприємство». Results are never deleted, only switched off; at most 30.
- **R-07 Recording a call.** Only the owner records calls; a free lead is taken by the caller first. A call stores who, when, the result name and outcome, a comment (≤ 2000) and the next contact; it is never changed. The next contact may not be more than 5 minutes in the past. Every change checks the version the screen loaded (409).
- **R-08 Tasks.** Recording a call marks the lead's open «Подзвонити» task done; a next contact creates a new task for the actor: «Подзвонити: <компанія>», due at the next contact, description with the lead number, result, comment and call reason, `LeadId` set. So one open task per lead at most; the task opens the lead from «Мої завдання».
- **R-09 «Мій день».** Own open leads due by the end of the Kyiv day or never called (overdue and today first by time, then new by category and taking time), the next 50 planned later, calls recorded today, today's plan (calls + queue) and the number of free leads.
- **R-10 «Зведення».** For a period of Kyiv dates (default the last 7 days): totals now (all, free, in work, offer, clients, closed, overdue), calls and new leads in the period; a row per manager (own open leads, calls, % reached = calls that are not retries, offers, clients, overdue); calls per result; up to 20 industries by category; the last sync.
- **R-11 «Довідники».** Call results (add, rename, outcome, retry days, on/off, order) and categories (description only; codes come from the sheet).

## Scenarios

| ID | Given | When | Then |
| --- | --- | --- | --- |
| SC-01 | Sheet with 3 rows | Sync, then the sheet changes a company and loses a row | 3 added; then 1 updated, 1 gone; the owner and work stay; categories A, B; notes from «Кто взял» |
| SC-02 | Anastasiia took lead 1 | Vadym takes it; Vadym records a call | 409 with Anastasiia's name; 403 |
| SC-03 | Own lead | «Передзвонити» without a date; «Не дозвонились»; «Передзвонити» in 3 days; «Відмова» | 400; next contact +1 day and a task; first task done, new task due in 3 days with LeadId; stage Closed, no open task |
| SC-04 | Own lead with a task | Vadym releases it; the director releases it | 403; free, task deleted |
| SC-05 | One lead due today, one in 4 days, one free | «Мій день», «Усі ліди» search «київ», free chip | queue [1], later [2], 2 calls today, plan 3, free 1; one found; free [3] |
| SC-06 | Two calls (retry, offer) | «Зведення» for the default period | 2 calls, 50 % reached, 1 offer, results listed, industry «Склад» first, sync rows 3 |

Evidence: `tests/IntegrationTests/Crm/LeadPersistenceTests.cs` (SC-01…SC-06), `tests/UnitTests/Crm/Leads/` (parser, domain rules, access defaults, contract and policies).

## Data

Schema `crm`, migration `AddLeads`, see [Crm schema](../../../architecture/data-model/crm.md): `Leads`, `LeadCalls`, `LeadCallResults` (seeded), `LeadCategories`, `LeadSyncStates`, and `ManagerTasks.LeadId`. Identity users and results are stored as ids with names, without foreign keys. Rollback: the previous image ignores the new tables and column.

## Admin stages (brevi-web)

2. «Менеджер сервісу › Ліди»: tabs «Мій день» (concept C) and «Усі ліди» (list with search, chips «Мої / Вільні / Усі», category, «Взяти собі»); the lead card; the task panel of «Мої завдання» links to the lead.
3. «Адміністрування › Ліди»: tabs «Зведення» and «Довідники», «Синхронізувати зараз» with the last sync.
