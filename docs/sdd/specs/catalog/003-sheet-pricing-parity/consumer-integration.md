# TS-005 — published-price consumer integration

Scope: React Admin Article calculation/publication and published prices in Product cards/lists. The frontend owns its linked scenario SDD; this backend revision adds a bounded published-price read projection. PPE and quote cutover remain separate. Existing Product response fields remain compatible; the Admin consumer selects published Article prices for Sewing and never falls back to the legacy formula.

SC-C01: CatalogRead callers request 1–100 unique positive Product IDs and receive valid active Sewing Products, their active Article count, and the latest publication per active Article. Products with no publication return an empty Article list; missing/inactive/non-Sewing IDs are omitted. No unbounded history, costs or actor identity are exposed.

SC-C02: publishing a later calculation changes the projection; replaying an older publication does not restore it as current. The projection uses a fixed two-query plan for the entire Product set, not one request/query per Product or Article. It reads stored tier values, never recalculates formulas. Read consistency is eventual across the two projection queries; publication remains atomic under the existing write protocol.

Operation `getPublishedSewingProducts`: GET `/api/v1/pricing/sewing/published`, repeated `productIds` query parameter, CatalogRead, 200 array, 400 invalid/empty/over-limit IDs, 401/403 access. Additive [OpenAPI](../../../contracts/catalog/sewing-pricing.openapi.yaml); no schema/migration change. New Application reader and Infrastructure SQL projection live beside Publications; one query/handler and controller bind the read scenario. Preserve immutable calculation records and source JSON format.

Verification: PostgreSQL latest-publication projection and no-publication fixtures, request bounds and HTTP authorization/operation contract checks. After checks the user-authorized local backend checkpoint is pinned into the frontend contract snapshot from a clean checkout. No push or remote deployment.

Backend checkpoint verification: 40 focused Unit/contract checks passed, including aggregate OpenAPI resolution, request bounds and the new schema/policy checks. Nine focused PostgreSQL/API integration checks passed; the publication fixture verifies no-publication Article count and latest stored tier projection. The additional HTTP batch-binding/access test passed (1/1). No schema or target database migration is required. Frontend consumer implementation continues from this checkpoint.
