# Implementation sequence and phase gates

**Progress:** EN-001/TS-001/TS-002 backend client slice and EN-002/TS-003/TS-004 local backend draft-quote slice are implemented with focused evidence. FE-003/FE-004 and integrated P2 acceptance remain open until an authorized immutable backend revision can be pinned. Backend `TS-*`/`EN-*` IDs are local to this specification; frontend uses `FE-*` IDs in its counterpart for clarity. Each task below links to a separate executable task document. Within an authorized feature/phase, proceed through ready dependencies without asking permission per file; unresolved business gates stop only their dependent work.

| Phase | Backend tasks | Frontend tasks | Exit outcome |
| --- | --- | --- | --- |
| P0 — baseline/contracts | EN-001; EN-002/003 as dependent discovery | FE-EN-001 baseline/Nx/permission readiness | Actual existing Clients menu, scope, schemas and contract sequence confirmed |
| P1 — client master | TS-001, TS-002 | FE-001 client data-access, FE-002 list/detail/contact/merge | Existing Clients entry reads/writes one Crm master with access/dedup rules |
| P2 — draft quotes | EN-002, TS-003, TS-004 | FE-003 quote boundary, FE-004 editor | Select customer and Catalog articles/PPE, compare options and reopen immutable server-priced draft |
| P3 — history and actions | TS-005 | FE-005 history/reprice/copy/transfer/archive | Explicit reprice creates new version; old/sent prices and ownership history safe |
| P4 — custom sewing | EN-003, TS-006, TS-007, TS-008 | FE-006 custom editor, FE-007 files/templates/suggestions | Private versioned custom configurations/alternatives, real private files, reusable selections |
| P5 — ready/PDF | EN-005, TS-009, TS-010, TS-016 | FE-008 issuer/ready/PDF, FE-011 first-contact | Verified seller/tax/numbering and public projection; rendered document parity |
| P6 — delivery/production package | EN-004, TS-011, TS-012 | FE-009 explicit send/history/package | Truthful outbox states and pinned production artifact, no automatic external Workshop integration |
| P7 — archive migration | EN-006, TS-013, TS-014 | FE-010 reconciliation UI and integrated acceptance | Controlled import with row/file accounting, idempotency, restore evidence |
| P8 — full acceptance | TS-015 | FE-010 | All scenarios/evidence, architecture and feature maps match delivered scope |

P7 source-completeness discovery may start early; its write/activation depends on the relevant implemented model. Missing old files/provider configuration must not force stalled client/draft development. If a phase is deferred, record it explicitly: do not mark full block complete.

## Atomic task registry

| Task | Responsibility | Depends on |
| --- | --- | --- |
| [EN-001](readiness/en-001-crm-foundation-and-client-contract.md) | Crm foundation and client contract | None |
| [TS-001](clients/ts-001-customer-domain-and-persistence.md) | Customer domain and persistence | EN-001 |
| [TS-002](clients/ts-002-client-contact-use-cases-and-api.md) | Client/contact use cases and API | TS-001 |
| [EN-002](readiness/en-002-price-basis-and-custom-arithmetic-reconciliation.md) | Price-basis and custom arithmetic reconciliation | EN-001 |
| [TS-003](quotes/ts-003-server-quote-price-preview-adapter.md) | Server quote price preview adapter | EN-002 |
| [TS-004](quotes/ts-004-quote-drafts-and-immutable-saves.md) | Quote drafts and immutable saves | TS-002, TS-003 |
| [TS-005](quotes/ts-005-version-history-and-lifecycle-actions.md) | Version history and lifecycle actions | TS-004 |
| [EN-003](readiness/en-003-private-storage-and-document-adapter-readiness.md) | Private storage and document adapter readiness | EN-001 |
| [TS-006](custom-sewing/ts-006-custom-drafts-calculation-and-pinned-versions.md) | Custom drafts, calculation and pinned versions | TS-003, TS-004 |
| [TS-007](custom-sewing/ts-007-private-file-lifecycle.md) | Private file lifecycle | EN-003, TS-006 |
| [TS-008](custom-sewing/ts-008-reusable-templates-and-model-suggestions.md) | Reusable templates and model suggestions | TS-006 |
| [EN-005](readiness/en-005-issuer-numbering-and-tax-policy-values.md) | Issuer, numbering and tax policy values | EN-002 |
| [TS-009](documents/ts-009-versioned-issuer-and-template-settings.md) | Versioned issuer and template settings | EN-003, EN-005 |
| [TS-010](documents/ts-010-ready-validation-and-pdf-artifacts.md) | Ready validation and PDF artifacts | TS-005, TS-007, TS-009 |
| [EN-004](readiness/en-004-delivery-provider-readiness.md) | Delivery provider readiness | EN-003 |
| [TS-011](documents/ts-011-explicit-email-outbox-and-history.md) | Explicit email outbox and history | TS-010, EN-004 |
| [TS-012](custom-sewing/ts-012-pinned-production-package-export.md) | Pinned production package export | TS-007, TS-010 |
| [EN-006](readiness/en-006-complete-history-and-retention-evidence.md) | Complete history and retention evidence | None |
| [TS-013](migration/ts-013-staging-mapping-and-dry-run-pipeline.md) | Staging, mapping and dry-run pipeline | TS-002, TS-007, EN-006 |
| [TS-014](migration/ts-014-controlled-historical-write-and-recovery.md) | Controlled historical write and recovery | TS-013, TS-005, TS-010, TS-011 |
| [TS-015](release/ts-015-block-acceptance-and-architecture-handoff.md) | Block acceptance and architecture handoff | TS-002, TS-005, TS-008, TS-010, TS-011, TS-012, TS-014, TS-016; frontend CRM-001 tasks |


## First-contact task

| Task | Responsibility | Depends on |
| --- | --- | --- |
| [TS-016](documents/ts-016-first-contact-proposals.md) | First-contact seasonal layout and owned selections | TS-008, TS-010; TS-011 for send |

## Handoff/checkpoint for every API slice

1. Agree only the slice's canonical schema/operation IDs and expected errors. Add focused failing behavior/contract tests; no fabricated “existing” operation.
2. Implement Domain/Application/mapping/transport boundaries; run focused tests, migration constraints where applicable and code responsibility audit. Freeze committed contract+implementation to a clean backend SHA under the project's user-authorized commit workflow.
3. Frontend syncs/generates that exact SHA, validates responses/maps at data-access, then connects the existing menu route and screens. A future SHA is recorded only after it exists, never a placeholder presented as provenance.
4. Test integrated scenario, privacy/revision errors and narrow/wide UI. Update task evidence/traceability and actual feature map. Incompatible deployed UI/API order needs a documented compatibility window before activation.
5. Keep current source snapshots, Catalog imports/pricing and unrelated working changes intact. No schema/import job or real email executes merely because an SDD was written.
