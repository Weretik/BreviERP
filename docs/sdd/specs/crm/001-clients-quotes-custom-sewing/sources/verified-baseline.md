# Verified baseline and source hierarchy

**Evidence date:** 2026-10-26. Evidence below is bounded to these sources, not to a production deployment.

## Current Brevi code and local database

Backend baseline commit: `95a22055e7a0e841bfcb5cdbfc861123e1f916e0`. `src/Modules/Crm/Crm.{Domain,Application,Infrastructure,Api}/Crm.<Layer>/` contains project/assembly scaffolding, no client/quote business implementation. The repeated project directory is real; do not flatten it incidentally. [Module map](../../../../architecture/overview/modules.md) confirms structural CRM only. A read-only `information_schema.tables` query on the configured localhost database found no `crm` tables or business table names matching customer/client/counterparty/quote/proposal. This is local metadata evidence only; recheck the deployment target before creating migrations or importing.

Frontend baseline commit: `e5ac604c1856ecb0590bb20693fc8d6c8d2d586c`. In `apps/admin-react/src/app/router/legacy-menu.ts`, existing IDs `clients`, `quotes`, `quote-archive` are in `service-manager`. They have labels but no paths. `navigation-config.ts` enables paths only when registered; `admin-route-definitions.tsx` has no client/quote route. Preserve those entries and attach routes; do not create another CRM menu. Visual-reference assets are not implemented screens. The contract snapshot was pinned to backend `0529daf910b837eb808a662695706dda4beaf833`; no CRM operation is in that baseline.

Catalog/Reference already own articles, materials, operations, settings and versioned prices. The [pricing specification](../../../catalog/003-sheet-pricing-parity/README.md) and actual contract/code are authoritative for reuse. Quote snapshots and private custom composition are additions, not replacements of those masters.

## Live workbook

Workbook: [Brevi — новые справочники и расчёты](https://docs.google.com/spreadsheets/d/1BMl9jZLprY-15TfErBi9pbv9e4TVq8DUdjlZ4HvCxRE/edit). Metadata reports 22 tabs, locale `uk_UA`, time zone `Europe/Kiev`. Relevant exact reads were `A1:T1000` for each tab below, covering its configured 1,000-row grid. Every response contained only one header row: **zero populated quote, item, option, calculation or issuer-setting records** in those five tabs at this date.

| Tab | sheetId | Header fields | Meaning |
| --- | --- | --- | --- |
| КП | 89730105 | 13 | Quote header and recipient display fields |
| Позиции КП | 2123354410 | 13 | Items and client-visible presentation |
| Варианты позиции | 1164053440 | 13 | Alternatives, three tiers and override provenance |
| Расчётный состав варианта | 892515517 | 12 | Original/edited calculation inputs and results |
| Настройки КП | 913215478 | 11 | Seller/issuer details, not customer bank details |

There is no separate Clients tab in this workbook metadata. `Постачальники` is a supplier directory. The workbook is therefore the primary field and pricing-reference source; it cannot supply nonexistent client/quote records. Do not claim those five tabs contain history or approved issuer/tax values. All headers map in [the field map](google-field-map.md).

## Calculator export and partial database

Source artifact: sanitized Calculator source export **v211**. Key evidence paths within that artifact: `db/schema.ts`, `db/finance-schema.ts`, `lib/quote-api.ts`, `lib/quote-access.ts`, `lib/custom-sewing.ts`, `lib/custom-sewing-store.ts`, `lib/custom-sewing-suggestions.ts`, `app/api/proposal-settings/route.ts`. These are source-provenance names, not runtime dependencies or local filesystem links. Older research notes can disagree; v211 source wins for observed legacy behavior, the workbook wins for source field structure/pricing basis, and Brevi rules govern target architecture and validation.

The source manifest explicitly has `full_backup=false` and `transaction_consistent_snapshot=false`. The database artifact's manifest reports these exported rows. Counts are not a claim of a complete or transaction-consistent source backup:

| Dataset | Exported rows | Limitation |
| --- | --- | --- |
| counterparties | 958 | All available rows read; role filter required, not 958 proven customers |
| quotes | 153 | Headers read; child completeness not established |
| quote_items | 179 | Partial/truncated; last page not reached |
| quote_versions | 31 | Partial/truncated; last page not reached |
| quote_change_log | 595 | Partial/truncated |
| quote_search_index | 118 | Partial; rebuildable, never a history substitute |
| quote_transfers | 8 | All available rows read |
| quote_email_sends | 96 | All available rows read; not authorization to send again |
| quote_ppe_price_overrides | 21 | All available rows read |
| custom_sewing_drafts | 2 | All available rows read |
| custom_sewing_versions | 12 | Partial |
| custom_sewing_files | 30 | Metadata only; blob bytes not supplied by table rows |
| proposal_settings | 1 | Partial; cannot approve legal settings from it |

Before a historical import, freeze the artifact version, checksum files, record extraction times and completeness. Do not import passwords, sessions, OAuth secrets or unrelated finance data. Use synthetic clients in tests and documentation. See [migration](../migration.md).

First-contact sources additionally include `lib/introduction-proposal.ts`, `lib/introduction-saved-template.ts`, `lib/introduction-saved-template-api.ts` and `introduction_saved_templates`. They evidence owned Manager selections and 4/8-model seasonal layouts; see [introduction requirements](../requirements/introductions.md).

## Source differences to preserve deliberately

- Source statuses are `draft`, `ready`, `sent`; archive/soft hide is separate. “Accepted order” is not silently introduced as a quote status.
- Source counterparties have mixed roles and external identities. Only explicit CLIENT membership qualifies for client migration; names alone are not identities.
- Source supports sewing, PPE, individual, custom sewing and one-off PPE. Legacy `individual` needs typed normalization at import, not loss of data.
- Source allows manual/unlinked client names; target supports unresolved legacy recipient snapshots with visible reconciliation status, while new ready proposals require a resolved client/contact.
- PPE override reasons are optional in some old code but explicit in the workbook. Target requires a reason; preserve old missing-reason history as such rather than inventing one.
- Source custom sewing has fallback coefficients and an operation-ID fallback. Target uses approved persisted versions and typed blockers, with no SKU/operation magic defaults.
