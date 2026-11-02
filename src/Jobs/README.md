# Operational jobs

- [Catalogue import](CatalogImport/README.md): source validation, transactional initial transfer and provenance.
- [Sewing pricing](SewingPricing/README.md): initial pricing baseline and non-publishable arithmetic verification.

Host.Seed owns command selection and normal migration/seed lifecycle. Job projects reuse Catalog/Reference domain rules and infrastructure; Domain, Application and Host.Api do not reference these projects. Both jobs use the same Brevi PostgreSQL database. This separation changes code ownership, not storage or CLI flags.


The [Telegram reminders](../../docs/sdd/operations/jobs/telegram-reminders.md) daily run (`--reminders-dispatch=true`) is a Host.Seed mode that calls the Reminders module dispatcher, deployed as the `brevierp-reminders` Cloud Run Job; it is not a project in `src/Jobs`.

The [daily sync job](../../docs/sdd/operations/jobs/sync-job.md) (`--sync=true`, Cloud Run Job `brevierp-sync`) is also a Host.Seed mode: scheduled refreshes from external systems and imports run there as steps (`Host.Seed/Jobs/Sync`), starting with the KeepinCRM suppliers refresh. Add new scheduled imports as steps of this job rather than as new jobs.

The [CRM history offline dry-run](../../docs/sdd/operations/data/crm-history-dry-run.md) is an explicit Host.Seed mode using the owning Crm Application/Infrastructure adapters. It has no separate job project and never opens a target database or calls migration/seed/dispatch.

The [dedicated cloud catalogue import](../../docs/sdd/operations/data/catalog-import-cloud.md) runs the same owning job in a separate Cloud Run Job with explicit target/source pins and private persistent reports; it is independent of normal deployment migrations and scheduled reminders.
