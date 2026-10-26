# Verification and acceptance

**Current evidence: documentation/source analysis only.** No Crm runtime scenario is marked verified. Follow [testing rules](../../../standards/testing-rules.md); use existing `tests/UnitTests`, `tests/IntegrationTests`, `tests/ArchitectureTests` projects, inspect actual targets/filters first. Test database must be isolated, never run destructive tests on the user's working database. Use synthetic customer/contact data.

## Focused test matrix

| Risk | Level | Required evidence |
| --- | --- | --- |
| Customer/contact validation, primary selection, deactivation/merge | Domain + persistence for unique/race constraints | SC-C01–05; exact source identity collision, shared email allowed, atomic merge and frozen recipients |
| Quote state/versions/visibility/override rules | Domain/Application | SC-Q01–07; 0/negative/missing prices distinct, reason mandatory, body totals ignored/rejected, sent version immutable |
| Persistence/concurrency/idempotency | PostgreSQL integration | Two independent transactions saving same revision; one 409, no partial rows; same-key retry stable, changed hash conflict |
| Contract/access | API integration + OpenAPI/schema checks | Every implemented operationId, paged value/pagedInfo, 401/403/404 policy, guessed IDs/foreign files, CSRF and malformed data |
| Common price adapter/custom arithmetic | Unit plus approved golden fixtures | Source input/settings versions, exact decimal rounding stages, quantity-tier boundaries around each current cutoff; no extra VAT/assembly/application charge |
| Snapshot independence | Application + persistence | Change live client/cost/settings; read/text-edit/PDF retain V1; only explicit reprice produces changed V2 |
| Private file/PDF | Adapter integration + rendered document check | MIME spoof, over-limit, unknown owner, missing bytes; extracted text and inspected rendered pages exclude every hidden/private marker |
| Email/outbox | Integration with fake provider | Worker overlap, retry/crash before/after provider response, unknown outcome reconciliation; no send from imported history |
| Imports/rollback | Integration + dry-run reports | SC-M01–03, idempotent rerun, source hash conflict, full row classification, FK graphs, document/file parity and restore rehearsal |
| Boundaries | Architecture tests + scoped code audit | No foreign EF/Domain import, one primary responsibility/type per file, scenario-local specs, no duplicate formulas or generated DTO edits |

Golden fixtures must include Catalog sewing and PPE, modified norms, three visible/hidden tiers, manual PPE override, one-off PPE, two custom fabric variants, application internal cost vs sale amount, delivery/packaging and blocker inputs. Reuse approved engine fixtures; create only missing cases. Cover 452/453 normal composition; 724 and 931 blocked for new selection without modifying history. Record fixture source/hash and any accepted source-vs-engine difference.

## Manual product acceptance, per vertical slice

1. Existing Clients entry opens the new list; create client/contact, reopen and search. No duplicate menu/master or supplier contamination.
2. Create a quote for that client; add sewing Article, PPE and variants; edit public text/photos/visibility and save. Reload preserves order, tier labels/amounts and author.
3. Change a current material price/settings in test data; old quote remains unchanged. Preview/confirm reprice and compare versions. Try simultaneous save and transfer between two managers; verify conflict UI and changed owner scope.
4. Create private custom sewing with two fabrics, operations, application/artwork and private files; add a pinned version to a quote, then change the draft. Quote remains fixed; template/suggestion application is explicit.
5. Configure verified issuer/tax/numbering. Mark a quote ready, generate PDF, inspect rendered pages and text, download again after changing live masters. Private note/cost/hidden options never appear; Ukrainian labels and legible pagination/media hold on a multipage document.
6. Explicitly send to an approved test recipient; failure/unknown/retry states remain truthful. Confirm version-specific send history; copying/downloading does not send or reuse the number.
7. Run historical dry-run, fix/reconcile scope, import repeat and rollback rehearsal. Do not sign off archive completion with unclassified gaps.

At each task record: scenario/test names, actual command, test database/runtime prerequisites, Red failure for missing behavior, Green, refactor and focused regression result, contract SHA, and manual evidence where applicable. Documentation-only tasks are exempt from Red tests and use link/content/diff validation. Do not run a full suite/build as an unrequested blanket acceptance step; choose checks needed for changed boundaries.

First-contact acceptance (SC-I01–03): general layout has two seasonal sets/eight models, seasonal layout four, configured featured model and one fabric per slot, valid metadata/product links/large tier. Verify own Manager template revision/archive, foreign denial, current prices on new reuse and immutable old PDF; inspect rendered seasonal content and ordinary send history.

## Definition of done and review

All required task checkpoints pass; applicable EN gates resolved; traceability links evidence; backend and frontend use a clean matching contract revision; architecture audit findings fixed/verified; actual feature/data maps updated; restore/rollout documented. Partial historical migration and deferred provider setup have explicit statuses and cannot be called full completion. User manual acceptance remains a separate recorded outcome, not inferred from automated tests.
