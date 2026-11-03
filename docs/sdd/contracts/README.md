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
- Catalog: [products](catalog/product-catalog.openapi.yaml), [«Товари» page overview](catalog/product-overview.openapi.yaml), [categories](catalog/product-dependencies.openapi.yaml).
- Media: [photo library with folders](media/media-library.openapi.yaml).
- Reference: [suppliers](reference/suppliers.openapi.yaml), [fabrics](reference/fabrics.openapi.yaml), [garment accessories](reference/garment-accessories.openapi.yaml), [garment parts](reference/garment-parts.openapi.yaml), [operations](reference/garment-part-operations.openapi.yaml), [pricing parameters and PPE markup rules](reference/pricing-parameters.openapi.yaml), [product sections](reference/product-sections.openapi.yaml).

OpenAPI files describe the contract of the actual code. They do not make an endpoint publicly accessible: the host endpoint conventions, authorization attributes and fallback policy determine runtime access.

Each feature SDD contains `contracts/api-contract.md` for human-readable decisions: boundaries, consumers, compatibility, security, errors, idempotency, and deployment. For new or changed HTTP behavior, agree on these decisions and create or update the machine-readable contract before implementing the transport layer where practical. Add focused Red contract/API tests based on the agreed contract, then implement the transport. The feature document must link to `docs/sdd/contracts/<module>/<feature>.openapi.yaml`, and the aggregate `openapi.yaml` must be updated in the same change.

Do not create a public OpenAPI document for internal-only changes. Before adding an HTTP contract, agree on the route, method, operation ID, DTOs, examples, errors, security, idempotency for side-effecting writes, and migration path for breaking changes.

- Crm: [clients and contacts](crm/clients-quotes.openapi.yaml), [quote drafts, lifecycle and ready](crm/quote-drafts.openapi.yaml), [custom sewing](crm/custom-sewing.openapi.yaml), [templates and suggestions](crm/custom-templates.openapi.yaml), [first-contact selections](crm/introduction-templates.openapi.yaml), [private files](crm/private-files.openapi.yaml), [issuer settings](crm/issuer-settings.openapi.yaml), [private quote PDF artifacts](crm/quote-pdf.openapi.yaml). Delivery operations remain planned. TS-009/010 frontend provenance is pinned to e5f563456efd1b29a67a36805c2b3775ce2f9d06. The TS-016 extension is a local uncommitted checkpoint awaiting a separately authorized immutable consumer handoff.

P6 local backend contracts: [versioned email templates](crm/email-templates.openapi.yaml) and [pinned private production packages](crm/production-packages.openapi.yaml). Gmail provider is fail-closed until reviewed OAuth and controlled provider proof; delivery outbox operations remain planned. These new contracts are not yet an immutable frontend consumer handoff.

P6 [explicit quote delivery queue/history](crm/quote-deliveries.openapi.yaml): enqueueCrmQuoteDelivery, getCrmQuoteDeliveries and getCrmQuoteDelivery. Local implementation; frontend consumption requires a separately authorized immutable backend handoff.

- Reminders: [Telegram reminders admin API](reminders/telegram-reminders.openapi.yaml) — tasks, periods, chats, non-working days and bot connection. The Telegram webhook is intentionally not in the contract.

## Temporary anonymous deployment mode

The host currently enables temporary anonymous access (runbook: `docs/sdd/operations/configuration/temporary-anonymous-api.md`) through ApiAccess:AnonymousEnabled=true. All controller operations accept requests without bearer authorization in that mode, including previously protected writes/files. Actor-dependent operations use an existing shared Admin; no eligible actor produces 503 with code anonymous_api_actor_unavailable. Login/refresh still validate credentials/refresh cookies/CSRF as inputs to those operations.

The module YAML contracts retain the normal access rules used when this switch is disabled. Routes, operationIds and DTOs are unchanged, so no generated frontend DTO synchronization is required. Live generated Swagger at /swagger/v1/swagger.json removes its global bearer requirement while anonymous mode is enabled; /openapi/v1.json and /swagger are also public in Production. Consumers must account for this explicit temporary deployment override. Disable the flag and restart to restore original policies.
