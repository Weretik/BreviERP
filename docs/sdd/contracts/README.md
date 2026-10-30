# API Contracts

`docs/sdd/contracts/` is the canonical, versioned source for machine-readable public API contracts.

## Structure

```text
docs/sdd/contracts/
├── openapi.yaml                         # aggregate entry point for all public APIs
└── <module>/
    └── <feature>.openapi.yaml           # contract owned by one feature
```

`openapi.yaml` is the entry point for Swagger UI, frontend client generation, and contract tests. It references module contracts through `$ref`; do not duplicate operations or schemas in the aggregate file.

## Current frontend handoff

- [Frontend integration guide](frontend-integration.md) describes authentication, CSRF, pagination, errors, and current contract coverage.
- [Aggregated OpenAPI 3.1 contract](openapi.yaml) covers APIs for which a versioned contract has been created and verified.
- Identity: [session](identity/session.openapi.yaml).
- Catalog: [products](catalog/product-catalog.openapi.yaml), [categories and media](catalog/product-dependencies.openapi.yaml).
- Reference: [suppliers](reference/suppliers.openapi.yaml), [fabrics](reference/fabrics.openapi.yaml), [garment accessories](reference/garment-accessories.openapi.yaml), [garment parts](reference/garment-parts.openapi.yaml), [operations](reference/garment-part-operations.openapi.yaml), [additional references](reference/additional-references.openapi.yaml).

OpenAPI files describe the contract of the actual code. They do not make an endpoint publicly accessible: authorization attributes and the host fallback policy remain authoritative.

Each feature SDD contains `contracts/api-contract.md` for human-readable decisions: boundaries, consumers, compatibility, security, errors, idempotency, and deployment. For new or changed HTTP behavior, agree on these decisions and create or update the machine-readable contract before implementing the transport layer where practical. Add focused Red contract/API tests based on the agreed contract, then implement the transport. The feature document must link to `docs/sdd/contracts/<module>/<feature>.openapi.yaml`, and the aggregate `openapi.yaml` must be updated in the same change.

Do not create a public OpenAPI document for internal-only changes. Before adding an HTTP contract, agree on the route, method, operation ID, DTOs, examples, errors, security, idempotency for side-effecting writes, and migration path for breaking changes.

- Crm: [clients and contacts](crm/clients-quotes.openapi.yaml), [quote drafts, lifecycle and ready](crm/quote-drafts.openapi.yaml), [custom sewing](crm/custom-sewing.openapi.yaml), [templates and suggestions](crm/custom-templates.openapi.yaml), [first-contact selections](crm/introduction-templates.openapi.yaml), [private files](crm/private-files.openapi.yaml), [issuer settings](crm/issuer-settings.openapi.yaml), [private quote PDF artifacts](crm/quote-pdf.openapi.yaml). Delivery operations remain planned. TS-009/010 frontend provenance is pinned to e5f563456efd1b29a67a36805c2b3775ce2f9d06. The TS-016 extension is a local uncommitted checkpoint awaiting a separately authorized immutable consumer handoff.

P6 local backend contracts: [versioned email templates](crm/email-templates.openapi.yaml) and [pinned private production packages](crm/production-packages.openapi.yaml). Gmail provider is fail-closed until reviewed OAuth and controlled provider proof; delivery outbox operations remain planned. These new contracts are not yet an immutable frontend consumer handoff.

P6 [explicit quote delivery queue/history](crm/quote-deliveries.openapi.yaml): enqueueCrmQuoteDelivery, getCrmQuoteDeliveries and getCrmQuoteDelivery. Local implementation; frontend consumption requires a separately authorized immutable backend handoff.
