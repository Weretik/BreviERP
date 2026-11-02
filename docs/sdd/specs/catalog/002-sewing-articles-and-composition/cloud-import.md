# Dedicated cloud catalogue import — 2026-11-02

Status: implemented and locally verified; cloud deployment/import not executed. [Evidence](cloud-import-evidence.md). Extends the existing initial import; no new business module, HTTP endpoint, schema or parallel product master.

## Rules and scenarios

R-CI01: A separate Cloud Run Job brevierp-catalog-import uses the Host.Seed image with explicit catalogue arguments, one task, no retries and no normal migration/seed fallback. API deploy and recurring reminders are unchanged.
R-CI02: A frozen version1 workbook includes all available sheet values/formula evidence and model website URLs. Native source and contact-bearing reports remain outside Git and in a private bucket. No source credentials/accounts/tokens are imported.
R-CI03: The default local-only guard remains. An explicit target approval pins host, database and a 64hex SHA256. All three are validated before any remote connection/write; source hash is verified before opening PostgreSQL. No hardcoded credentials or generic allow-remote switch.
R-CI04: Existing catalogue validation, transaction/trial rollback, source mappings, same-hash replay/reconciliation and changed-snapshot/nonempty rejection remain unchanged. A different source is an initial import only into an empty Product catalogue. This feature does not implement a destructive replacement or incremental merge.
R-CI05: The deployment wrapper defaults to a plan without cloud mutations; execution is explicit. validate is the default mode; apply requires an explicit backup reference. Source is content-addressed in a private Cloud Storage bucket, mounted read-only; reports use a distinct writable mount and unique path. Database credentials come from an existing named Secret Manager secret. No connection string is printed.
R-CI06: Imported operational scope is models/PPE, Articles, suppliers/fabrics/accessories/operations, materials/routing and provenance. Other sheet families remain source evidence or their existing owning jobs. Source arithmetic is not automatic price publication; D1 CRM historical writes retain their separate ownership and integrity gates.

SC-CI01: Prepare native XLSX, preserve typed cell positions/formulas and extract real model hyperlinks; reject invalid/oversized input and existing output. Produce v1source and SHA.
SC-CI02: Default deployment wrapper emits an inspectable plan and performs no cloud call. Explicit execution uploads verified source, creates/updates only the dedicated job and runs it with the selected mode.
SC-CI03: Wrong host/database/hash or partially specified target approval fails before database access. Default remote target without approval fails; old local call remains compatible.
SC-CI04: Initial trial writes and reconciles then rolls back; apply persists; repeat of same source reconciles without duplicates. A changed source against existing products fails. Reports survive job exit in the private output bucket.

## File ownership and verification

- deployment/prepare-catalog-source.ps1: native XLSX to frozen v1source (new).
- deployment/import-catalog.ps1: plan/deploy/run one dedicated Cloud Run Job (new).
- src/Jobs/CatalogImport/Execution/CatalogImportTarget*.cs: explicit target/source approval (new); CatalogImportJob validates without altering persistence semantics.
- src/Bootstrapper/Host.Seed/Jobs/CatalogImport/: CLI/config binding and report output (existing slice extended).
- tests/UnitTests/CatalogImportTargetTests.cs and host command tests: rejected/matching approval behavior.
- Existing CatalogImportPersistenceTests: isolated PostgreSQL trial/apply/replay/reject regression, no shared database.

Use focused valid Red/Green, script parser/plan checks, real available-source preparation and planner validation, then focused PostgreSQL regression. Do not run a full suite or execute cloud deployment merely to prepare this implementation. Cloud activation requires an actual configured private bucket, database secret, service account and backup; do not invent them or report a local test as a live import. Recovery is the pre-import target backup, not destructive Down. No automatic deployment-trigger import.
