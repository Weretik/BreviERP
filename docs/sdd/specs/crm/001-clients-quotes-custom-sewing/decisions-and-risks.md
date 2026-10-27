# Decisions, prerequisites and readiness

## Specified decisions

| ID | Decision and basis |
| --- | --- |
| D-01 | Extend existing Crm and existing Clients/menu IDs. Current code has scaffolding/menu placeholders, so this is an extension, not replacement of a working client API |
| D-02 | Workbook fields/pricing basis are primary source; PostgreSQL holds all operational data. v211 supplies evidenced workflow and legacy records missing from header-only tabs |
| D-03 | Catalog/Reference remain owners of products, inputs and formulas; Crm owns snapshots and private configurations. No formula duplication in API, UI, PDF or import |
| D-04 | Typed long internal IDs + Guid public IDs + separate immutable number; exact source identity maps avoid collisions |
| D-05 | Append-only versions, optimistic revision and requestId idempotency; manual PPE price requires reason, sewing is composition-driven |
| D-06 | New ready quotes require resolved active client/recipient; incomplete old client identity survives as a frozen legacy snapshot with reconciliation warning |
| D-07 | Source draft/ready/sent states retained; archive separate. Editing after issue creates draft without mutating issued history |
| D-08 | Existing backend price basis and rounding remain authoritative; 452/453 include the approved operation; 724/931 restrictions are respected |
| D-09 | Private custom files/document projections are separate from public Catalog media; imported sends never enqueue new emails |
| D-10 | New-flow rollout and archival migration have separate gates. Full source/history gaps do not block basic client/draft functionality |
| D-11 (2026-10-26) | User confirmed draft quote monetary basis: use the current three Catalog/PPE prices in UAH without an extra VAT calculation; freeze source identity/version in each immutable snapshot. Customer-ready PDF tax wording remains the separate EN-005 decision. Applies to EN-002/TS-003/004 and FE-003/004. |

## Gates: resolve only before dependent tasks

| Gate | Known gap / required result | Blocks | Does not block |
| --- | --- | --- | --- |
| EN-001 | Confirm target Crm schema/DI, role policy and client contract; target database metadata recheck | First client schema/API | Documentation |
| EN-002 | Draft UAH/no-extra-VAT decision is confirmed in D-11. Freeze quote price-basis port, semantic tier boundaries, numeric precision and calculation token lifetime; reconcile custom additions with workbook engine | Quote price adapter/custom arithmetic/ready financial output | Client master and draft text shell |
| EN-003 | Choose existing usable private storage/renderer adapters, confirm MIME/size and retention linkage with a real round-trip | Private uploads and valid customer PDF | Clients and text/price snapshots |
| EN-004 | Confirm mail provider, sender identity, credentials and delivery reconciliation capability; no inherited OAuth secrets | Real sending | PDF/download and delivery UI with disabled send |
| EN-005 | [NEEDS CLARIFICATION: actual issuer requisites, document number format/scope and approved tax/currency wording]. Source tabs have no values. Use verified existing organizational settings if available; otherwise obtain actual values before activation | Ready/customer-ready PDF and number issuance | New clients and saved drafts |
| EN-006 | Obtain complete consistent historical export/file bytes or documented accepted archive gaps; set retention and source owner mappings | Historical migration completion/production retention activation | New clients, quotes, calculation and document development |

Gates are planned tasks with evidence, not claims these decisions are already approved/configured. Default-deny CRM User role and specified Manager scope can be implemented through current policies; any broader sharing is a separate accepted policy change. EN-002 must not infer sales quantity thresholds from legacy labels alone. Unknown legacy individual payloads are classified/quarantined; they do not become catalog products automatically.

## Risks and mitigation

Partial quote/version exports can produce believable but false history: quarantine missing graphs, compare source and target counts/hashes, never synthesize missing versions. Client merge may affect ownership/privacy: Admin-only atomic merge, exact source identity checks, immutable recipient snapshots. Current Product legacy price fields may be zero/deprecated: consume published Article/PPE inputs, never use legacy fallback. User-entered images/HTML can leak private data: validated file refs and server projection/escaping. Outbound delivery may duplicate after provider crash: stable request/outbox keys and explicit uncertain-state reconciliation.

No business approval, implementation completion, import success or production release is inferred from writing this SDD. Record any later decision here with date, actor, affected requirement/task and required regression.
