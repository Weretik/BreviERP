# Architecture decisions

Keep accepted decisions that change long-term boundaries, contracts, or approaches across multiple modules in this directory. Feature-specific decisions are in [specifications](../../specs/README.md), while current architecture rules are in the [architecture overview](../README.md).

| # | Decision | Status |
| --- | --- | --- |
| [0001](0001-modular-monolith.md) | Modular monolith with a database schema per module | Accepted |
| [0002](0002-cross-module-ports.md) | Modules talk through ports implemented in the host | Superseded by 0006 |
| [0003](0003-cookie-session-behind-proxy.md) | Cookie session + CSRF behind a Cloudflare proxy | Accepted |
| [0004](0004-keyless-cloud-run-delivery.md) | Keyless delivery to Cloud Run, migrations as a job | Accepted |
| [0005](0005-nextjs-site-on-workers.md) | Public site on Next.js + Cloudflare Workers, reading the ERP API | Accepted |
| [0006](0006-module-contracts.md) | Modules talk through their own Contracts projects | Accepted |
| [0007](0007-use-cases-through-mediator.md) | Every use case is a command or query sent through Mediator from a Minimal API endpoint | Accepted |
| [0008](0008-result-and-typed-errors.md) | Business rules return a Result with a typed error; exceptions only for the unexpected | Accepted |
| [0009](0009-transactional-outbox.md) | Side effects leave through a transactional outbox per module | Accepted |
| [0010](0010-scheduler-tick-for-background-tasks.md) | One scheduler tick runs the outbox and every due background task | Accepted |
| [0011](0011-access-rights-per-page.md) | Access is granted per admin page and action, and roles are edited by the owner | Accepted |
| [0012](0012-media-library-on-cdn.md) | Photos live on the CDN; the Media module keeps a library of them by id | Accepted |
| [0013](0013-folder-axiom.md) | A code folder holds at most six files | Accepted |
| [0014](0014-external-data-through-integrations.md) | Data from outside systems comes in through scheduled imports behind module ports | Accepted |
| [0015](0015-openapi-generated-from-code.md) | The OpenAPI contract is generated from the code at build | Accepted |
| [0016](0016-domain-events-to-activity-log.md) | Domain events reach the activity log through each module's outbox | Accepted |
| [0017](0017-opentelemetry-traces-to-cloud-trace.md) | Requests are traced with OpenTelemetry straight to Google Cloud Trace; logs are Cloud Logging JSON with the trace id | Accepted |

For a new decision, create `NNNN-short-name.md` and record:

1. Decision status and date, participants or responsible team.
2. Context and verified sources in code, configuration, or current documents.
3. Options considered and reasons for the choice.
4. Consequences for modules, data, API, security, operations, and tests.
5. Rollout, backward-compatibility, and review plan if needed.

After acceptance, update the corresponding architecture or standards document; the ADR explains why, while the current rule should remain in one place. Do not create retrospective ADRs without sources.

## Key decisions that must have an ADR

Every backend keeps an accepted ADR for each area below. A full architecture audit checks this list and reports each missing or outdated ADR as a finding; it also checks that the [C4 diagrams](../c4/README.md) match the code.

| Area | ADR here |
| --- | --- |
| Architecture style and module boundaries, data ownership (schema per module) | 0001 |
| Communication between modules | 0006 (supersedes 0002) |
| Use-case shape (CQRS, mediator, endpoints) | 0007 |
| Errors and validation | 0008 |
| Authentication and authorization | 0003, 0011 |
| Side effects and integration messages | 0009, 0016 |
| Domain events and the activity history | 0016 |
| Background work and scheduling | 0010 |
| Database migrations and their order in a release | 0004 |
| Deployment, hosting and secrets | 0004 |
| Network edge (proxy, domains, cookies) | 0003 |
| Files and media storage | 0012 |
| Code organization (folders) | 0006, 0013 |
| Main external systems | 0014 |
| Public site and how it reads the backend | 0005 |
| Public API contract (OpenAPI) | 0015 |
| Observability (traces, structured logs) | 0017 |

A new decision of this kind (a new module, a new external system, a new way to run work or to deploy) gets an ADR in the same change, and the C4 diagrams are updated with it. Frontend-only decisions live in brevi-web ([workspace ADRs](https://github.com/Weretik/brevi-web/blob/master/docs/architecture/adr/README.md)).
