# Delivery rules

Prepare an SDD before implementing a new feature or changing the scenarios, API contracts, database schema, or job behavior of an existing feature. For an existing feature, update its current specification or add a revision in its directory; do not duplicate the entire SDD. Maintain traceability for the changed scope.

For a local internal fix that restores already documented behavior without changing a scenario, contract, database schema, or module boundary, a short description of the problem, fix, and focused verification in the task or report is enough. Read historical revisions only for a specific need.

## SDD workflow

Keep a feature's current scope and navigation in its `README.md`, requirements in the current specification, evidence in technical tasks, and traceability as brief links and statuses. Preserve earlier revisions as history; do not copy current status into them or open them without a specific need.

1. Define the goal, scope, business rules, and acceptance scenarios with stable `R-*` and `SC-*` IDs.
2. Resolve ambiguous examples, then agree on design, data model, risks, and contracts only where they add value.
3. Map each scenario to technical tasks and a test level in the traceability matrix. Use `TS-*` for implementation tasks and `EN-*` for shared technical prerequisites.
4. Implement ready tasks scenario by scenario. Apply Red → Green → Refactor within each testable technical slice.
5. Verify each completed scenario, then run the relevant regression checks.
6. Update the specification with evidence, deviations, and residual risks.
7. After implementing a new feature, add its current structure to the [feature map](../features/README.md) and its index; when changing an existing feature, review and update the description in the same change where needed.

## Documentation granularity

Describe a new feature with a brief card for its actual architecture in `docs/sdd/features/<module>/` and link to it from the index. The card contains a tree of key files with their roles and a compact Mermaid diagram of the verified flow between entry point, handlers, model, data, and integrations. It ends with a `## Tests` section (test files and the focused run command, or “No automated tests cover this feature yet.”) and an optional `**Data:**` link to the owner's [database map](../architecture/data-model/README.md) file. Keep environment state, such as “migration applied” or “imported locally”, out of cards; it belongs in SDD evidence. Update the map after a substantial change to these facts; an internal detail that does not affect it needs no repeated description. The SDD records decisions and evidence; the map describes the current implementation.

- When a specification's status changes, update its row in the [specification index](../specs/README.md) in the same change.
- When a feature's test files are added or moved, update the card's `## Tests` section.
- Small changes (styles, texts, a refactoring inside a file) do not touch the card.
- One feature SDD has one coordinating `README.md`; a brief description of the actual implementation lives separately in `features/`.
- Each document covers one topic; do not create empty or duplicate files.
- Acceptance scenarios describe externally observable behavior without implementation details.
- Design documents remain the source for technical boundaries and decisions.
- `traceability.md` links scenarios, technical tasks, and evidence without duplicating their details.
- An ADR is needed only for a lasting architecture change; ordinary scenario decisions belong in the feature design.
- Feature specifications link to `standards/` rather than copying stable backend, API, security, or testing rules.

## Completion criteria

Every scenario in scope is implemented or explicitly deferred, all required technical tasks are complete, and automated or documented manual evidence passed. Contracts and implementation are aligned. Focused tests for the changed feature and related components are run, or the reason they could not be run is recorded; general tests and a full suite run only at the user's explicit request under the [testing rules](testing-rules.md). The delivery report lists changed files, verification evidence, unrun checks, and residual risks.

Before marking a task complete, perform a self-check: compare the scope with the specification, file responsibilities with `backend-rules.md`, style with adjacent code, and the exact verification commands. Do not report a phase or task as complete while dependent work remains unfinished.
