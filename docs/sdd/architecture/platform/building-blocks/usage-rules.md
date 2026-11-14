# BuildingBlocks Usage Rules

## Reuse Before Creating

| Need | Use first | Do not |
| --- | --- | --- |
| Expected use case failure | Ardalis.Result and the existing Result mapping | do not throw ordinary validation or resource-not-found exceptions |
| Input validation | FluentValidation plus ValidationBehavior | do not validate only in the controller |
| Business invariant | aggregate behavior returning `DomainResult` with a module `*Errors` `Error` | do not throw for a business rule; do not move a domain rule into a pipeline behavior |
| Current user/permissions | the user passed into the command by the Api (`By`) and host access policies | do not inject `IHttpContextAccessor` into Application |
| Tell another module or the outside world about a change | the transactional outbox (module rules) | do not call it inside or around the transaction; there are no domain events |
| Database migration/seeding | startup extensions `IDatabaseMigrator`/`ISeeder` | do not run them inside a handler |
| Request diagnostics | existing behaviors and logging helpers | do not log raw sensitive data |

## Extension Rule

Add a shared primitive only when at least two modules need the same stable technical abstraction. A feature-specific abstraction stays in its own module's Application. A business concept must never move into BuildingBlocks.

Document additions to or ordering changes in cross-cutting infrastructure in an ADR and the feature specification.
