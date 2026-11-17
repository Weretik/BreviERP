# Containers and Cloud Run deployment

**Verification status:** First deploy through this workflow succeeded on 2026-11-02 from the old deploy fork `pnevman-creator/brevi-api`. Since [ADR-0018](../../architecture/decisions/0018-deploy-on-merge-from-main-repository.md) the deploy runs in `Weretik/BreviERP`; its runs are in that repository's Actions tab.

## Sources

- [`.github/workflows/deploy-cloudrun.yml`](../../../../.github/workflows/deploy-cloudrun.yml) defines verification, image build and publication, migration, and deployment order. It follows the same scheme as KedrStore: GitHub Actions builds the images, Cloud Run deploys the ready containers. Deployment runs in `Weretik/BreviERP`: every merge to `master` starts it ([ADR-0018](../../architecture/decisions/0018-deploy-on-merge-from-main-repository.md)).
- The [Host.Api Dockerfile](../../../../src/Bootstrapper/Host.Api/Dockerfile) and [Host.Seed Dockerfile](../../../../src/Bootstrapper/Host.Seed/Dockerfile) define images based on .NET 10 SDK/runtime.
- [`Host.Seed/Program.cs`](../../../../src/Bootstrapper/Host.Seed/Program.cs) calls `MigrateAppAsync()` and `SeedAppAsync()`; [`Host.Api/Program.cs`](../../../../src/Bootstrapper/Host.Api/Program.cs) starts the HTTP host.

## Configured sequence

Every pull request and push runs the `verify` job: restore, Release build, and the full solution test run against a PostgreSQL service. The `deploy` job runs only on a push to `master` of `Weretik/BreviERP` (a merged pull request) after `verify` passes; in any fork it is skipped:

1. Docker builds `host-api` and `host-seed` images from their Dockerfiles and pushes them to Artifact Registry `europe-central2-docker.pkg.dev/brevistoreapi/api` with the commit SHA tag.
2. The workflow creates or updates the `brevierp-seed` Cloud Run Job: one task, concurrency 1, at most one retry. It then creates `brevierp-reminders` if missing (copied from the seed job) and updates its image, `--reminders-dispatch=true` argument and bot token. It does the same for the shared `brevierp-sync` job (argument `--sync=true`, secrets of every sync step, now `keepincrm-api-key`, `storage-login` and `storage-password`, plus the supplier cabinet secrets below; see [sync job](../jobs/sync-job.md), runs Mon–Fri 09:00 Kyiv). The workflow never executes these jobs. The seed job also gets `ADMIN_DEFAULT_PASSWORD` from `admin-default-password` (first administrator, see [Cloudflare API proxy](cloudflare-api-proxy.md#setup-order)).
3. It executes `brevierp-seed` and waits for completion. Only then does it deploy `Host.Api` to the `brevierp-api` service.
4. The API deployment sets `Accounting__Files__Bucket` to `ACCOUNTING_FILES_BUCKET` (the private bucket of «Бухгалтерія» files, see [configuration](../configuration/configuration-and-secrets.md)) and adds Secret Manager references for `AdmToolsStorage__Login` and `AdmToolsStorage__Password`, and always adds `Reminders__Telegram__BotToken` and `Reminders__Telegram__WebhookSecret` from `telegram-reminders-bot-token` and `telegram-reminders-webhook-secret`, `KeepinCrm__ApiKey` from `keepincrm-api-key`, and `TrustedProxy__SharedSecret` from `api-proxy-shared-secret` ([Cloudflare API proxy](cloudflare-api-proxy.md)); a missing secret fails the deploy ([Telegram reminders](../jobs/telegram-reminders.md)). When `CRM_GMAIL_ENABLED` is `'true'` it also adds `Crm__Gmail__ClientId` and `Crm__Gmail__ClientSecret` from `crm-gmail-client-id` and `crm-gmail-client-secret` ([Gmail for КП](../configuration/crm-gmail.md)); while it is `'false'` КП sending stays off. The Трєле dealer login (`trele-login`, `trele-password` → `SupplierCabinets__Trele__Login`, `…__Password`) is attached to the API and `brevierp-sync` only when both exist (step «Find supplier cabinet secrets»); without them the deploy succeeds and «Імпорт цін» reports that the cabinet logins are not added.
5. Finally it creates or updates the `brevierp-catalog-import` job without executing it ([cloud catalogue import](../data/catalog-import-cloud.md)).

Region, project, Artifact Registry repository, service and job names are `env` values at the top of the workflow. These are **configuration values**, not a check of the actual cloud environment.

## Google Cloud access

The deploy job signs in without a key, through Workload Identity Federation: GitHub issues a short-lived OIDC token, the pool `github` / provider `github-oidc` in project `brevistoreapi` accepts it only for repository `Weretik/BreviERP` (attribute condition `assertion.repository=='Weretik/BreviERP'`), and the job acts as service account `github-deploy@brevistoreapi.iam.gserviceaccount.com`. Service account keys are blocked by the organization policy `iam.disableServiceAccountKeyCreation`. The service account needs `roles/iam.workloadIdentityUser` for that repository principal set and, in project `brevistoreapi`:

- Artifact Registry Writer — push images;
- Cloud Run Admin — create, update and execute jobs, deploy the service;
- Service Account User — run jobs and the service as their runtime service account;
- Storage Object Admin on bucket `brevistoreapi-catalog-import` — create the report folder marker.

The Cloud Resource Manager API (`cloudresourcemanager.googleapis.com`) must be enabled in the project: `gcloud run jobs replace`, used for the reminders and catalogue import jobs, fails with `SERVICE_DISABLED` without it when called from outside Google Cloud.

The runtime service account of `brevierp-api` (not the deployer) needs Storage Object Admin on bucket `brevistoreapi-accounting-files`; the bucket has uniform access and public access prevention. Without the bucket or the role the API still starts, and adding a file answers 503 «Сховище файлів зараз недоступне».

Secret values are never read by the workflow; Cloud Run resolves the references with the runtime service account, which needs Secret Manager Secret Accessor.

Only one pipeline may deploy at a time. The previous Cloud Build trigger on the fork (`cloudbuild.yaml`) must stay disabled; `cloudbuild.yaml` was removed from the repository.

## Boundaries and verification

- The API Dockerfile exposes port `8080`; local `https` and `http` profiles are documented separately in the [run guide](../local-development/run-api.md).
- Running `Host.Seed` can change the database schema and data. Before rerunning it, check the target database, backups, previous Job result, and [migration rules](../data/migrations-and-seeding.md).
- After deployment, check the GitHub Actions run, Cloud Run Job and service status, logs from both hosts, `GET /health`, and the relevant feature contract scenarios. Do not claim a successful rollout without these results.

## Host.Seed restore inputs

Host.Seed references the CatalogImport and SewingPricing job projects. Their project files must be copied into the Docker build context before dotnet restore, along with all other transitive project files. Copying them only in the later COPY . . leaves their NuGet assets absent when publish --no-restore runs. Local bin/obj can mask this container-only failure.

ContainerRestoreCoverageTests in tests/ArchitectureTests traverses the actual project-reference graph for Host.Api and Host.Seed and checks that every project is copied before restore. Run: dotnet test tests/ArchitectureTests/ArchitectureTests.csproj --filter FullyQualifiedName~ContainerRestoreCoverageTests. The API image can build successfully while a later Host.Seed image failure still blocks deployment of that API revision; a green `verify` job is not a successful deployment.

## Switching from the deploy fork

One-time steps for [ADR-0018](../../architecture/decisions/0018-deploy-on-merge-from-main-repository.md), in Cloud Shell of project `brevistoreapi`, before the change that moves the `deploy` job is merged:

```bash
gcloud config set project brevistoreapi

# 1. Accept GitHub tokens only from Weretik/BreviERP.
gcloud iam workload-identity-pools providers update-oidc github-oidc \
  --location=global --workload-identity-pool=github \
  --attribute-condition="assertion.repository=='Weretik/BreviERP'"

# 2. Let that repository act as the deploy service account.
gcloud iam service-accounts add-iam-policy-binding \
  github-deploy@brevistoreapi.iam.gserviceaccount.com \
  --role=roles/iam.workloadIdentityUser \
  --member="principalSet://iam.googleapis.com/projects/443622778364/locations/global/workloadIdentityPools/github/attribute.repository/Weretik/BreviERP"

# 3. Check: prints the new condition.
gcloud iam workload-identity-pools providers describe github-oidc \
  --location=global --workload-identity-pool=github --format="value(attributeCondition)"
```

If step 2 is refused at the service-account level, grant the same member the role on the project (`gcloud projects add-iam-policy-binding brevistoreapi --role=roles/iam.workloadIdentityUser --member=...`), as KedrStore does. After the first green deploy from `Weretik/BreviERP`, the old binding for `pnevman-creator/brevi-api` can be removed with `gcloud iam service-accounts remove-iam-policy-binding` and the same arguments.
