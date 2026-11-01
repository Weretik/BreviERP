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
4. The API deployment step adds two Secret Manager references for `AdmToolsStorage__Login` and `AdmToolsStorage__Password`; verify other required variables and secrets in the environment configuration. If Secret Manager has `telegram-reminders-bot-token` and `telegram-reminders-webhook-secret`, the API also gets `Reminders__Telegram__BotToken` and `Reminders__Telegram__WebhookSecret`, and the Job gets the bot token; missing secrets are skipped ([Telegram reminders](../jobs/telegram-reminders.md)). The file alone does not prove they are already configured.

Default substitution values in `cloudbuild.yaml`: region `europe-central2`, Artifact Registry repository `api`, Cloud Run service `brevierp-api`, Job `brevierp-seed`. These are **configuration values**, not a check of the actual cloud environment. For another environment, verify substitution parameters and access to Artifact Registry, Cloud Run, Secret Manager, and PostgreSQL.

## Boundaries and verification

- The API Dockerfile exposes port `8080`; local `https` and `http` profiles are documented separately in the [run guide](../local-development/run-api.md).
- Running `Host.Seed` can change the database schema and data. Before rerunning it, check the target database, backups, previous Job result, and [migration rules](../data/migrations-and-seeding.md).
- The Cloud Build configuration has no test step. The [testing rules](../../standards/testing-rules.md) define required verification commands; do not assume they ran merely because the pipeline executed.
- After deployment, check Cloud Build, Cloud Run Job, and service status, logs from both hosts, `GET /health`, and the relevant feature contract scenarios. Do not claim a successful rollout without these results.

## Host.Seed restore inputs

Host.Seed references the CatalogImport and SewingPricing job projects. Their project files must be copied into the Docker build context before dotnet restore, along with all other transitive project files. Copying them only in the later COPY . . leaves their NuGet assets absent when publish --no-restore runs. Local bin/obj can mask this container-only failure.

ContainerRestoreCoverageTests in tests/ArchitectureTests traverses the actual project-reference graph for Host.Api and Host.Seed and checks that every project is copied before restore. Run: dotnet test tests/ArchitectureTests/ArchitectureTests.csproj --filter FullyQualifiedName~ContainerRestoreCoverageTests. The API image can build successfully while a later Host.Seed image failure still blocks deployment of that API revision; a successful GitHub verification is not a successful Cloud Build deployment.

Local verification on 2026-11-01: replaying the Dockerfile restore/publish steps with only tracked source in a fresh directory reproduced NETSDK1004 for both missing job projects before the fix and completed successfully after it. This was an SDK replay; no Docker engine was available. The full Release solution build succeeded, and the full solution test run finished with exit code 0: 623 passed, 0 failed, 3 skipped (PDF proofs require explicitly configured Chromium and font). The run used both CRM and Catalog test connection variables against isolated temporary local databases. The P3 migration regression now seeds its compatible quote before downgrading the schema; CRM and Sewing fixture cleanup waits for connections to close instead of requiring forced backend termination. GitHub CI now supplies the CRM test connection as well, so those database tests are executed. This evidence does not establish a successful Cloud Run rollout.
