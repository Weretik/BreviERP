# Product page — narrow Reference pricing reader for the admin list

**Status:** implemented without caching; performance verification for a non-empty page awaits `05.3`\
**Scope:** internal read path for `GET /api/v1/products`; the public HTTP/OpenAPI contract, Product schema, and calculation rules do not change.

## Problem

After loading a page, the current list handler calls the full
`IProductReferenceReader.GetSnapshotAsync()`. The Host adapter sequentially runs the general
Reference UI queries for suppliers, fabrics, accessories, operations, and additional references.
Some of these queries make extra searches to resolve display names.

The admin list does not display supplier or garment-part names and needs only the following for pricing:

- `Fabric`: `Id`, `Price`;
- `GarmentAccessory`: `Id`, `Price`;
- `GarmentPartOperation`: `Id`, `Min`;
- `AdditionalReference`: `Id`, `Key`, `Value`, `Unit` only for percentages used by the current PPE page and allowed Sewing pricing keys.

Loading complete UI DTOs and all reference records fetches unnecessary data and adds a series of
sequential database calls for each page request.

For an empty list, this path is not run: the handler falls back to calling
`GetSnapshotAsync()`. The first slow empty-list request turned out to be a cold start, so this
subphase does not fix the already verified warmed empty-list request latency.

## Production design decision

1. After Catalog projects the page, collect unique IDs only from Product rows on that page:
   fabrics, accessories, operations, and AdditionalReferences. Do not issue a query for an empty set.
2. Catalog Application introduces the narrow abstraction
   `IProductListPricingReferenceReader`; it returns only the numeric pricing inputs needed for
   `MinimumWholesalePrice`.
3. The Host adapter maps sets of primitive IDs to a specialized Reference Application query.
   Reference does not depend on Catalog DTOs or entities.
4. The Reference query performs set projections (`WHERE id IN (...)`) without UI search queries,
   supplier names, or garment-part names. The number of database commands is bounded by reference
   data types, not by the number of Product rows on the page.
5. The handler builds `ProductReferenceData` or an equivalent pricing model only for the in-memory
   page and applies the existing formulas without changing values or rounding.

## Caching

The primary optimization is narrowing the data retrieved. Caching is allowed only for the small,
explicitly permitted set of stable Sewing pricing coefficients (`AdditionalReference`), not the full
Reference snapshot. Before adding caching, the implementation must confirm:

- the TTL is short and explicitly documented;
- an invalidation mechanism exists after Reference writes, or caching is **not added**;
- the cache does not hide changes to `Fabric.Price`, accessory price, operation minutes, or PPE percent;
- behavior across multiple Host instances is defined (distributed invalidation or a TTL-only compromise
  with an agreed maximum stale-data period).

Do not add `IMemoryCache` or a distributed cache “just in case.” Without an agreed invalidation
strategy, narrow database projections remain the only implementation.

A search of the current codebase found no Reference cache invalidation or distributed cache write
semantics for Fabric, GarmentAccessory, GarmentPartOperation, or AdditionalReference.
Therefore the implementation deliberately does not add caching: each list page reads current, narrow
pricing projections.

## Invariants and criteria

- The list retains filtering, sorting, and pagination, plus `categoryIds`, `mainPhoto`, and
  `minimumWholesalePrice` for Sewing/PPE/fallback `0`.
- A single page does not issue one query per Product; the number of SQL commands is bounded.
- The full `IProductReferenceReader` remains for create/update/detail, where full
  reference data is actually needed.
- Integration tests compare prices from the old full snapshot and the new narrow reader
  using the same Product/Reference data set.
