# ADR-0004: Keyless delivery to Cloud Run, migrations as a job

**Status:** accepted · **Date:** 2026-11-13 (recorded from the current code)

## Context

The backend runs on Google Cloud. The production database and several integrations hold real business data. A leaked deploy key would expose everything, and a schema change must never run halfway with the API.

## Decision

- GitHub Actions builds the API and job images, runs every test against a PostgreSQL service, and pushes the images to Artifact Registry.
- Actions signs in to Google Cloud with **Workload Identity Federation** (OIDC). No service-account JSON key is stored anywhere.
- Before the API deploys, a Cloud Run Job (`Host.Seed`) applies the EF Core migrations of every module and the seed data. The new API revision deploys only after the job succeeds.
- The same job image also runs the scheduled work through Cloud Scheduler: the daily sync (KeepinCRM, Google Sheets, supplier prices) and the Telegram reminders.
- Runtime secrets (database, Gmail, Telegram, KeepinCRM, storage, proxy secret) come from **Secret Manager** as environment variables. `appsettings.json` holds only empty placeholders.
- Production deploys from the client's deploy repository after its `master` syncs from the development repository.

## Why

- No long-lived credential to leak or rotate.
- Migrations run once, in order, separate from scaling API instances.
- Cloud Run scales to zero, which fits a small team's traffic and budget.

## Trade-offs

- The pipeline is longer: seed job first, then deploy.
- Production deploy depends on a manual "sync fork" step. It is a deliberate gate, but it is easy to forget.
- There is no staging environment yet. Safety comes from tests in CI and from import screens with preview.
