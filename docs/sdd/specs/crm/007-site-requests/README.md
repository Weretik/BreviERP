# LEAD-002 — «Заявки з сайту»: requests from the site form

**Status:** backend implemented 2026-11-11; Admin page, the «Нагадування» checkbox and the site form in brevi-web. **Date:** 2026-11-11. **Module:** Crm («Менеджер сервісу»), schema `crm`; one column in Reminders. Feature card: [site requests](../../../features/crm/site-requests.md). Contract: [site-requests.openapi.yaml](../../../contracts/crm/site-requests.openapi.yaml).

## Why

The new site (brevi-web `apps/site`) has the form «Заявка» (phone, workwear, quantity, comment, file), but it only asked visitors to call. The owner decided on 2026-11-11 (thread «Настроить SEO сайта Brevi в админке», stage 8, last) that a request goes into Brevi for managers and to a Telegram group chosen on the existing «Нагадування» page, through the existing bot @brevibuhtime_bot.

## Decision

Requests live in Crm next to leads, but in their own table and page «Заявки з сайту»: a lead is a company the AI found, a request is a visitor who asked. The Telegram send uses the Reminders bot and chats through a Host.Api adapter; modules do not reference each other.

## Rules

- **R-01 Access.** Page `site-requests` («Менеджер сервісу › Заявки з сайту», view + edit): defaults Admin and Manager; the director always passes. The public form needs no sign-in.
- **R-02 Form.** Phone is required: 10 digits from `0` or 12 from `380` after removing spaces, brackets, dashes and `+`. Workwear ≤ 200, quantity ≤ 60, comment ≤ 1000 characters, locale `uk` or `ru`. One optional file ≤ 10 MB: pdf, jpg, jpeg, png, webp, heic, doc, docx, xls, xlsx, ai, cdr, eps, svg, zip. The form is limited to 20 requests per 10 minutes per IP (policy `SiteRequest`).
- **R-03 Save first.** The file goes to the CDN `/site-requests/{yyyy-MM}/{guid}{ext}`, then the request is saved, then Telegram is tried. A Telegram failure keeps the request and stores the reason (`NotifyError`); the visitor still gets 200.
- **R-04 Telegram.** The bot posts the request (HTML-escaped texts, file link, link to «Заявки з сайту») to every approved chat where the bot is a member and «Заявки з сайту» is ticked on «Нагадування». No ticked chat or no bot token is a stored reason, not an error.
- **R-05 Work.** Statuses `New` → `InWork` → `Done` or `Spam`; «Взяти» makes a new request the actor's; «Виконано» / «Спам» close it with an optional note; «Повернути» reopens a closed one as `InWork` of its owner (or `New` when nobody had it). Only the owner or Director/Admin manage a taken request. Every change checks the version (409 on a stale one).
- **R-06 List.** Tabs «Нові та в роботі», «Мої», «Закриті», «Усі» with counts, newest first, 50 per page.

## Scenarios

- **SC-01** A visitor sends phone and file → 200, the request is `New`, the file is on the CDN, ticked groups get the message.
- **SC-02** A bad phone, a long text, a wrong locale or a 12 MB file → 400 with Ukrainian messages; nothing is saved.
- **SC-03** No ticked group → the request is saved with «Не вибрано групу…» as the Telegram reason.
- **SC-04** A manager takes a new request; another manager cannot close it (403); the director can.
- **SC-05** «Спам» closes it with a note; «Повернути» brings it back.

## Design

| Layer | Files |
| --- | --- |
| Domain | `Crm.Domain/SiteRequests/` `SiteRequest`, `SiteRequestStatus` |
| Application | `Crm.Application/SiteRequests/` ports (store, file storage, notifier, links), contracts, mapper, messages, `Submit/`, `GetList/`, `Change/` |
| Infrastructure | `SiteRequestConfiguration`, `CrmDbContext : ISiteRequestStore`, migration AddSiteRequests |
| Api | `SiteRequestsController` (`/api/crm/site-requests`), `PublicSiteRequestsController` (`/api/public/site-requests`, multipart, anonymous, rate-limited) |
| Host | `Host.Api/Crm/SiteRequestFileStorage` (Media CDN), `SiteRequestNotifier` (Reminders bot and chats) |
| Reminders | `ReminderChat.ReceivesSiteRequests`, `ReminderChatInput.ReceivesSiteRequests` (optional, unchanged when absent), migration AddChatSiteRequests |

Rollout: migrations add a table and a `false` column; nothing changes until a group is ticked. Rollback: untick the group; the site falls back to «Зателефонуйте нам» when the API fails.

## Tasks and evidence

- **TS-01** Domain, application, persistence, API, Host adapters — `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~UnitTests.Crm.SiteRequests"`.
- **TS-02** Access and anonymous endpoint — `ApiAccessTests`, `AccessCatalog` defaults.
- **TS-03** Contract — `SiteRequestContractTests`, root `openapi.yaml`.
- **TS-04** brevi-web: Admin page, the «Нагадування» checkbox, the site form posts to the API.
