# P6 outbox and OAuth checkpoint — 2026-10-30

## Scope and outcome

User requested the next two P6 items using multiple agents. Delivered TS-011 backend queue/history/worker and the EN-004 OAuth code/setup slice. Three contributors owned outbox, OAuth and composition tests; the coordinator refined the existing [design](../design/delivery-outbox-p6.md), integrated DI/host/contracts and reviewed one combined state. No frontend changes, Git commit/push or deployment were performed. Previous P5/P6 working changes were preserved.

Queue is explicit and authorized: the current issued version, intact private PDF, immutable email version, expanded public text, recipient, original actor and provider/sender are pinned with one transactional request receipt. Current ACL applies before replay hash comparison. New archived requests fail. Safe paged/detail API exposes metadata/status without PDF bytes, expanded body, tokens or credentials. Canonical [contract](../../../../contracts/crm/quote-deliveries.openapi.yaml) defines enqueue202 and history/detail200, with readiness503.

Worker commits a lease/attempt before HTTP; overlapping claims cannot submit twice. Expired attempts and Unknown use read-only reconciliation with a five-minute backoff; no automatic repost or retry endpoint. Stale lease finalizers are rejected. Confirmed success records one exact-version event and changes the quote to sent only when that version remains current/issued/ready. A newer draft stays draft. Queued archived records are failed without send; uncertain prior sends can still reconcile.

Delegated OAuth source refreshes using fixed endpoints and returns a token only after verifying the Gmail profile matches configured sender. Singleton cache serializes refresh and stops before token expiry. HTTP redirect/retry behavior is disabled. Empty configuration still resolves the dependency graph and blocks delivery. No token is stored in CRM tables or API responses.

## Verification

- Safe wrapper `E:/Development/p6-delivery-db-check.ps1 -Mode test`: **68 unique integration cases passed across focused runs**, with no remaining failures/skips. The unified run passed67; after adding the unresolved-attempt/new-key guard, seven PostgreSQL cases were rerun and passed (one new case), reusing unchanged adapter/HTTP/composition evidence. Filter `FullyQualifiedName~QuoteDelivery|FullyQualifiedName~GmailOAuth` includes fake HTTP OAuth/adapter/readiness, PostgreSQL outbox, API/contract and real hosted-worker composition. Unique fixture databases apply/reapply the current CRM migration graph; no application data is truncated.
- Unit filter `FullyQualifiedName~QuoteDeliveryStateTests`: **4 passed**, covering one-submit/lease expiry/unknown, terminal finalization and exact-current-version sent behavior.
- OAuth contributor Red stub:15failed/1passed; implemented initial16green; final21 cases included in unified verification. Outbox initial compilation/PG corrections, if any, are described in contributor evidence rather than represented as a Red behavior proof.
- Local helper offline tests: **6 passed**; syntax validation passed. Tests do not open a browser/listener, call Google or write real user-secrets.
- Composition tests: **3 passed within integration total**: ValidateOnBuild/Scopes with disabled config and actual hosted worker using only submit or only reconcile.

Final architecture/diff/link results are appended below. No real Google consent/send/reconcile acceptance, frontend interaction or load test was performed.

## Schema and local upgrade

Crm owns `20261030163554_AddQuoteDeliveryOutbox`: four new tables (immutable QuoteDeliveries, mutable QuoteDeliveryStates, append-only QuoteDeliveryEvents and QuoteDeliveryRequests), composite scope FKs, actor/request uniqueness and one-confirmed-success index. Existing QuotePdfArtifacts receives an additive same-quote/version/ID alternate key. No applied migration was edited. UPDATE/DELETE triggers protect immutable pins/events/receipts.

Verified application target **localhost / brevidb**. Before upgrade, `pg_dump --format=custom` created **E:/Development/p6-20261030-before-delivery.dump**, **770311 bytes**; `pg_restore --list` succeeded with **627 lines**. CRM-only Host.Seed plan/apply reported this single pending migration and **pendingAfter=[]**. No unrelated seed/import/activation ran. Disable dispatch before recovery; after use, do not destructively Down immutable send evidence. Use verified coordinated backup/forward repair.

## User setup still required

User confirmed no Google OAuth client exists and requested instructions. [Gmail setup runbook](../../../../operations/crm-gmail-oauth.md) explains project/API, consent/test user/scopes, Desktop client JSON and explicit local PKCE helper. The helper saves credentials through stdin to Host.Api development user-secrets, verifies grant/account and leaves Enabledfalse/Sandboxtrue. It was not run against the user's Google account.

EN-004 remains **partial** until the user creates the client, completes consent and a controlled test send/reconcile succeeds. Runtime sending remains disabled and no real message was sent. Structural configured-credential readiness is not a claim of consent or delivery. TS-011 backend is locally verified; full SC-D03 live acceptance remains gated by EN-004. FE-009 and frontend registry/production UI need a separately authorized immutable backend commit and consumer synchronization.

Final CRM dependency architecture check exited0: **1 passed**. Final diff check exited0 and updated local documentation links resolved. Existing NU1903 warning for Microsoft.OpenApi.YamlReader2.7.5 remains outside this feature change.

Contributor detail: [TS-011 chronology and fixes](ts-011-outbox-2026-10-30.md), [OAuth proof and setup boundaries](en-004-oauth-2026-10-30.md). The first57-case run had five fixture seed failures and one real oversized-OAuth-response failure; subsequent verification also found/fixed PostgreSQL positional-record paging translation. These failures are resolved in the recorded final state. The final extra pending-attempt guard was tested Red (expectedConflict/actualOk) then Green with seven PostgreSQL cases.

Follow-up [functional audit/refactoring](../code-audit/p6-outbox-oauth-audit-2026-10-30.md) records the sandbox candidate-selection and consent-refusal fixes, modular helper and fresh verification.
