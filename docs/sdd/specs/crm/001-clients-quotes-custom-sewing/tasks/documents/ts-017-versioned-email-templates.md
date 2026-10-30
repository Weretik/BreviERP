# TS-017 — Versioned email templates

**Status:** backend implemented; final verification in [checkpoint](../../evidence/p6-backend-checkpoint-2026-10-30.md). **Depends on:** TS-009, TS-016. **Enables:** TS-011 and P6 frontend selection creation.

Admin explicitly creates immutable plain-text subject/body versions and activates one with a revision check. Admin/Manager can read bounded lists, a specific version and the active version. New or changed selection references must resolve a real email-template version. An unchanged historical reference is retained without making it sendable. No automatic version seed or activation occurs.

Domain, Application, Infrastructure and API ownership, contract, migration and focused unit/contract/PostgreSQL coverage are recorded in [registry evidence](../../evidence/p6-email-template-registry-2026-10-30.md) and [design](../../design/email-templates-p6.md). Canonical contract: [email templates](../../../../../contracts/crm/email-templates.openapi.yaml). Frontend remains on its existing pinned contract until a separately authorized immutable handoff. Outbox and actual email sending remain TS-011/EN-004.
