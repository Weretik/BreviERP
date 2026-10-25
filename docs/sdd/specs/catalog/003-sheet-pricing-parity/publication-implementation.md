# Saved sewing calculations and publication (EN-004 / TS-004)

Authorized scope: append-only input changes, saved calculations, publication and backend HTTP contracts. Consumer cutover (TS-005) and PPE (TS-006) remain separate.

## Acceptance

- SC-P01: an authorized manager saves a calculation, including blockers, the full composition, selected settings, reference values/version IDs and formula version. A client-generated request UUID makes retries return the original result; reusing it for another Article is a conflict.
- SC-P02: a manager publishes a saved, successful calculation only when a fresh calculation at server time selects the same composition and pricing versions and remains successful. Missing, inactive, ambiguous, unreviewed or changed inputs produce conflict. A failed publication adds no price row.
- SC-P03: publishing the same calculation again returns the same publication. Publishing another calculation appends history; current published price is the latest publication for the Article. Reads do not recalculate historical results.
- SC-P04: changing a previously versioned Fabric price, accessory price or operation norm appends an approved effective revision in the same Reference transaction. Name-only edits do not append. Existing versions are immutable. Unversioned legacy references are unchanged. Explicit OtherCost revisions and complete sewing setting revisions use protected commands with expected-current-version concurrency checks.
- SC-P05: reads require CatalogRead; writes require CanManageProducts. Actor identity comes from authenticated claims. No caller can publish arbitrary prices or supply calculation time. Existing Product price consumers remain unchanged.

## Storage and consistency

Catalog owns immutable Infrastructure persistence records for saved calculation envelopes and publications, each with generated long relational keys and separate UUID public IDs. These are persistence records, not Domain entities; Product/Article IDs remain scalar columns with restrictive FKs. Calculation JSON is a complete immutable Application snapshot, not a second editable master. Publication stores only its calculation FK, timestamp and actor; exact prices are read from the retained calculation. Unique request/publication-per-calculation indexes enforce retries. Reads are bounded detail/current endpoints; no unbounded history list.

All sanctioned pricing writes acquire PostgreSQL transaction advisory lock `681025004` before reading current versions. Saved calculation and publication use one Catalog transaction; the Host reference bridge enlists Reference reads in that transaction/connection. Publication also locks the Product row before reading composition, serializing against normal composition writes. Lock order is global pricing lock, then Product row. Reference edits acquire the pricing lock before SaveChanges and append revisions atomically. Settings and explicit cost revisions use the same lock. ReadCommitted after locks provides a fresh view and avoids stale snapshots while waiting. Every transaction is disposed on failure; no automatic write retries.

Calculation requests always use server time. Effective versions, full settings and reference values are retained; publication compares composition version, settings ID, sorted reference version IDs and successful readiness. Versioned source values are never updated in place. Normal Reference changes preserve unit/source identity and activity; they become approved through the existing protected editing workflow. Future-dated baseline versions are rejected by ordinary editing rather than silently overwritten.

## API and rollout

See [human contract](contracts/api-contract.md) and [machine contract](../../../contracts/catalog/sewing-pricing.openapi.yaml). Catalog migrations create calculation/publication tables and enforce a composite Product/Article FK. Reference migration widens operation minutes from numeric(18,2) to numeric(24,8), retaining all 16 original integer digits and adding six fractional digits. New norm edits must fit pricing numeric(20,8). Existing pricing version tables remain. Deploy migrations through Host.Seed before API. Verify empty disposable PostgreSQL migrations and an upgrade from the current local baseline. Rollback disables the new endpoints/consumer cutover; retain history tables and input versions. Do not run destructive Down on populated history or narrow norm precision after eight-decimal edits. Local migration is authorized; remote deployment is not.

Tests: focused API authorization/shape tests, calculation/publication PostgreSQL scenarios for retries, blocked and stale publication, retained history, version edits and rollback. Record evidence separately after execution.
