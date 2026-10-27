# P2 functional audit and refactoring evidence — 2026-10-26

Scope: the just-implemented P2 backend quote slice, its tests, mappings and actual architecture documentation. See [findings](../code-audit/p2-implementation-audit-2026-10-26.md) and the [current feature map](../../../../features/crm/clients-and-quotes.md). Frontend runtime code and canonical HTTP payloads did not change in this audit.

## Verification and behavior

| Stage / command | Observed result |
| --- | --- |
| Red: `dotnet test tests/IntegrationTests/IntegrationTests.csproj --no-build --filter FullyQualifiedName~QuoteVersionIntegrityTests --logger 'console;verbosity=normal'` | Two tests failed because SQL updates to a foreign quote's current/parent version raised no exception. This reproduced the missing database invariant. |
| Green: same integrity filter with `--no-restore --verbosity quiet` after composite mapping/migration | 2/2 passed; invalid references raise SQLSTATE 23503. |
| Refactor: `dotnet test tests/IntegrationTests/IntegrationTests.csproj --no-restore --filter FullyQualifiedName~IntegrationTests.Crm.Quotes --logger 'console;verbosity=normal'` | 18/18 passed after resolver/journal extraction and folder moves, including new access/paging and migration rollback/reapply checks. |
| Final regression: `dotnet test tests/IntegrationTests/IntegrationTests.csproj --no-restore --filter 'FullyQualifiedName~IntegrationTests.Crm.Quotes\|FullyQualifiedName~CustomerApiTests' --logger 'console;verbosity=normal' --verbosity quiet` | Exit 0; 21/21 passed after the shared actor claims reader moved to `Crm.Api/Access`. The filter uses a literal pipe between the two alternatives. |
| `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj --no-restore --filter FullyQualifiedName~CrmDependencyTests --verbosity quiet` | 1/1 passed; CRM project references preserve module/layer boundaries. |
| `dotnet ef migrations has-pending-model-changes --project src/Modules/Crm/Crm.Infrastructure/Crm.Infrastructure/Crm.Infrastructure.csproj --startup-project src/Modules/Crm/Crm.Infrastructure/Crm.Infrastructure/Crm.Infrastructure.csproj --context CrmDbContext --no-build` | Exit 0; no model changes since the last migration. Uses the design-only factory, no runtime database. |
| `git diff --check` and local Markdown link checks for the changed audit/evidence/feature/database pages | Passed; Git only reports line-ending normalization warnings. |

The tests receive local PostgreSQL configuration without printing it. `CrmDatabaseFixture` checks localhost, creates its own random `brevi_crm_test_*` database, migrates it and drops it. Tests never migrate the configured working database.

## Migration review and deployment boundary

`20261026154708_EnforceQuoteVersionOwnership` changes constraints and indexes only; it does not remove business rows or alter columns. Explicit identity mapping preserves Quote ID generation when Id also participates in the composite FK. Its Up/Down and snapshot were reviewed; tests apply the full chain, then downgrade to `AddQuoteDrafts` and upgrade with a valid existing draft, preserving data and allowing a second generated ID.

Normal deployment must apply the additive migration through Host.Seed before rollout. Existing cross-quote references, if any exist in a target, must be reconciled before applying it: the new FK intentionally rejects inconsistent history rather than rewriting it. Down restores ordinary single-column FKs and weakens integrity; the rollback test proves mechanics in a disposable environment, not permission to downgrade a working database. The earlier `AddQuoteDrafts` migration was preserved.

## Residual scope

No working database migration, import, commit, push or deployment ran. Expired basis retention remains the existing P2-A04 operations follow-up; expired tokens cannot authorize a save. Ready/PDF, lifecycle actions, custom sewing and frontend quote integration remain later work. No whole-solution test suite was run.
