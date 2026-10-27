# Crm schema

Code model reviewed 2026-10-26. `Crm.Infrastructure.Persistence.CrmDbContext` owns schema `crm` in the existing PostgreSQL database. Five client tables are added by `InitialCustomerMaster`; seven quote/pricing tables by `AddQuoteDrafts`. `EnforceQuoteVersionOwnership` adds composite version relationships. Migration history is `crm.__EFMigrationsHistory`. Quote migrations have only run in disposable test databases; application at another environment is confirmed by its job run.

| Table | Identity / relationships / purpose |
| --- | --- |
| Customers | Generated typed long ID, unique Guid PublicId, kind/names/tax identity, typed int Identity owner reference, active, optimistic revision, UTC audit; restrictive self-FK MergedIntoId preserves merge survivor |
| CustomerContacts | Generated typed long ID/PublicId, restrictive Customer FK, name/position/active/primary. Partial unique index permits at most one active primary per customer |
| ContactChannels | Generated typed long ID, Contact FK, email/phone/raw/normalized values. Unique within contact/kind/normalized value, not globally unique; searchable normalization |
| CustomerExternalIdentities | Generated typed long ID, restrictive Customer FK. Unique source/type/externalId across customers; source IDs never replace Brevi PK |
| MutationRequests | Technical long ID; unique actor/operation/requestId, input hash, customer PublicId, successful intent/result audit JSON and UTC time. Atomic with business write; no secret/auth credential data |
| Quotes | Generated typed long ID/PublicId, owner/customer and frozen recipient, draft state/revision, nullable number, current version. Composite `(Id,CurrentVersionId)` FK points to `(QuoteId,Id)` in QuoteVersions, so the current version belongs to this quote |
| QuoteVersions | Generated long ID/PublicId; restrictive Quote FK, unique `(QuoteId,VersionNumber)` and alternate `(QuoteId,Id)` key; parent composite FK stays within this quote. Immutable saved recipient, currency/tax basis and graph |
| QuoteVersionItems | Version FK, logical PublicId reused across versions, ordered public text/private notes and include flag |
| QuoteItemVariants | Item FK, logical PublicId, order/title/visibility, selection references and durable server price-basis JSON |
| QuoteVariantTiers | Variant FK, unique semantic tier per variant, nullable decimal amount, UAH and visibility |
| CalculationBases | Opaque token/PublicId, actor/requestId uniqueness, input hash and optional quote scope, server snapshot JSON, expiry greater than creation; indexed expiry |
| QuoteMutationRequests | Actor/operation/requestId uniqueness, payload hash, quote PublicId and committed response JSON; journal shares the graph transaction |

No physical FK or EF navigation to Identity/Reference/Catalog. Role claims supply current actor; there is no supplier-to-customer auto-copy. Clients are not Identity users. Draft quote recipient and price snapshots are implemented; later lifecycle, document, custom and archival import tables are not.

Customer root row locks serialize writes; request advisory locks serialize idempotency; revisions are concurrency tokens. Merge locks both roots in deterministic PublicId order and transfers persisted contact/source identities, retaining the victim inactive with survivor link. Quote graph insertion and pointer/revision update commit together. Tests verify races, source/actor/expiry checks, scoped paging and composite version integrity. Issued/ready quote invariants belong to later tasks.
