# P6 outbox and OAuth functional audit — 2026-10-30

## Scope

User requested functional audit and responsibility refactoring of the immediately preceding TS-011 / EN-004 implementation. One agent reviewed Domain state, enqueue/read/claim/finalization, current ACL/idempotency, pinned public message/PDF, worker/DI, Gmail refresh/profile verification, public API/contract, new schema and local consent helper. Previous P5, production ZIP implementation and unfinished frontend are not changed here. Applied migrations are preserved; no schema, public HTTP route/payload or real delivery activation changes are needed.

## Findings and fixes

| ID | Severity/status | Exact path and evidence | Impact, owner and completed action | Verification |
| --- | --- | --- | --- | --- |
| AF-P6-004 | P2 / fixed | `Crm.Infrastructure/Crm.Infrastructure/Persistence/Delivery/QuoteDeliveryStore.Claims.cs`: LIMIT20 was applied before recipient-specific readiness. Twenty older ineligible addresses hid an eligible sandbox message indefinitely. | Outbox claim: expose optional configured recipient restriction on the internal identity port and apply its parameterized SQL filter before LIMIT/locks. Full readiness is still checked before every claim. No state is changed for blocked recipients. Dependencies: configured identity and existing readiness policy. | New PostgreSQL case seeds21 ordered messages, expects the last eligible case-insensitive sandbox address to be claimed and20 others remain queued. Red failed exactly with expected non-null/actual null; final result below. |
| AF-P6-005 | P3 / fixed | `tools/gmail_oauth/consent.py` (formerly `tools/gmail-oauth-setup.py`): handler swallowed verified `consent_declined` and kept waiting until timeout. | Consent receiver records a terminal bounded error only after callback state validation, closes the local receiver and immediately returns the safe decline error. A forged state remains nonterminal. Dependencies: existing callback validator. | Offline Red:1failed/7passed, with a second callback wait after denial. Green:8passed, including valid decline and hostile-state-then-valid-consent flows. No browser/network/listener used. |
| AF-P6-006 | P3 / refactored | `tools/gmail-oauth-setup.py` combined CLI/config/user-secrets orchestration, input/grant validation, HTTP and browser/loopback handling. | Setup tool split into explicit `gmail_oauth/validation.py`, `transport.py`, `consent.py`; existing command-line entry remains. No generic utilities or extra library dependency. | Same8 offline tests plus entrypoint --help and source syntax checks; command and secure stdin provisioning retained. |
| AF-P6-007 | P3 / refactored | `Persistence/Delivery/QuoteDeliveryStore.Processing.cs` combined durable claim and result completion transactions. | Separate `QuoteDeliveryStore.Claims.cs` and `QuoteDeliveryStore.Completion.cs`; the same scoped store and existing transaction boundaries remain. Main store owns enqueue/replay and Reads owns authorized projection/pagination. | Focused delivery/OAuth integration regression covers claims, overlap, stale finalizers, archive, version-specific sent state, HTTP and hosted worker. |

## Retained architecture and functional checks

Domain owns lease/outcome invariants and exact-current-version sent transition. Application processor chooses submit versus read-only reconciliation; Host creates a scope per tick. Infrastructure commits the claim before IO and finalizes under a fresh lease token. Immutable pins/events/request receipts remain append-only and same-quote version/PDF constraints remain enforced. Current owner/Admin authorization precedes request replay comparison; a new request key cannot bypass an unresolved same-version/recipient attempt.

Reviewed private data projection, strict DTO binding/status mapping, bounded history pagination before positional projection, sender/provider pins, no automatic unknown resend, parameterized SQL, immutable version/template/PDF source, fail-closed configuration and public logging. OAuth still validates profile before returning a token; fixed endpoints, no redirects/retries, bounded response and cancellation/cache lock remain in dedicated runtime files. The helper only runs on explicit user invocation and still saves Enabledfalse/Sandboxtrue via stdin user-secrets. No credential or live address was added to source/tests/docs.

No further split was justified in the short coherent Application/Domain/host files. No new independent module/database context or abstraction layer was introduced. Remaining EN-004 consent/live-test and frontend FE-009 are recorded external/delivery boundaries, not claimed completed by this audit. No load test or actual Google round-trip was performed.

## File responsibilities

```text
Persistence/Delivery/
  QuoteDeliveryStore.cs                 enqueue transaction, current ACL and replay
  QuoteDeliveryStore.Reads.cs           bounded authorized history/detail projection
  QuoteDeliveryStore.Claims.cs          candidate scope and committed attempt/lease
  QuoteDeliveryStore.Completion.cs      safe outcome and version-specific sent finalization
tools/
  gmail-oauth-setup.py                  explicit CLI and secure settings orchestration
  gmail_oauth/validation.py             pure input/grant/state validation
  gmail_oauth/transport.py              fixed-endpoint bounded HTTP JSON
  gmail_oauth/consent.py                explicit browser and loopback consent lifetime
  tests/test_gmail_oauth_setup.py       offline consent/validation regression
```

## Verification

Red for AF-P6-004: safe local wrapper filter `FullyQualifiedName~Sandbox_recipient_after_twenty_ineligible_messages_is_still_claimed`, exit1,1failed. A no-build normal-verbosity diagnostic confirms the intended expected non-null/actual null assertion, not setup/compile failure. It uses only a unique disposable PostgreSQL database.

Red for AF-P6-005: `python -B tools/tests/test_gmail_oauth_setup.py`, exit1,1failed/7passed because the old consent receiver requested another callback after a validated refusal. After fix/split, the same command exited0,8passed. `python -B tools/gmail-oauth-setup.py --help` exited0 and opened no browser or listener.

Final integration/architecture/diff/link results are appended after the unified run. Unit state/transition behavior and schema were not changed; their preceding four-case unit and migration evidence is reused rather than rerun. No commit/push, application DB migration, live OAuth setup or real send occurred during this audit.

Final Green/regression: E:/Development/p6-delivery-db-check.ps1 -Mode test, filter FullyQualifiedName~QuoteDelivery|FullyQualifiedName~GmailOAuth, exit0: **69 passed,0failed,0skipped**, duration36seconds. Includes eight PostgreSQL outbox cases and all HTTP/composition/OAuth/readiness/adapter cases. CrmDependencyTests --no-build --no-restore, exit0: **1passed**. Offline helper **8passed**. All4 Python production sources parse, changed local documentation links resolve, and final git -c core.safecrlf=false diff --check exits0. No application migration or real send was run.
