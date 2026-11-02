# Containers and Cloud Run deployment

**Verification status:** Static review of repository files on 2026-11-02. This document describes the configured pipeline; actual Google Cloud resource state and the most recent successful deployment were not checked.

## Sources

- [`.github/workflows/deploy-cloudrun.yml`](../../../../.github/workflows/deploy-cloudrun.yml) defines verification, image build and publication, migration, and deployment order. It follows the same scheme as KedrStore: GitHub Actions builds the images, Cloud Run deploys the ready containers. Deployment runs from the deploy fork `pnevman-creator/brevi-api`: changes land in `Weretik/BreviERP`, the fork's `master` is synced from it, and that push starts the deploy.
- The [Host.Api Dockerfile](../../../../src/Bootstrapper/Host.Api/Dockerfile) and [Host.Seed Dockerfile](../../../../src/Bootstrapper/Host.Seed/Dockerfile) define images based on .NET 10 SDK/runtime.
- [`Host.Seed/Program.cs`](../../../../src/Bootstrapper/Host.Seed/Program.cs) calls `MigrateAppAsync()` and `SeedAppAsync()`; [`Host.Api/Program.cs`](../../../../src/Bootstrapper/Host.Api/Program.cs) starts the HTTP host.

## Configured sequence

Every pull request and push runs the `verify` job: restore, Release build, and the full solution test run against a PostgreSQL service. The `deploy` job runs only on a push to `master` of `pnevman-creator/brevi-api` (normally the fork sync) after `verify` passes; in `Weretik/BreviERP` it is skipped:

1. Docker builds `host-api` and `host-seed` images from their Dockerfiles and pushes them to Artifact Registry `europe-central2-docker.pkg.dev/brevistoreapi/api` with the commit SHA tag.
2. The workflow creates or updates the `brevierp-seed` Cloud Run Job: one task, concurrency 1, at most one retry. It then creates `brevierp-reminders` if missing (copied from the seed job) and updates its image, `--reminders-dispatch=true` argument and bot token. The workflow never executes the reminders job.
3. It executes `brevierp-seed` and waits for completion. Only then does it deploy `Host.Api` to the `brevierp-api` service.
4. The API deployment adds Secret Manager references for `AdmToolsStorage__Login` and `AdmToolsStorage__Password`, and always adds `Reminders__Telegram__BotToken` and `Reminders__Telegram__WebhookSecret` from `telegram-reminders-bot-token` and `telegram-reminders-webhook-secret`; a missing secret fails the deploy ([Telegram reminders](../jobs/telegram-reminders.md)). It writes `Crm__Delivery__Enabled` from the workflow's `CRM_DELIVERY_ENABLED` value.
5. Finally it creates or updates the `brevierp-catalog-import` job without executing it ([cloud catalogue import](../data/catalog-import-cloud.md)).

Region, project, Artifact Registry repository, service and job names are `env` values at the top of the workflow. These are **configuration values**, not a check of the actual cloud environment.

## Google Cloud access

The deploy job signs in with a service account JSON key stored in the secret `GCP_SA_KEY` of the fork repository (Settings → Secrets and variables → Actions), as KedrStore does. GitHub Actions must be enabled in the fork. Until that secret exists, the deploy job only logs a notice and skips; build and tests still run. The service account needs, in project `brevistoreapi`:

- Artifact Registry Writer — push images;
- Cloud Run Admin — create, update and execute jobs, deploy the service;
- Service Account User — run jobs and the service as their runtime service account;
- Storage Object Admin on bucket `brevistoreapi-catalog-import` — create the report folder marker.

Secret values are never read by the workflow; Cloud Run resolves the references with the runtime service account, which needs Secret Manager Secret Accessor.

Only one pipeline may deploy at a time. The previous Cloud Build trigger on the fork (`cloudbuild.yaml`) must stay disabled; `cloudbuild.yaml` was removed from the repository.

## Boundaries and verification

- The API Dockerfile exposes port `8080`; local `https` and `http` profiles are documented separately in the [run guide](../local-development/run-api.md).
- Running `Host.Seed` can change the database schema and data. Before rerunning it, check the target database, backups, previous Job result, and [migration rules](../data/migrations-and-seeding.md).
- After deployment, check the GitHub Actions run, Cloud Run Job and service status, logs from both hosts, `GET /health`, and the relevant feature contract scenarios. Do not claim a successful rollout without these results.

## Host.Seed restore inputs

Host.Seed references the CatalogImport and SewingPricing job projects. Their project files must be copied into the Docker build context before dotnet restore, along with all other transitive project files. Copying them only in the later COPY . . leaves their NuGet assets absent when publish --no-restore runs. Local bin/obj can mask this container-only failure.

ContainerRestoreCoverageTests in tests/ArchitectureTests traverses the actual project-reference graph for Host.Api and Host.Seed and checks that every project is copied before restore. Run: dotnet test tests/ArchitectureTests/ArchitectureTests.csproj --filter FullyQualifiedName~ContainerRestoreCoverageTests. The API image can build successfully while a later Host.Seed image failure still blocks deployment of that API revision; a green `verify` job is not a successful deployment.

## P6 send-disabled deployment

The `CRM_DELIVERY_ENABLED` workflow value defaults to false and is explicitly written to Crm__Delivery__Enabled when the API revision is deployed. This prevents a retained Cloud Run environment override from activating mail while Gmail acceptance is deferred. Enabling it is a separate owner-authorized operation after EN-004; source defaults alone do not prove the current live value. Existing anonymous access and other environment settings remain unchanged.
