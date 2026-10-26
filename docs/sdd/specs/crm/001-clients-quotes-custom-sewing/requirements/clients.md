# Clients and contacts

## Rules

- **R-C01:** One Crm customer master serves the existing Clients entry and every quote. Customer is a business identity, not an Identity login. Keep Reference suppliers under Reference; a party with both roles has an explicit association, not an automatically duplicated supplier/customer row.
- **R-C02:** Store display/legal name, customer kind (organization/person), optional tax identity, contacts and their channels, active/archive state, responsible Identity user and external identities. Basic drafts need a nonblank display name; required legal fields depend on kind and the confirmed issuer/document policy. Do not force IBAN or an invented legal code on an individual.
- **R-C03:** Contacts belong to a customer, have name and optional position, email/telephone channels and a primary indicator. Normalize for search while retaining entered values. Validate email format and feasible length; do not reject real international telephone formats using a local-only mask. At most one active primary contact per customer, changed atomically.
- **R-C04:** Exact external identity `(source,type,id)` is unique. Tax identifier matches and normalized names/phones/emails produce review candidates; a name match never automatically merges people/organizations. Do not force email/phone uniqueness across customers. Merge is Admin-only, audited, locks both revisions and repoints live references/source mappings; issued snapshots remain unchanged. Conflicting external identities block merge.
- **R-C05:** Deactivation hides a customer/contact from new selections; existing history remains readable. No destructive deletion of a referenced customer/contact. Quote recipient details are frozen per quote version, not changed by editing the live client.
- **R-C06:** Search/filter/pagination respect access scope on the server. Admin sees all; Manager sees owned clients and clients referenced by quotes currently assigned to them. Transferring a quote grants that scoped read but does not transfer the customer's master ownership or expose other managers' quotes. Only an owner or Admin changes master data. User role has no CRM access by default.

## Scenarios

| ID | Given | When | Then |
| --- | --- | --- | --- |
| SC-C01 | Authorized Manager, existing Clients menu | Creates an organization/person and a contact | One client and linked contact are saved in PostgreSQL and appear in that menu's list/detail, not a second master |
| SC-C02 | Similar names and an existing external identity | Creates/imports another candidate | Name-only matches are reviewed; the same exact source identity cannot create duplicates, including concurrent requests |
| SC-C03 | Two contacts and quote version V1 | Changes primary contact, then deactivates it | At most one active primary remains; V1 retains original recipient; inactive contact cannot enter a new ready version |
| SC-C04 | Other manager's client and quote | Searches, requests guessed IDs, updates or merges | Server enforces scope, returns no unrelated data and audits authorized changes; only Admin can merge |
| SC-C05 | Two proven duplicate customers, both used in history | Admin merges with correct revisions | Live references use survivor, both source identities resolve, old issued names/prices remain byte-for-byte unchanged; stale/conflicting merge has no partial effect |
