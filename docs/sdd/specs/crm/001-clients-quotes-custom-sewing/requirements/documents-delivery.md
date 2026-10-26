# Issuer settings, PDF and sending

## Rules

- **R-D01:** Seller legal name, registration/tax identifiers, address, IBAN/bank/MFO, contacts, site and logo belong to versioned Crm issuer settings. They are not customer bank details. Admin edits and activates verified settings; each ready version pins issuer and document-template versions. No fabricated defaults, copied source token or inferred VAT registration. Blank settings do not block drafts but block ready/customer-ready PDF.
- **R-D02:** PDF is generated server-side from a pinned ready/sent quote version through an explicit customer projection. It contains only included items/variants/tiers, public text/images, contact and issuer snapshot. Internal notes, costs, suppliers, wage breakdown, hidden options, override reasons and production-only files are excluded. Images resolve from validated media references; no arbitrary URL fetch. User text is escaped, not executable HTML.
- **R-D03:** Store private PDF artifact reference, quote/version/template IDs, generation time, content hash and render status. Generation retries are idempotent for identical version/template/render parameters. Render failures never set ready/sent or overwrite a good artifact. Historical downloads do not recalculate/re-render from current data. Metadata-only missing source PDF displays a clear missing-file state.
- **R-D04:** Sending requires an explicit authorized action with visible recipient and pinned version/PDF. Enqueue delivery and audit in a database transaction. Worker dispatches via a configured provider; status is queued/processing/succeeded/failed/unknown. A confirmed provider success records sent time/event for that exact version; download or queue submission alone is not sent. A retry with the same requestId does not create a second delivery. Unknown provider outcome must be reconciled before retry, because SMTP cannot guarantee exactly-once delivery. No automatic sends during migration.
- **R-D05:** Email credentials/secrets remain in deployment configuration, never quote payloads or imported rows. Retain delivery attempts, provider message ID and bounded error code, with personal-data-safe logging. Shared links, external Gmail/Keepin access and generic mail campaigns are outside scope. A separate reviewed integration is needed if an existing provider cannot satisfy EN-004.
- **R-D06:** Currency/tax wording and numbering are explicit settings/policies resolved in EN-002/EN-005. Quote prices retain the backend's approved monetary basis; do not add VAT again in PDF. A stale draft must validate issuer, recipient, price visibility and pricing blockers before becoming ready. Sent version remains immutable even if live issuer/customer settings change.

## Scenarios

| ID | Given | When | Then |
| --- | --- | --- | --- |
| SC-D01 | Draft quote and missing issuer/tax policy | Saves draft then attempts ready/PDF | Draft works; customer-ready action names missing settings and produces no fake legal document |
| SC-D02 | Ready quote with hidden options/internal notes/costs | Generates/reopens a PDF after masters change | Document uses pinned public content, retains checksum, contains no internal data and does not reprice |
| SC-D03 | Explicit recipient, provider available, repeated request | Sends then retries the same action | One outbox delivery is created; only provider-confirmed success marks that version sent; failure/unknown remains visible |
| SC-D04 | Malicious text, foreign private file or broken media | Renders/downloads document | Escaping, ACL and media validation prevent data leakage/remote fetch; failure preserves the good prior artifact |
