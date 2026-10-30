# TS-016 / FE-011 implementation audit — 2026-10-29

Scope: latest owned introduction selections, featured mapping, introduction quote creation, pinned public PDF, and FE-011 selection panel. This is a local working-tree checkpoint, not release acceptance.

| ID | Severity / status | Evidence and consequence | Target / owner |
| --- | --- | --- | --- |
| AF-P5-016-01 | High / verified fixed | IntroductionTemplatesController expected paged list while Application still returned skip/take array after interrupted agent work. Synced staged Application/store changes; count and rows use the same owner/admin/archive predicate and response retains value/pagedInfo. | TS-016 backend |
| AF-P5-016-02 | Medium / verified refactor | IntroductionTemplateStore mixed CRUD, read projection/pagination, transaction locks, replay journal and exception mapping. Split into IntroductionTemplateStore.cs (writes/shared mapping), .Reads.cs and .Mutations.cs. One scoped DbContext and existing transaction behavior retained. | TS-016 backend |
| AF-P5-016-03 | Low / verified refactor | IntroductionTemplateWriteRequest mixed wire DTO, slot DTO and conversion. Extracted IntroductionSlotSelectionRequest.cs and IntroductionTemplateRequestMapper.cs; strict unmapped-member rejection retained. | TS-016 API |
| AF-P5-016-04 | High / verified fixed | New introduction controllers are absent from canonical OpenAPI. Consumer sync and FE-011 API/route integration remain blocked until matching schemas, status/ownership/pagination/idempotency contract tests and immutable backend revision exist. | TS-016 API contract |
| AF-P5-016-05 | Medium / verified fixed | IntroductionQuoteCreator resolves template/current data before CreateQuote replay lookup. Retrying the same request after template archive or source/mapping change can fail before reaching an already successful quote replay. Deterministic child price keys prevent duplicate bases; they do not prove end-to-end replay semantics. | TS-016 application; add an actor-bound outer apply replay boundary and regression test |
| AF-P5-016-06 | Medium / verified fixed | Two additive CRM migrations exist, but the interrupted database agent did not deliver final apply/persistence evidence. Do not claim local database migration or persistence acceptance from scaffolding/build alone. | TS-016 database verification |

## Functional boundaries reviewed

- Template payload stores selection IDs/version references rather than old prices; apply revalidates current offers. Store access predicates constrain Managers to own templates and Admin to all; replay checks current access.
- IntroductionQuoteContentBuilder and IntroductionQuotePinValidator reject different selected identities/prices. Public snapshots are separate from private calculation basis; rendering uses pinned fields. Ordinary save/reprice/copy reject introduction versions until a typed edit flow exists.
- FE-011 model owns pure layout/approval rules; use-introduction-selection owns asynchronous check/apply state; panel composes UI. No extra split is warranted for the small panel/hook. API/route remains unfinished; callback tests are not full browser acceptance.
- Unfinished OpenAPI, historical PDF visual acceptance, database acceptance and explicit-send dependency prevent marking TS-016 or FE-011 complete.

## Verification

Fresh checks: Host.Api build passed with zero warnings/errors; focused Introductions unit tests passed 11/11; FE-011 selection panel component tests passed 3/3; backend and frontend git diff --check passed (line-ending notices only). The unit-test command reports an existing NU1903 dependency advisory for Microsoft.OpenApi.YamlReader 2.7.5. Open findings above are not closed by compilation or unit tests.

## Follow-up 2026-10-30

AF-P5-016-04/05/06 are verified fixed by [backend gate evidence](../evidence/ts-016-backend-gates-2026-10-30.md). Their original descriptions above record the discovered state. Frontend integration and PDF acceptance remain open.
