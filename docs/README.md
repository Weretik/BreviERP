# BreviERP documentation

This directory contains versioned BreviERP engineering documentation. Backend documentation is maintained alongside code changes as docs-as-code.

## Backend SDD

Start with the [SDD documentation map](sdd/README.md) if you do not yet know which section you need.

- [Architecture](sdd/architecture/README.md) — actual backend topology, modules, and layer boundaries.
- [Engineering rules](sdd/standards/README.md) — backend, API, data, security, testing, and delivery rules.
- [Operations](sdd/operations/README.md) — local startup, configuration, data, diagnostics, and background job status.
- [Specifications](sdd/specs/README.md) — feature specifications and evidence of completion.
- [Feature map](sdd/features/README.md) — implemented scenarios by module, flows, and code entry points.
- [API contracts](sdd/contracts/README.md) — versioned OpenAPI contracts and handoff rules for the frontend team.

## Product reference

- [Glossary](product/glossary.md) — stable business terms shared across features.

## Ideas

- [Sign-in and access like Kedr](ideas/auth-like-kedr.md) — auth audit and a step-by-step plan (Russian; not an accepted decision).
- [Public catalog for the new brevi.com.ua site](ideas/public-site-catalog.md) — backend requirements and OpenCart import for the Next.js site (Russian; not an accepted decision).

## Placement principle

Stable rules belong in `sdd/architecture` or `sdd/standards`. Current feature structure and code navigation belong in `sdd/features/`; decisions, requirements, tasks, and verification evidence belong in the corresponding `sdd/specs/` directory. Public machine-readable API contracts are stored separately in `sdd/contracts/`.
