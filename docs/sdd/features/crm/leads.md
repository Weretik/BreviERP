# «Менеджер сервісу»: leads

**Implemented:** backend of [LEAD-001](../../specs/crm/006-leads/README.md) (2026-11-08). The AI writes companies into the Google sheet «Лиды Brevi»; the step `leads` of the daily sync job and «Синхронізувати зараз» bring them in by company name (not by the «ID» the AI may repeat) without touching the calling work; a new «Кто взял в работу» / «Результат звонка» / «Следующий контакт» of the sheet takes the lead for that manager and records the call once. Managers take free leads («Усі ліди»), call them on «Мій день» and record a result; a next contact makes a «Подзвонити» task in «Мої завдання» and closes the previous one. «Адміністрування › Ліди» shows «Зведення», edits «Довідники» (call results, category descriptions) and runs the sync.

Access: page `leads` (view, edit) — Admin, Manager; page `lead-stats` (view, create = sync, edit = references) — Admin; the director always.

## Code structure

```text
src/Modules/Crm/
  Crm.Domain/Crm.Domain/Leads/
    Lead.cs                        root: sheet fields, take, release, record call, stage, version
    LeadCall.cs, LeadSheetRow.cs, LeadPerson.cs, LeadStage.cs, LeadOutcome.cs (stage/date rules)
    LeadCallResult.cs, LeadCategory.cs, LeadSyncState.cs, LeadLimits.cs
  Crm.Domain/Crm.Domain/Tasks/ManagerTask.cs   LeadId, ForLeadCall
  Crm.Application/Crm.Application/Abstractions/Leads/   ILeadStore (sync lock), ILeadSheetReader
  Crm.Application/Crm.Application/Features/Leads/
    Queries/                       GetLeads GetLead GetMyDay GetLeadCallResults GetLeadStats GetLeadCategories
    Commands/                      TakeLead ReleaseLead RecordLeadCall SaveLeadCallResult SaveLeadCategory SyncLeads
    Shared/Access/                 LeadActor, Ukrainian messages
    Shared/Persistence/            load/save helpers
    Shared/Sheet/                  LeadSheetParser (columns by header), LeadSheetSync
    Shared/Calling/LeadDetailLoader.cs  card with category text and open task
    Shared/                        responses, Mapping/, LeadClock.cs (Kyiv day)
  Crm.Infrastructure/Crm.Infrastructure/
    Leads/                         GoogleLeadSheetReader (ADC, read-only), LeadSheetOptions (Crm:LeadSheet)
    Persistence/Leads/                  five tables, seeded call results
    CrmDbContext.cs                implements ILeadStore
  Crm.Api/Crm.Api/Leads/
    LeadsController.cs             /api/crm/leads
    LeadStatsController.cs         /api/crm/lead-stats
src/Bootstrapper/Host.Seed/Jobs/Sync/LeadsSyncTask.cs   daily step `leads`
```

[API contract](../../contracts/crm/leads.openapi.yaml), [database ownership](../../architecture/data-model/crm.md), [sync job](../../operations/jobs/sync-job.md#leads).

## Tests

- `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~UnitTests.Crm.Leads"` — parser, rules, access defaults, contract, policies.
- `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter LeadPersistenceTests` — sync, taking, calls and tasks, «Мій день», stats over PostgreSQL (`BREVIERP_CRM_TEST_CONNECTION`).
