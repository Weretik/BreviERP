# Traceability and evidence

| Scenario / rule | Test |
| --- | --- |
| R-02, SC-12 (all 25 deadlines, labels, validation) | `tests/UnitTests/Reminders/ReminderScheduleTests.cs` |
| R-03, R-05, R-06, R-07, R-08, SC-03, SC-04, SC-07, SC-08 | `tests/UnitTests/Reminders/ReminderPlannerTests.cs` |
| R-05, R-09, SC-01, SC-02, SC-03 | `tests/UnitTests/Reminders/ReminderDispatcherTests.cs` |
| R-07, R-08, R-09, R-10, SC-05, SC-06, SC-07, SC-10 | `tests/UnitTests/Reminders/ReminderButtonHandlerTests.cs` |
| D-01 module references and schema | `tests/ArchitectureTests/RemindersDependencyTests.cs` |
| Container restore inputs | `tests/ArchitectureTests/ContainerRestoreCoverageTests.cs` |
| SC-11, SC-13 | Manual local HTTP check (below) |

Unit tests use SQLite in memory (EF InMemory does not support complex properties) and `FakeTimeProvider`.

## Evidence — 2026-11-01 (local)

- `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~UnitTests.Reminders"`: 62 passed.
- `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj`: 7 passed. `dotnet test tests/HostApiTests/HostApiTests.csproj`: 16 passed.
- Host.Seed (Production environment) on an empty local PostgreSQL 16 database: `InitialReminders` applied, 7 tables in schema `reminders` including its history table, 25 tasks seeded.
- Host.Api against that database: GET tasks returned 25 tasks with the next three deadlines; preview of "40 days after quarter" returned 2026-11-09, 2027-02-09, 2027-05-10; invalid create returned 400 with field errors; connect without configuration returned 409. The OpenAPI contract was generated from this run.
- Not verified: real Telegram calls, Cloud Build, Cloud Scheduler and Cloud Run. They need the owner's bot token and Google Cloud access.
