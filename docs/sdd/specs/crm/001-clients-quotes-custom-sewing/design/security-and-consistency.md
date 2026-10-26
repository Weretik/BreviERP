# Access, consistency and privacy

## Proposed role policy (EN-001 contract checkpoint)

| Action | Admin | Manager | User/anonymous |
| --- | --- | --- | --- |
| List/read clients | All | Owned plus clients attached to currently assigned quotes | Denied |
| Create/update client/contact | All | Own client master | Denied |
| Merge client masters | Allowed, revision/audit required | Denied | Denied |
| List/read/save/custom/files/quote PDF | All | Currently assigned quote/draft only; own unattached custom drafts | Denied |
| Copy/reprice/PPE override/ready/send/archive | All | Assigned records, server validates allowed action | Denied |
| Transfer quote | All | Own assigned quote to active eligible Manager/Admin; loses edit after transfer | Denied |
| Own reusable selection/configuration templates | All | Create/read/revise/archive own; use approved shared templates | Denied |
| Issuer/document-layout administration, historical import | Allowed | Read approved configuration only | Denied |

This is the specified target, not an existing permission guarantee. Implement via current Identity/policy mechanisms, no source director email, no trusted posted manager ID, no new role name without a policy change. Query and attachment authorization must enforce the same scope before materialization. Use indistinguishable not-found responses for inaccessible business IDs where current API conventions permit. Global search counts must not leak other owners' records.

## Concurrency and idempotency

Every mutation body carries `requestId` (UUID); changing an existing root also requires `expectedRevision` (integer). This feature uses the body convention consistently, not an additional competing idempotency header. Persist `(actorId, operationId, requestId)` plus normalized payload hash and outcome. Same key/hash returns original outcome; changed hash returns 409 `idempotency_conflict`. Authorization is checked on every replay. Stable resource command scope is included in the hash. Retain successful domain mutation keys with the audit record; do not silently expire them while retries remain possible.

Optimistic revision compare-and-increment, version number allocation and unique constraints arbitrate concurrent writes in PostgreSQL. 409 `revision_conflict` returns safe current revision info; browser offers reload/compare, never blind overwrite or automatic replay. Transaction rollback leaves no partial version/transfer or primary-contact change. Deleting or merging two roots locks them in deterministic ID order.

Price preview returns a server-held immutable calculation basis/token binding normalized input hash, input revisions, actor/scope and expiry. Save resolves the token on the server, validates submitted selections, and either persists that exact valid basis or returns `price_basis_expired`/`price_inputs_changed`. It does not replace it with current prices silently. Multiple sources are resolved coherently through the Catalog adapter, not parallel unversioned reads. Expiry and persistence policy is fixed in EN-002; expiry rejects new save, never invalidates a historical snapshot.

## Files and external side effects

New Crm documents/files are private even if current Catalog media is public. Reuse a storage implementation only behind an ACL-aware Crm port. Two-stage upload validates actual bytes and finalizes a file owned by actor/root; guessed or foreign attachment IDs cannot be linked. No remote URL fetch for arbitrary source images/PDFs; historical import uses controlled extraction with allowlisted storage origins. Keep per-version metadata and actual content checksum. A file referenced by history cannot be garbage-collected as a temporary upload.

Renderer accepts an explicit public projection, never arbitrary stored JSON/raw HTML. API UI-only flags are insufficient: the server excludes costs, internal notes, override reasons, private files and finance fields from PDF/email/production projections. Audit logs use record IDs and bounded error codes instead of payload dumps or credentials.

Outbox worker claims one delivery with a lease, records each attempt and stable provider key if supported. Provider success and local state can diverge on a crash; reconcile uncertain attempts using provider message ID/status before retry. “Sent” is version-specific and not a claim the customer read the mail. Old delivery history imports are inert evidence, not pending outbox records. Test worker overlap and crash boundaries.

## Retention

Keep immutable issued versions, source mappings, referenced attachments and delivery events. Final retention duration and personal-data erasure policy are EN-006 before production activation; no physical deletion job is introduced by default. Customer deactivation/archive prevents new usage without destroying history. Do not include production personal data in fixtures or the repository.
