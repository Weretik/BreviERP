# P2 draft-quote backend evidence — 2026-10-26

## Delivered locally

- `Crm.Domain/Quotes` owns the draft aggregate, immutable versions, ordered items/variants and three tier values. `Crm.Application/Quotes` separates preview, create, save, get and list use cases and maps saved snapshots. `Crm.Infrastructure/Persistence/Quotes` owns token storage, transactions, replay journal and scoped reads. `Crm.Api/Quotes` exposes the five operations in `contracts/crm/quote-drafts.openapi.yaml`.
- The Host adapter reads current Catalog Sewing publications together with their saved calculation versions, or evaluates approved PPE offer/rule inputs through the existing Catalog engine. One-off PPE requires manual prices and a reason; a catalog PPE override requires tier and reason. Draft prices are UAH without additional VAT multiplication (D-11).
- Preview persists a 15-minute actor/quote/input-scoped basis token. Save copies the complete basis and tiers into a new immutable version, checks source identity for a fresh Catalog token, and atomically advances the root revision. Reopening and text-only edits retain saved prices. Blocked source prices remain null with blocker codes in drafts.
- Additive migration `20261026151551_AddQuoteDrafts` was generated and applied only by the test fixture to its own disposable `brevi_crm_test_*` localhost database. No working database migration or import ran.

## Verification

| Check | Result |
| --- | --- |
| `dotnet test tests/IntegrationTests/IntegrationTests.csproj --no-restore --filter FullyQualifiedName~QuoteCatalogPriceReaderTests --verbosity quiet` | 3 passed: approved external PPE, fallback rule pinning/inactive PPE, missing Sewing publication |
| Final combined `FullyQualifiedName~IntegrationTests.Crm` run | 25/25 passed in 12 seconds against the disposable localhost database; earlier customer and quote subsets separately passed 12/12 and 11/11 |
| Expired token and one-off PPE regression tests | 2/2 passed after adjusting the test fixture to satisfy its `ExpiresAt > CreatedAt` database constraint |
| Earlier quote persistence and HTTP tests | Preview/create/reopen/text-only V2, replay/scope/source drift/race; unauthorized role and forged totals rejected |
| OpenAPI parser/generator check | Canonical aggregate and standalone quote contract parsed by `openapi-typescript` from a temporary ASCII path; no frontend snapshot generated |
| `git diff --check` | Passed; Git reported only existing CRLF normalization warnings |

The first combined CRM run ended with 3 unspecified failures after about five minutes under quiet logging. A diagnostic repeat passed 23/23 in about 12 seconds; both isolated subsets passed. The first run's failure names were not captured, so this is recorded as an unresolved intermittent run rather than attributed to a specific defect.

## Pending handoff

Frontend FE-003/FE-004 and integrated/manual acceptance are open. The frontend contract workflow requires a clean immutable backend SHA. The original session forbade commits; on 2026-10-27 the user authorized the local backend commit without push to enable handoff. The resulting revision is recorded by the frontend sync tooling. EN-005 still owns customer-ready tax language and numbering. No live deployment is claimed.
