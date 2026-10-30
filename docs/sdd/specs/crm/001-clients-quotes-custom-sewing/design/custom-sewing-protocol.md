# Private custom draft and version protocol

TS-006 implements private mutable configuration separately from immutable calculated versions. Crm owns the roots/snapshots; Catalog remains the arithmetic owner through the Host adapter. [Arithmetic reconciliation](../evidence/en-002-custom-pricing-2026-10-28.md).

## Scope and writes

Create accepts a bounded structured composition, name/type/season, client description/comment/private note, applications with explicit sale approval, delivery/packaging and optional customer/quote PublicIds. Posted unit costs, wages, coefficients or calculated totals are rejected. A draft may omit fabric/operations and remain editable; explicit calculation returns blockers. Unknown/inactive/unapproved references and unit mismatches never become zero prices.

An unattached draft is accessible to its owner or Admin. A linked draft follows the currently assigned quote owner or Admin, including replay checks. Quote ownership is locked before the custom root during writes; revision comparison precedes mutation. Each successful create/save/calculate/version request journals its exact response atomically under actor/operation/requestId. Changed payload with the same key conflicts; replay rechecks current access. No hard-delete API.

## Calculate and freeze

Calculation accepts requestId/expectedRevision, not a browser snapshot. The server reads approved settings/reference versions at one UTC instant and stores a 15-minute opaque basis token bound to draft/revision/actor. Saving a custom version accepts only that token and expectedRevision. Expired, foreign or stale tokens conflict; blocked bases cannot publish. Current server source versions/amounts are compared again before freezing; drift conflicts without silently replacing the preview. Version persistence, root-pointer/revision change and journal commit share one transaction.

Each fabric alternative can be saved as an independently named immutable version. The source configuration, original/effective norms, source cost/norm versions, formula/settings IDs, internal breakdown and three decimal tiers are frozen. Custom charges/application sale follow EN-002. Final tiers must fit the existing quote numeric(20,2) storage without truncation; out-of-range/zero prices are explicit blockers.

## Quote hydration

Quote intent kind customSewing carries only customVersionPublicId. Preview resolves an authorized stored version, then creates its own quote-scoped basis token. The original custom-calculation expiry never invalidates an already frozen historical custom version. Quote save hydrates the stored configuration/basis and stores a restrictive numeric CustomVersionId FK plus typed saved snapshot. Selected versions are resolved in one bounded SQL batch, not one extra query for every variant.

Text-only quote saves retain the saved basis. Copy retains the selected version and prices without recalculation. Later private draft/V2 changes leave quote V1 unchanged; choosing V2 explicitly creates a new quote version. An assigned quote owner may read the exact versions already pinned in that quote even when the originating unattached draft belongs to someone else; this does not grant editing of that private root. Opening/replaying a fresh preview rechecks custom access before returning a cached result.

## Storage and remaining phase work

CustomSewingDrafts and CustomSewingVersions use typed long IDs with separate Guid PublicIds. Typed configuration/basis records are privately serialized as JSONB through infrastructure converters. Scalar root/version names support SQL projection. Same-draft composite current/parent FKs, unique draft/number, append-only version trigger and ordered-parent insert trigger preserve history. CustomSewingCalculations and CustomSewingMutationRequests have long keys and scoped token/replay indexes.

AddCustomSewingVersions and EnforceCustomSewingHistory are additive migrations. Apply through the existing Crm Host.Seed migration entry point before deploying the API. Do not run Down after custom history is used; disable the feature and recover forward or restore a coordinated backup. Empty-database creation/reapplication and upgrade from P3 with an existing quote were verified on a task-owned PostgreSQL instance only.

Artwork IDs can be retained in an editable configuration, but ArtworkFileNotVerified blocks calculated version creation until TS-007 verifies real private bytes/ACL. No metadata-only file or storage adapter is faked. EN-003/TS-007 owns uploads, per-version visibility and retention; TS-008 owns templates/suggestions; FE-006/007 follows a clean immutable backend contract handoff. Manual acceptance stays scheduled at the end.
