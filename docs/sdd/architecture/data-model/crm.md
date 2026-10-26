# Crm schema

Code model reviewed 2026-10-26. `Crm.Infrastructure.Persistence.CrmDbContext` owns schema `crm` in the existing PostgreSQL database. Five business/technical tables are added by `InitialCustomerMaster`; migration history is `crm.__EFMigrationsHistory`. Application at an environment is confirmed only by a job run, not this model description.

| Table | Identity / relationships / purpose |
| --- | --- |
| Customers | Generated typed long ID, unique Guid PublicId, kind/names/tax identity, typed int Identity owner reference, active, optimistic revision, UTC audit; restrictive self-FK MergedIntoId preserves merge survivor |
| CustomerContacts | Generated typed long ID/PublicId, restrictive Customer FK, name/position/active/primary. Partial unique index permits at most one active primary per customer |
| ContactChannels | Generated typed long ID, Contact FK, email/phone/raw/normalized values. Unique within contact/kind/normalized value, not globally unique; searchable normalization |
| CustomerExternalIdentities | Generated typed long ID, restrictive Customer FK. Unique source/type/externalId across customers; source IDs never replace Brevi PK |
| MutationRequests | Technical long ID; unique actor/operation/requestId, input hash, customer PublicId, successful intent/result audit JSON and UTC time. Atomic with business write; no secret/auth credential data |

No physical FK or EF navigation to Identity/Reference/Catalog. Role claims supply current actor; there is no supplier-to-customer auto-copy. Clients are not Identity users. No quote tables, archival import or quote recipient snapshots exist in this slice; future quote-derived reads/merge reference updates belong to the quote slice.

Root row locks serialize writes; request advisory locks serialize idempotency; revision is a concurrency token. Merge locks both roots in deterministic PublicId order and transfers persisted contact/source identities, retaining the victim inactive with survivor link. Issued quote invariants will be tested when quote versions exist.
