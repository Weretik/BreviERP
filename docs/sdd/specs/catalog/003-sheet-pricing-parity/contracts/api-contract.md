# Sewing pricing API decisions

Source: [OpenAPI](../../../../contracts/catalog/sewing-pricing.openapi.yaml). Scope: [EN-004 / TS-004](../publication-implementation.md).

All reads require CatalogRead and all writes CanManageProducts. Article routes retain existing numeric Product/Article IDs; new calculations/publications expose UUIDs, hiding relational history IDs. The actor is read from the authenticated NameIdentifier claim; no actor, arbitrary price, effective time or approval flag is accepted in calculation/publication requests.

Save returns HTTP 200 for both a new result and an idempotent replay. Its request UUID identifies the original Article calculation even after inputs change. Reuse for another Article returns 409. A blocked result is a successful diagnostic save, with no prices; publishing it returns 409. Publication uses the calculation UUID as its idempotency identity. Replaying an old publication returns it without making it current again. Scoped detail reads and the current-price endpoint return 404 for missing results. No unbounded history endpoint is introduced.

Settings updates accept a complete setting object, expected latest version ID and request UUID; they append an approved server-time revision. All thirteen values must be present. Reuse with different settings returns 409. OtherCost changes follow the same rule, preserving the service unit. Existing Fabric/accessory/operation edits append approved pricing revisions atomically when their numeric cost/norm changes. These three Reference controllers preserve anonymous reads and now require CanManageProducts for writes.

Existing Product list/detail prices remain unchanged until TS-005. This is an additive pricing API, with the explicitly documented Reference authorization correction. Frontend synchronization and its clean-commit pin are deferred to TS-005; no dirty backend snapshot is handed off as a stable pin.
