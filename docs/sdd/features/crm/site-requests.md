# «Менеджер сервісу»: site requests

**Implemented:** backend of [LEAD-002](../../specs/crm/007-site-requests/README.md) (2026-11-11). The site form «Заявка» posts to `POST /api/public/site-requests` (anonymous, multipart, 20 per 10 minutes per IP). The file goes to the CDN, the request is saved, and the bot @brevibuhtime_bot posts it to the groups ticked «Заявки з сайту» on «Нагадування»; a Telegram failure only stores the reason. Managers see the list «Заявки з сайту», take a request and close it as done or spam.

Access: page `site-requests` (view, edit) — Admin, Manager; the director always.

## Code structure

```text
src/Modules/Crm/
  Crm.Domain/Crm.Domain/SiteRequests/        SiteRequest (take, close, reopen, notified), SiteRequestStatus
  Crm.Application/Crm.Application/SiteRequests/
    Abstractions/SiteRequestPorts.cs         store, file storage, notifier, admin links
    Contracts/, SiteRequestMapper.cs, SiteRequestMessages.cs
    Submit/ GetList/ Change/
  Crm.Infrastructure/Crm.Infrastructure/Persistence/Configurations/SiteRequests/
  Crm.Api/Crm.Api/SiteRequests/              SiteRequestsController, PublicSiteRequestsController
src/Bootstrapper/Host.Api/Crm/               SiteRequestFileStorage (Media CDN), SiteRequestNotifier (Reminders bot)
src/Modules/Reminders/.../Chats/ReminderChat.cs   ReceivesSiteRequests
```

[API contract](../../contracts/crm/site-requests.openapi.yaml), [database ownership](../../architecture/data-model/crm.md).

## Tests

- `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~UnitTests.Crm.SiteRequests"` — submit, Telegram failure, take/close rules, contract.
- `dotnet test tests/HostApiTests/HostApiTests.csproj --filter ApiAccessTests` — the anonymous endpoint.
