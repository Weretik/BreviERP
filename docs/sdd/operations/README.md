# Operations

This section explains how the server application is configured, started, and diagnosed. It describes operational facts rather than feature design; feature-specific checks remain in the corresponding specification.

```text
operations/
├── local-development/  API startup and local URLs
├── configuration/      configuration sources and secret handling
├── data/               migrations and seeding through Host.Seed
├── diagnostics/        health checks, Swagger, and logs
├── deployment/         containers and Cloud Run deployment
└── jobs/               background job status and requirements for future runbooks
```

## Read by task

- Start the server application locally: [local development](local-development/run-api.md)
- Configure a machine or fix missing settings: [configuration and secrets](configuration/configuration-and-secrets.md)
- Prepare a separate `Host.Seed` run that changes the database: [migrations and seeding](data/migrations-and-seeding.md)
- Check a running API or reproduce an endpoint: [diagnostics](diagnostics/health-logs-swagger.md)
- Understand the configured container and deployment pipeline: [Cloud Run deployment](deployment/cloud-run-deploy.md)
- Check background job support: [jobs and runbooks](jobs/README.md)
- Configure the Telegram bot and the daily reminder run: [Telegram reminders](jobs/telegram-reminders.md)

Do not place passwords, tokens, full connection strings, or production endpoints in this documentation.

- Configure delegated Gmail OAuth without enabling delivery: [Gmail setup](crm-gmail-oauth.md).

- Temporary public access to every API and restoration: [anonymous API mode](configuration/temporary-anonymous-api.md).

[CRM archive offline dry-run](data/crm-history-dry-run.md) documents frozen inputs, private diagnostic/staging outputs, no-write guarantees and the EN-006 source/ownership gates.

- Import the catalogue into an explicitly selected cloud database: [dedicated catalogue Cloud Run Job](data/catalog-import-cloud.md).
