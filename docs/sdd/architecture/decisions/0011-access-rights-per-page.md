# ADR-0011: Access is granted per admin page and action, and roles are edited by the owner

**Status:** accepted · **Date:** 2026-11-15 (recorded from the code; [ACC-001](../../specs/identity/002-access-rights/README.md)) · **Owner:** Виталий (project owner)

## Context

Brevi staff have different jobs: director, accountant, managers, workshop chief, SEO. Who may see prices, accounting or payroll changes over time, and the owner wants to change it without a developer. Fixed roles in code would need a release for every change.

## Options

- Fixed roles checked in code (`[Authorize(Roles=...)]`): rejected; every change is a release.
- An external authorization service: rejected; too heavy for a team of a few people.
- A catalogue of admin pages in code, with each role's saved rights in the database: chosen.

## Decision

- `Identity.Domain/Authorization/AccessCatalog.cs` lists every admin page with its actions (view, create, edit, delete) and default rights, in the order of the admin menu.
- Every work endpoint requires `access:<page>` (action from the HTTP method) or `access:<page>:<action>`; shared reads use `CatalogRead`; system settings use `Admin`.
- On «Права доступу» the owner sets rights per role and can create custom roles; the director always has every right.
- A test fails when an endpoint has no policy, or a named policy does not exist.
- Every new page or endpoint is added to the catalogue in the same change ([checklist](../security/authorization/roles-and-policies.md#new-feature-or-page-права-доступу-checklist)).

## Consequences

- The owner changes access without a release.
- Every new feature carries the extra step of adding its page to the catalogue.

## Sources

[Roles and policies](../security/authorization/roles-and-policies.md).
