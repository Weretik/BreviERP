# «Менеджер сервісу»: site requests

**Implemented:** backend of [LEAD-002](../../specs/crm/007-site-requests/README.md) (2026-11-11). The site form «Заявка» posts to `POST /api/public/site-requests` (anonymous, multipart, 20 per 10 minutes per IP). The file goes to the CDN, the request is saved, and the bot @brevibuhtime_bot posts it to the groups ticked «Заявки з сайту» on «Нагадування». The post goes through the [outbox](../../operations/jobs/outbox.md): saved with the request, tried at once, retried every minute after a Telegram failure (the reason is shown until it goes through). Managers see the list «Заявки з сайту», take a request and close it as done or spam.

Access: page `site-requests` (view, edit) — Admin, Manager; the director always.

## Code structure

```text
src/Modules/Crm/
  Crm.Domain/Crm.Domain/SiteRequests/        SiteRequest (take, close, reopen, notified), SiteRequestStatus
  Crm.Application/Crm.Application/Abstractions/SiteRequests/SiteRequestPorts.cs   store, file storage, notifier, admin links
  Crm.Application/Crm.Application/Features/SiteRequests/
    Commands/SubmitSiteRequest/ Commands/ChangeSiteRequest/ Queries/GetSiteRequests/
    Shared/                                  responses, SiteRequestMapper, SiteRequestMessages
    Shared/Notify/NotifySiteRequestOutboxHandler.cs  outbox message crm.site-request.notify
  Crm.Infrastructure/Crm.Infrastructure/Persistence/Configurations/SiteRequests/
  Crm.Api/Crm.Api/SiteRequests/              SiteRequestsController, PublicSiteRequestsController
src/Modules/Crm/Crm.Infrastructure/Crm.Infrastructure/Adapters/
  Media/SiteRequestFileStorage.cs           Media.Contracts upload
  Reminders/SiteRequestNotifier.cs          Reminders.Contracts ITelegramBot
src/Modules/Reminders/.../Chats/ReminderChat.cs   ReceivesSiteRequests
```

[API contract](../../contracts/crm/site-requests.openapi.yaml), [database ownership](../../architecture/data-model/crm.md).

## Tests

- `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~UnitTests.Crm.SiteRequests"` — submit, Telegram failure, outbox retry and repeat, take/close rules, contract.
- `dotnet test tests/HostApiTests/HostApiTests.csproj --filter ApiAccessTests` — the anonymous endpoint.
