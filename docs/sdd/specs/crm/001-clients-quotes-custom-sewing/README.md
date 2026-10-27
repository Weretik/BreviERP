# CRM-001 — Clients, quotes and custom sewing

**Status:** client/contact backend and Admin slice implemented locally; P2 draft-quote backend implemented locally with focused automated verification. Manual acceptance and frontend quote integration remain pending; custom/document/import phases are planned. **Reviewed:** 2026-10-26. **Owner:** existing Crm module. This is the next customer-facing block after Catalog composition/pricing. Local implementation does not mean deployment or migration of a working database.

## Outcome and scope

Extend the existing “Клієнти”, “Розрахунки та КП” and “Архів КП” menu entries into one workflow: select a customer/contact, compose a proposal from Catalog or private custom sewing, calculate on the server, save immutable versions, prepare a customer-safe PDF and explicitly send it with history. PostgreSQL is the operational source of truth. Google Sheets defines the source fields and approved pricing basis, not runtime storage.

Reuse Catalog's implemented articles, component/input versions and calculation engine. Do not create a second customer navigation entry, product master, pricing engine or supplier directory. Each capability remains a separate folder/use case within Crm. Frontend follows its existing `clients` domain structure.

In scope: customers and contacts; quote lists/search/archive; items, options and three price tiers; private notes and visibility; composition changes and reasoned PPE overrides; one-off PPE; versions, copy and explicit reprice; ownership transfers; custom sewing drafts, versions, suggestions, fabric alternatives and private files; reusable introduction/configuration templates; issuer settings, PDF, email outbox/history; controlled historical migration and a pinned production-package contract.

Outside this block: accounting, invoices/payments/payroll, procurement/stock, order acceptance workflows not evidenced by the source, live KeepinCRM/Gmail synchronization and actual Workshop transport. Production handoff is a versioned export contract here; connecting Workshop is a later integration. No runtime Google reads or old source credentials.

## Read in order

1. [Verified starting point and sources](sources/verified-baseline.md), [all 62 Google fields](sources/google-field-map.md).
2. Requirements: [clients](requirements/clients.md), [quotes](requirements/quotes.md), [custom sewing](requirements/custom-sewing.md), [documents and delivery](requirements/documents-delivery.md), [first-contact proposals](requirements/introductions.md).
3. [Backend boundaries and file map](design/backend.md), [data model](data-model.md), [security and consistency](design/security-and-consistency.md).
4. [Proposed API contract](contracts/api-contract.md), [migration and rollback](migration.md).
5. [Implementation sequence](tasks/implementation-plan.md), [verification](verification.md), [traceability](traceability.md), [decisions and readiness gates](decisions-and-risks.md).

Frontend counterpart: repository `Weretik/brevi-web`, specification `docs/specs/admin/clients/001-clients-quotes-custom-sewing/README.md`. Each repository owns its implementation tasks; scenario IDs are shared. This document contains no personal checkout/vault dependencies.

## Readiness and completion

EN-001/TS-001/TS-002 and the local backend implementation of EN-002/TS-003/TS-004 have focused checks. Frontend FE-003/FE-004 requires an immutable backend contract revision before generated-client sync. On 2026-10-27 the user authorized the local backend commit without push; the resulting SHA is recorded in frontend handoff provenance. Missing email setup or incomplete historic exports do not prevent new client/quote development. Unresolved business decisions are named gates, not invented defaults. See [client-slice evidence](evidence/clients-backend-2026-10-26.md) and [P2 backend evidence](evidence/p2-quote-backend-2026-10-26.md).

Complete means all in-scope scenarios have recorded automated/manual evidence, contract consumers use the same pinned backend revision, customer-safe document and permission checks pass, migration reconciliation accounts for every input row/file, and unresolved history is visibly quarantined. A partial import, downloaded PDF or saved draft does not mean the whole block is complete.

## Stable project references

- [Existing Catalog pricing SDD](../../catalog/003-sheet-pricing-parity/README.md)
- [Delivery rules](../../../standards/delivery-rules.md), [backend rules](../../../standards/backend-rules.md), [database rules](../../../standards/database-rules.md)
- [Canonical API contract index](../../../contracts/README.md)

## Change log

- 2026-10-26: initial complete block specification; source fields verified, existing menu retained, historical incompleteness and integration gates separated from new-flow delivery.

## Structural audit

[Client-slice audit and refactoring](code-audit/audit.md) and [P2 code audit](code-audit/p2-implementation-audit-2026-10-26.md) record responsibility/layout changes and verification. They do not claim completion of frontend, custom, document or import tasks.
