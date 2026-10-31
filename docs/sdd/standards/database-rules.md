# Database rules

## Identifiers

The [identifier strategy](identifier-strategy.md) defines how to choose a scalar type, typed domain ID, `PublicId`, and human-readable document number. Record the choice and rationale in the feature specification's `data-model.md` before creating code or a migration.

## Reads and performance

Use projections and `AsNoTracking` for read scenarios where appropriate. Avoid N+1 queries, unbounded list endpoints, and unnecessary materialization. Pagination and filtering of large lists must be part of the contract.

For a list or detail read model that does not need domain behavior, the default approach is `Ardalis.Specification.Specification<TEntity, TResult>` with SQL `Select` into a response DTO. A handler should receive `TResult` from the read repository rather than materializing `TEntity` and mapping it in memory. Add only necessary `Include` statements and joins; a projection from navigation properties should return only fields that are used. Verify with an integration test that filters, sorting, pagination, and projection translate to SQL.

## Consistency and transactions

Respect existing transaction and concurrency boundaries. Do not add automatic retries to writes with side effects without documented idempotency. Do not silently weaken consistency guarantees.

## EF Core, migrations, and data

EF Core mapping belongs in Infrastructure. Each new module keeps its tables and migration history in its own schema and does not reference another module's tables; see the [module rules](module-rules.md#database-schema-per-module). Domain entities have no persistence-mechanism dependencies. Create a migration only when the feature scope requires it; do not edit applied migrations. Verify a new migration against an empty test database and document rollback and deployment risks.

Preserve query semantics and database contracts. Change reference and seed data through the existing mechanism, not hidden runtime writes.
