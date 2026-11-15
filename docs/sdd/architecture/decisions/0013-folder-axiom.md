# ADR-0013: A code folder holds at most six files

**Status:** accepted · **Date:** 2026-11-14 (owner's rule) · **Owner:** Виталий (project owner)

## Context

After the Contracts restructure ([ADR-0006](0006-module-contracts.md)) some folders still held fifteen to thirty files, so a reader could not see which files belong together.

## Options

- No limit, rely on review: rejected; folders grew back.
- Split by kind of class (`Entities/`, `Dtos/`): rejected by the module rules.
- At most six files per folder, split by meaning, checked by a test: chosen.

## Decision

- A folder in `src/` and `tests/` holds at most six code files; when it would hold more, it splits by meaning into named subfolders of at least two files each.
- No empty folders, no wrapper folders, no new one-file folders. Only EF `Migrations/` is exempt.
- `tests/ArchitectureTests/Folders/FolderSizeTests.cs` enforces it; every audit lists violations.

## Consequences

- The folder tree reads like the business; more folders and longer namespaces.

## Sources

[Module rules](../../standards/module-rules.md). The same rule applies to KedrStore and the frontends.
