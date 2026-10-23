# Operations

This section explains how the server application is configured, started, and diagnosed. It describes operational facts rather than feature design; feature-specific checks remain in the corresponding specification.

```text
operations/
├── local-development/  API startup and local URLs
├── configuration/      configuration sources and secret handling
├── data/               migrations and seeding through Host.Seed
├── diagnostics/        health checks, Swagger, and logs
├── deployment/         container and Cloud Build configuration
└── jobs/               background job status and requirements for future runbooks
```

## Read by task

- Start the server application locally: [local development](local-development/run-api.md)
- Configure a machine or fix missing settings: [configuration and secrets](configuration/configuration-and-secrets.md)
- Prepare a separate `Host.Seed` run that changes the database: [migrations and seeding](data/migrations-and-seeding.md)
- Check a running API or reproduce an endpoint: [diagnostics](diagnostics/health-logs-swagger.md)
- Understand the configured container and deployment pipeline: [Cloud Build](deployment/cloud-build.md)
- Check background job support: [jobs and runbooks](jobs/README.md)

Do not place passwords, tokens, full connection strings, or production endpoints in this documentation.
