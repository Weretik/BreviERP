# Architecture decisions

Keep accepted decisions that change long-term boundaries, contracts, or approaches across multiple modules in this directory. Feature-specific decisions are in [specifications](../../specs/README.md), while current architecture rules are in the [architecture overview](../README.md).

| # | Decision | Status |
| --- | --- | --- |
| [0001](0001-modular-monolith.md) | Modular monolith with a database schema per module | Accepted |
| [0002](0002-cross-module-ports.md) | Modules talk through ports implemented in the host | Accepted |
| [0003](0003-cookie-session-behind-proxy.md) | Cookie session + CSRF behind a Cloudflare proxy | Accepted |
| [0004](0004-keyless-cloud-run-delivery.md) | Keyless delivery to Cloud Run, migrations as a job | Accepted |
| [0005](0005-nextjs-site-on-workers.md) | Public site on Next.js + Cloudflare Workers, reading the ERP API | Accepted |

For a new decision, create `NNNN-short-name.md` and record:

1. Decision status and date, participants or responsible team.
2. Context and verified sources in code, configuration, or current documents.
3. Options considered and reasons for the choice.
4. Consequences for modules, data, API, security, operations, and tests.
5. Rollout, backward-compatibility, and review plan if needed.

After acceptance, update the corresponding architecture or standards document; the ADR explains why, while the current rule should remain in one place. Do not create retrospective ADRs without sources.
