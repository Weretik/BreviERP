# P2 quote implementation audit — 2026-10-26

## Scope and layout

Reviewed the new Crm quote Domain/Application/Infrastructure/API folders, Host Catalog adapter, additive migration, quote OpenAPI and focused tests. This follows the [preparation-only audit](p2-preparation-audit-2026-10-26.md); that earlier document describes the state before quote runtime code existed.

The follow-up audit covered behavior as well as file length. The earlier conclusion that no additional split was needed was too narrow: the version builder mixed monetary-basis verification with graph construction, and the mutation store mixed transaction coordination with request journaling. Both are now separated. Quote EF mappings and integration tests are grouped in their own `Quotes` directories; shared HTTP actor binding moved from Customers to `Crm.Api/Access`.

## Findings

| ID | Priority | Evidence and disposition |
| --- | --- | --- |
| P2-A01 | Fixed | Catalog source provenance was initially at risk of being reduced to displayed tiers. The adapter reads the saved Sewing calculation snapshot and pins composition/settings/reference versions; PPE pins the approved offer and a rule ID only when fallback uses the rule. Focused adapter tests cover both PPE branches and missing publication. |
| P2-A02 | Fixed | Expired actor-bound tokens and manual one-off PPE needed direct persistence coverage. Added tests for both; they pass against the isolated PostgreSQL fixture. |
| P2-A03 | Fixed; see AF-CRM-P2-005 | Cross-quote current/parent links were reproduced and fixed with composite foreign keys. |
| P2-A04 | Follow-up | `CalculationBases` indexes expiry but has no scheduled pruning. Tokens stop authorizing saves at expiry; storage retention/cleanup should be defined in an operations task before sustained production use. |

The persisted version snapshot includes the original basis token and expiry as historical provenance. Consumers must treat it as a past calculation record, not a currently valid save token. Save still consults `CalculationBases` and checks expiry. Consider a distinct read projection when FE-003 maps the snapshot.

## Follow-up findings and fixes

Paths below are relative to the module's repeated source root unless prefixed with `tests/` or `docs/`. All fixes are owned by the CRM P2 backend slice; no other repository or module implementation changed.

| ID | Severity / status | Exact path and evidence | Impact, target change and verification |
| --- | --- | --- | --- |
| AF-CRM-P2-003 | Medium / fixed | `Crm.Application/Crm.Application/Quotes/Mapping/QuoteVersionBuilder.cs` used to resolve tokens, deserialize prior bases, check Catalog source drift and build the version graph. | Extracted `Quotes/Pricing/QuotePriceBasisResolver.cs`. Builder owns graph construction; resolver owns monetary authority. Existing expiry, actor, source drift, one-off and text-only save tests pass after extraction. |
| AF-CRM-P2-004 | Medium / fixed | `Crm.Infrastructure/Crm.Infrastructure/Persistence/Quotes/Writing/QuoteMutationStore.cs` encoded requests, checked replay access and serialized journal rows inside transaction orchestration. | Extracted `Persistence/Quotes/Idempotency/{QuoteMutationJournal,EncodedQuoteMutation}.cs`; transaction/locks/error mapping remain in the store. Replay and concurrent-save tests pass. No generic cross-feature transaction abstraction was introduced. |
| AF-CRM-P2-005 | Medium / fixed | `Persistence/Configurations/Quotes/{QuoteConfiguration,QuoteVersionConfiguration}.cs`; two isolated SQL tests originally accepted a current/parent version belonging to another quote. | Added composite `(QuoteId,Id)` version key and same-quote FKs via additive `20261026154708_EnforceQuoteVersionOwnership`. Both tests failed with no exception before the fix and pass with PostgreSQL FK violations after it. Rollback/reapply with valid persisted data preserves both drafts and generated IDs. No dependency remains on TS-005 for this invariant. |
| AF-CRM-P2-006 | Low / fixed | `Crm.Application/Crm.Application/Quotes/PreviewQuotePrices/PreviewQuotePricesCommandHandler.cs` reused `GetQuote/Specifications/QuoteDetailSpec.cs`, contrary to the repository's use-case-local specification rule. | Added `PreviewQuotePrices/Specifications/QuoteForPricePreviewSpec.cs`. Foreign manager preview returns NotFound without invoking Catalog; read/save access and SQL paging are also covered. |
| AF-CRM-P2-007 | Low / fixed | `Crm.Api/Crm.Api/Quotes/QuotesController.cs` imported `Crm.Api.Customers` only for actor binding; seven quote mappings and quote test classes were mixed into parent directories. | Actor binding moved to `Crm.Api/Access/CrmActorClaims.cs`; mapping files to `Persistence/Configurations/Quotes/`; tests to `tests/IntegrationTests/Crm/Quotes/`. HTTP names and payloads stay unchanged. Quote and customer API regressions verify binding. |
| AF-CRM-P2-008 | Medium / fixed | `docs/sdd/architecture/data-model/crm.md` still claimed no quote tables existed; the feature map described only clients. | Updated the actual twelve-table inventory, composite keys, quote directory tree and relationship diagram. The SDD target-model table now explicitly distinguishes future rows/fields from implemented schema. |

Price preview, one-off/PPE precedence, token expiry and source drift, immutable text edits, request replay, revision races, scoped SQL paging, foreign-owner denial and API transport were checked. The Host adapter remains the Catalog integration boundary; Crm has no Catalog persistence or internal HTTP dependency. Detailed snapshot reads load the bounded saved graph; list reads project and page in SQL.

Fresh commands/results are recorded in [audit verification evidence](../evidence/p2-audit-refactor-2026-10-26.md). Generated migration/model files remain under `Persistence/Migrations`; no file was split solely because of its line count.

## Limits

No live schema migration, deployment, import, PDF, ready-state tax wording or frontend quote integration was performed. The backend quote contract is local and uncommitted, so no immutable SHA is available for the frontend generated-client handoff.
