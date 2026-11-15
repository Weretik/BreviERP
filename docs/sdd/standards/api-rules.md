# API rules

## Contract stability

Public routes, action names, and response fields are stable by default. Prefer additive changes. Document a breaking change, affected clients, migration path, and rollout plan in the feature specification and contract.

For new or changed HTTP behavior, link each operation to its `SC-*` acceptance scenarios. Agree on the readable decisions in the feature's `contracts/api-contract.md` before implementing the transport layer. Use them to create focused Red tests, then implement the endpoint with `WithName`, `WithSummary`, `Produces<T>` and `ProducesProblem`; the build regenerates [`openapi.json`](../contracts/openapi.json), which is reviewed and committed with the change ([ADR-0015](../architecture/decisions/0015-openapi-generated-from-code.md)).

Use task-specific DTOs. Do not expose entities, persistence types, secrets, or internal exceptions. Decide what a repeat does for every write before implementing it, and take a `requestId` where the [idempotency rules](idempotency-rules.md) require one (money, stock, orders, numbered documents, messages, public forms).

## Controllers

Controllers must:

- accept explicit request models;
- pass `CancellationToken` to Mediator;
- use the established Ardalis.Result-to-HTTP mapping;
- declare relevant `ProducesResponseType` attributes;
- contain no validation, business logic, or EF logic.

For a paginated list endpoint, the controller must preserve the entire `PagedResult<IReadOnlyList<TItem>>` in the response: both `value` and `pagedInfo`. Do not use a generic Result-to-HTTP helper if it returns only `result.Value` and drops `PagedInfo`; add or use a mapping that preserves the metadata.

## Validation, results, and errors

FluentValidation validators are registered by scanning the Application assembly and run through the Mediator validation behavior. Place each validator in the corresponding scenario directory.

Validate input shape: required fields, ranges, formats, enum values, length, and field combinations. Check state and business invariants in Domain or Application scenario logic.

Return Ardalis.Result for expected failures. Preserve the current API error mapping. Do not use exceptions for ordinary validation or missing-resource control flow, and do not expose internal details in responses.
