# Operational jobs

- [Catalogue import](CatalogImport/README.md): source validation, transactional initial transfer and provenance.
- [Sewing pricing](SewingPricing/README.md): initial pricing baseline and non-publishable arithmetic verification.

Host.Seed owns command selection and normal migration/seed lifecycle. Job projects reuse Catalog/Reference domain rules and infrastructure; Domain, Application and Host.Api do not reference these projects. Both jobs use the same Brevi PostgreSQL database. This separation changes code ownership, not storage or CLI flags.


The [Telegram reminders](../../docs/sdd/operations/jobs/telegram-reminders.md) daily run (`--reminders-dispatch=true`) is a Host.Seed mode that calls the Reminders module dispatcher; it is not a project in `src/Jobs`.
