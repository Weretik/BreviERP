# Rules and scenarios

## Rules

- **R-001:** A Sewing Product may have several Articles. Each Article has one parent Product and a unique immutable sale SKU; Product ID and SKU have different meanings.
- **R-002:** A model-wide material or operation line applies to every Article of its Product. An article-specific material line applies only to that Article. A conflicting common/article override is rejected until an explicit precedence rule is specified.
- **R-003:** Material lines record kind, reference, positive quantity, unit, required flag and stable ordering. Fabric and accessory references resolve through the Reference boundary. Non-inventory costs are identified separately from physical materials.
- **R-004:** Operation lines record operation reference, positive quantity, optional positive manual minutes, ordering and review state. The effective time for a line is its reviewed manual minutes when present, otherwise the current referenced standard minutes; the later Costing specification decides snapshot and price behavior.
- **R-005:** Missing or inactive reference data, unknown units, duplicate sale SKU, duplicate effective composition or unreviewed required inputs produce explicit validation failures. They are never silently priced as zero.
- **R-006:** Existing Product create/replace/read clients remain compatible during rollout. Full Product replacement does not silently delete or overwrite Articles or the new composition. Once new composition becomes authoritative, legacy fabric/accessory/operation editing is removed or adapted through one write path.
- **R-007:** Source model codes and sale SKUs are business identifiers for matching and display, never Brevi primary keys. No existing Brevi Product is merged or overwritten by name alone.
- **R-008:** This feature does not publish prices. Existing Brevi pricing continues unchanged until a separate accepted Pricing SDD changes it.

## Observable scenarios

- **SC-001 — Create article:** Given an existing Sewing Product, when an authorized admin adds SKU `246`, then its detail shows one Article linked to that Product. Adding the same SKU again returns a conflict and leaves the first Article intact.
- **SC-002 — Two fabrics on one model:** Given model `MOD-025` with Articles `246` and `247`, when the admin assigns `FAB-059` at 3.6 m to `246` and `FAB-038` at 3.6 m to `247`, then each Article's composition shows only its assigned fabric and the same eight model-wide lines.
- **SC-003 — Shared operations:** Given twelve reviewed model operation lines, when the admin reads either Article, then both show the same operations and quantities. Changing one model-wide line is visible on both without copying it into Article records.
- **SC-004 — Validation:** Given an unresolved fabric/operation, unsupported unit or missing required quantity, when an admin saves, then the request is rejected with field-specific errors and no partial composition change.
- **SC-005 — Existing clients:** Given an older Product create/replace request, when it is processed during rollout, then its existing fields behave as before and do not remove Articles or new composition.
- **SC-006 — Review and lifecycle:** Given an unreviewed required operation line, when a client requests calculation readiness, then the Product/Article is blocked with a reason. Marking the line reviewed records actor/time and can make it ready if no other blockers remain.

## Exclusions

No import of live source rows, no new three-tier price formula, no quote/PDF history, no external Sheet/Gateway runtime integration, and no assumption that the source's `Other cost` line is stock inventory.
