# AI navigation: BreviERP

This is a short entry point for working in this backend repository. Its code and documentation define the actual rules, architecture, tools, and commands. Read only the sources needed for the specific task. This file applies throughout the repository unless a nested `AGENTS.md` provides more specific instructions.

## Product

BreviERP is the backend for Brevi, which sells in-house sewn products (Sewing: priced from fabrics, accessories, and sewing operations) and resold personal protective equipment (PPE: priced from versioned supplier offers). Terms are in the [glossary](docs/product/glossary.md).

## Project context

- [Architecture](docs/sdd/architecture/README.md) — modules, layers, and dependency boundaries.
- [Standards](docs/sdd/standards/README.md) — stable rules for development, delivery, and verification.
- [Specifications](docs/sdd/specs/README.md) — feature requirements, decisions, tasks, and completion evidence.
- [Implemented feature map](docs/sdd/features/README.md) — navigation by module, structure, and code entry points.
- [Contracts](docs/sdd/contracts/README.md) — public API behavior and contract synchronization.
- [Operations documentation](docs/sdd/operations/README.md) — startup, configuration, migrations, seed/jobs, and diagnostics.
- [Glossary](docs/product/glossary.md) — stable business terms.

Identify the module and affected documents from the request and code; do not assume another backend project uses the same implementation. `docs/sdd/` is the canonical backend documentation.

## Context search order

1. [Feature map](docs/sdd/features/README.md) → feature card → files from its "Code structure", "Tests", and "Data" lines.
2. Architecture and standards only for the specific rule in question.
3. Specifications hold decisions and history: open them only for acceptance IDs, a past decision, or a specification you are implementing. Do not search `docs/sdd/specs/`, code-audit, or evidence files for current behavior; the code wins.
4. Scope searches by path, for example `rg <term> src/Modules/<Module>` or `rg <term> tests/UnitTests`, not the whole repository.

For a new or substantially changed feature, update the [feature map](docs/sdd/features/README.md) under the [delivery rules](docs/sdd/standards/delivery-rules.md); do not document an internal detail again if it does not affect the map.

## Commands

| Purpose | Command | Cost |
| --- | --- | --- |
| Focused unit test | `dotnet test tests/UnitTests/UnitTests.csproj --filter <ClassOrName>` | fast |
| Architecture rules | `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj` | fast |
| Focused integration test | `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter <ClassOrName>` | slow; needs a separate PostgreSQL through `BREVIERP_CATALOG_TEST_CONNECTION` |
| Documentation change | `git diff --check` and check the changed links | fast |

Do not run a full `dotnet build`/`dotnet test BreviERP.sln`, all integration tests, or `Host.Seed` data commands unless the user asks; CI runs the full suite. A small fix that restores documented behavior needs no SDD, file-role map, or audit: fix it, run the focused test, and describe the change briefly.

## Task routing

| Task | Skill | Project rules |
| --- | --- | --- |
| Specify a new or changed feature | `$backend-feature-spec` | [Delivery rules](docs/sdd/standards/delivery-rules.md) |
| New capability outside existing modules, new module, or its DB schema | `$backend-feature-spec` | [Module rules](docs/sdd/standards/module-rules.md) |
| Implement an accepted specification | `$backend-feature-delivery` | [Delivery rules](docs/sdd/standards/delivery-rules.md) |
| Multiple agents at the user's explicit request | `$backend-feature-orchestrator` | [Specifications and dependencies](docs/sdd/specs/README.md) |
| Code audit / systemic remediation plan | `$backend-code-audit` / `$backend-remediation-plan` | [Architecture](docs/sdd/architecture/README.md) and [standards](docs/sdd/standards/README.md) |
| HTTP/API and external contracts | `$backend-api-contract-sync` | [Contracts](docs/sdd/contracts/README.md) |
| Operational jobs: catalogue import, pricing verification and activation | — | [Jobs overview](src/Jobs/README.md); run only through `Host.Seed`; runbooks in [data operations](docs/sdd/operations/data/migrations-and-seeding.md) |
| Find database tables and relationships | — | [Database map](docs/sdd/architecture/data-model/README.md): start with the tree, then read only the relevant owner's file |
| Database schema, migrations, and seed data | `$backend-database-change` | [Database rules](docs/sdd/standards/database-rules.md) and [data operations](docs/sdd/operations/data/migrations-and-seeding.md) |
| Verify changes / hand off for review | `$backend-change-verification` / `$backend-pr-handoff` | [Testing rules](docs/sdd/standards/testing-rules.md) |
| Differences between code and documentation | `$backend-docs-sync` | [Standards index](docs/sdd/standards/README.md) and [feature map](docs/sdd/features/README.md) |

## Process priority

The user's direct request and this repository's rules define the scope of work. For feature specification, implementation, and verification, use the relevant `backend-*` skills and project documents as the primary process. Apply additional skills only for a needed method; they do not add repeated approval, mandatory agents, a worktree, or a separate TDD cycle on top of the project process. Run checks and tests according to this repository's rules and the task requirements.

## Scope and verification

Complete all ready tasks within the agreed feature, phase, scenario, or task scope without a separate instruction for each file; stop at the scope boundary, an unresolved product decision, or a concrete blocker. The detailed procedure is in the [delivery rules](docs/sdd/standards/delivery-rules.md). Choose checks according to the [testing rules](docs/sdd/standards/testing-rules.md) and the configuration of affected projects. For new identifiers, read the [ID strategy](docs/sdd/standards/identifier-strategy.md).

For `Host.Seed`, migrations, and other actions that change database state, check the [project procedure](docs/sdd/operations/data/migrations-and-seeding.md) and the permitted environment.

Make commits, push, create a branch or PR, or take other actions that change remote state only at the user's direct request. Preserve unrelated working-tree changes. All links here point only to files in this repository; skills are available globally by name, and this repository has no local copies.
