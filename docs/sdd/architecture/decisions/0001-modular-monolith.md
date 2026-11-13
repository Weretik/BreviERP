# ADR-0001: Modular monolith with a database schema per module

**Status:** accepted · **Date:** 2026-11-13 (recorded from the current code)

## Context

Brevi ran on a Google Sheets calculator plus a separate workshop app. The new system had to cover many areas: pricing, offers, CRM, production, warehouse, purchasing, accounting, and site content. It is built and run by one developer, for a small team (owner, two managers, workshop chief). Areas were added one after another, and some were large enough to rebuild later.

## Decision

Build one deployable ASP.NET Core application split into business modules: Identity, Reference, Catalog, Crm, Production, Warehouse, Purchasing, Accounting, Content, Media, Reminders and Ai.

- Each module has four projects: Domain, Application, Infrastructure and Api.
- Each module owns its own PostgreSQL schema and its own EF Core `DbContext` and migrations.
- No foreign keys cross schemas. Another module's data is referenced by its ID as a plain value.
- Shared code lives in `BuildingBlocks` and holds no business rules.
- Architecture tests check the allowed project references.

## Why

- One deploy, one database, and one place to debug suit a single developer and a small user base. Microservices would add network calls, distributed transactions, and more infrastructure, with no benefit at this scale.
- Hard module edges keep a 70k+ line codebase understandable. A new area, such as the Warehouse module that took fabric stock over from Production, is added without touching the internals of others.
- A schema per module keeps the option to move a module into its own service later.

## Trade-offs

- No database-level integrity across modules. Consistency between modules is kept by application code and tests.
- Reports that join data from several modules must go through queries in each module, not a single SQL join.
- All modules share one process. A heavy job, such as a PDF render or an import, shares resources with the API. Long jobs run in separate Cloud Run jobs to limit this.
