# Containerization and Cloud Build configuration

**Verification status:** Static review of repository files on 2026-10-15. This document describes the configured pipeline; actual Google Cloud resource state and the most recent successful deployment were not checked.

## Sources

- [`cloudbuild.yaml`](../../../../cloudbuild.yaml) defines build, image publication, migration, and deployment order.
- The [Host.Api Dockerfile](../../../../src/Bootstrapper/Host.Api/Dockerfile) and [Host.Seed Dockerfile](../../../../src/Bootstrapper/Host.Seed/Dockerfile) define images based on .NET 10 SDK/runtime.
- [`Host.Seed/Program.cs`](../../../../src/Bootstrapper/Host.Seed/Program.cs) calls `MigrateAppAsync()` and `SeedAppAsync()`; [`Host.Api/Program.cs`](../../../../src/Bootstrapper/Host.Api/Program.cs) starts the HTTP host.

## Configured sequence

1. Docker builds `host-api` and `host-seed` images from their Dockerfiles and publishes them to Artifact Registry with the `$COMMIT_SHA` tag.
2. Cloud Build creates or updates the `Host.Seed` Cloud Run Job: one task, concurrency 1, at most one retry.
3. Cloud Build executes the Job and waits for completion. Only then does it deploy `Host.Api` to Cloud Run.
4. The API deployment step adds two Secret Manager references for `AdmToolsStorage__Login` and `AdmToolsStorage__Password`; verify other required variables and secrets in the environment configuration. The file alone does not prove they are already configured.

Default substitution values in `cloudbuild.yaml`: region `europe-central2`, Artifact Registry repository `api`, Cloud Run service `brevierp-api`, Job `brevierp-seed`. These are **configuration values**, not a check of the actual cloud environment. For another environment, verify substitution parameters and access to Artifact Registry, Cloud Run, Secret Manager, and PostgreSQL.

## Boundaries and verification

- The API Dockerfile exposes port `8080`; local `https` and `http` profiles are documented separately in the [run guide](../local-development/run-api.md).
- Running `Host.Seed` can change the database schema and data. Before rerunning it, check the target database, backups, previous Job result, and [migration rules](../data/migrations-and-seeding.md).
- The Cloud Build configuration has no test step. The [testing rules](../../standards/testing-rules.md) define required verification commands; do not assume they ran merely because the pipeline executed.
- After deployment, check Cloud Build, Cloud Run Job, and service status, logs from both hosts, `GET /health`, and the relevant feature contract scenarios. Do not claim a successful rollout without these results.
