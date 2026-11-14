# Operational jobs

- [Catalogue import](CatalogImport/README.md): source validation, transactional initial transfer and provenance.
- [Sewing pricing](SewingPricing/README.md): initial pricing baseline and non-publishable arithmetic verification.

Host.Seed owns command selection and normal migration/seed lifecycle; its runners only read arguments and call the job project (`CatalogImport.Execution.CatalogImportCommand`, `SewingPricing.Jobs.Activation.PricingActivationCommand`) or a module command. Job projects reuse Catalog/Reference domain rules and infrastructure; Domain, Application and Host.Api do not reference these projects. Both jobs use the same Brevi PostgreSQL database. This separation changes code ownership, not storage or CLI flags.


The [Telegram reminders](../../docs/sdd/operations/jobs/telegram-reminders.md) daily run (`--reminders-dispatch=true`) is a Host.Seed mode that calls the Reminders module dispatcher, deployed as the `brevierp-reminders` Cloud Run Job; it is not a project in `src/Jobs`.

Scheduled work runs as [«Фонові задачі»](../../docs/sdd/operations/jobs/background-tasks.md) (`--tasks=key:runId,...`, Cloud Run Job `brevierp-tasks`, started by the scheduler tick); the old [daily sync job](../../docs/sdd/operations/jobs/sync-job.md) (`--sync=true`, `brevierp-sync`) runs the same runners until the switch-over. Each runner in `Host.Seed/Jobs/Tasks` only sends its owning module's command (`services.SendAsync(new ...Command(...))`), logs the outcome and returns a short Ukrainian summary. Add new scheduled work as a task rather than as a new job.

The [dedicated cloud catalogue import](../../docs/sdd/operations/data/catalog-import-cloud.md) runs the same owning job in a separate Cloud Run Job with explicit target/source pins and private persistent reports; it is independent of normal deployment migrations and scheduled reminders.
