# Product page — admin list query performance

**Status:** implemented in corrective slice `03.9`\
**Scope:** `GET /api/v1/products`; the HTTP/OpenAPI contract, schema, and Product business rules do not change.

## Observation

The first `GET /api/v1/products` after starting Host API on 2026-08-19 returned `200 OK`, but logs showed:

- EF Core `MultipleCollectionIncludeWarning` for a query with multiple collection navigations;
- `Slow request GetAdminProductsQuery: 7740 ms` against a 500 ms threshold;
- HTTP request duration of 9142 ms.

Subsequent warmed requests without restarting Host API returned `200 OK` in 323.7063 ms and
172.2967 ms. Both are below the 500 ms slow-request threshold. Thus, the recorded delay on an empty
list is a cold-start cost (JIT, EF model/query compilation, or the first database connection), not a
steady-state delay for the list endpoint.

The current `GetAdminProductsSpec` projects `Categories`, the primary photo, and Sewing collections
in one list read query. Total handler time also includes counting, resolving Media URLs, and
`IProductReferenceReader.GetSnapshotAsync`, so the warning is not considered the sole cause of the
delay without measurement.

## Decision

1. Apply local EF Core `AsSplitQuery()` to the current projection: collection results are materialized
   with a fixed set of separate SQL queries instead of one cartesian query. If subsequent profiling
   shows that the fixed set of split queries misses the target, the next step is a flat page
   projection and bounded batch projections by page ID.
2. Do not apply a global `QuerySplittingBehavior` or add `Include` without SQL evidence.

## Invariants

- Filtering, child categories, sorting, stable secondary sorting by `Id`, pagination, and total count
  remain in the database and semantically unchanged.
- The response retains `categoryIds`, `mainPhoto` URL/null, and `minimumWholesalePrice`.
- Additional SQL operations are bounded by page size, not by the number of Product rows; N+1 is not allowed.
- Migrations, tables, OpenAPI YAML, and authorization do not change.

## Acceptance criteria

- The integration test does not produce `MultipleCollectionIncludeWarning` for the list page projection.
- SQL/projection tests confirm no N+1 and preserve the response contract.

## Implemented step

`GetAdminProductsSpec` uses `Query.AsNoTracking().AsSplitQuery()`.
The PostgreSQL integration test configures `MultipleCollectionIncludeWarning` as an exception
and successfully executes the projection.
