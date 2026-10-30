# TS-016 remaining backend gates — 2026-10-30

Scope: the three explicitly assigned P5 follow-ups: local PostgreSQL migration/persistence, introduction application replay, and canonical OpenAPI. This does not mark P5 or the frontend complete.

## Database

Verified target: loopback localhost, database brevidb. Host.Seed CRM-only plan reported exactly `20261029173840_AddIntroductionTemplatesAndFeaturedMapping` and `20261029174047_AddIntroductionQuotePublicSnapshot`. Before apply, pg_dump custom backup `E:/Development/p5-20261030-before-crm.dump` was created (734820 bytes) and pg_restore --list verified 547 listing lines. CRM-only apply returned those two pendingBefore and empty pendingAfter. No seed/import/pricing activation ran.

Safe local wrapper: `E:/Development/p5-db-check.ps1` modes plan, backup, apply, test; it verifies local brevidb and keeps credentials out of output. Recovery is the coordinated verified backup, not destructive Down after data use. Additive migration order: template/mapping tables first, nullable QuoteVersions jsonb snapshot second.

Final PostgreSQL command selected IntroductionTemplatePersistenceTests, IntroductionQuoteReplayTests and IntroductionSnapshotPersistenceTests in IntegrationTests.csproj, with --no-restore and a build. **8/8 passed, zero skipped**, after the final access-before-hash change. Fixtures create unique brevi_crm_test UUID databases, migrate the empty schema twice and drop only those databases. This verifies empty migration/idempotent reapply, owned revisions/archive/count, append-only triggers, repeated featured activation, current-price/mapping changes, pinned jsonb history and apply replay/concurrency/access. Applying to brevidb verifies the existing-schema upgrade path.

## Replay and contract

Introduction application uses a stable versioned intent discriminator and the existing CreateQuote journal. Replay is checked before live template/catalog access; current ownership is checked before payload conflict. Same successful request survives template archive/source drift; changed payload conflicts; transferred access returns not-found; concurrent successful requests create one quote/receipt. No new table or migration was added for this fix. Ordinary CreateQuote receipts retain their old serialization when the new intent is null. Receipts from earlier unfinished introduction code cannot safely be reinterpreted and return conflict; no guessing/backfill is performed.

Canonical `crm/introduction-templates.openapi.yaml` contains nine implemented operations, with six path refs in aggregate openapi.yaml. Strict request shapes, numeric response enums, value/pagedInfo, role/ownership, validation/409 and source/replay behavior are documented. New backend is not yet an immutable frontend consumer revision; no commit or push was authorized in this package.

## Verification and remaining work

Integrated focused unit/contract/projection selection: **28/28 passed**. Final PostgreSQL selection: **8/8 passed**, zero skipped. Unit restore metadata emits the existing NU1903 advisory for Microsoft.OpenApi.YamlReader 2.7.5; package changes are outside this slice.

Remaining P5: authorized immutable backend handoff, FE-011 generated transport/route/pickers/template actions, actual seasonal PDF visual/privacy acceptance and ordinary FE-008 manual PDF acceptance. Explicit email remains P6. Historical issued PDF acceptance is not established by jsonb round-trip alone.
