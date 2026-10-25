# Operational jobs

- [Catalogue import](CatalogImport/README.md): source validation, transactional initial transfer and provenance.
- [Sewing pricing](SewingPricing/README.md): initial pricing baseline and non-publishable arithmetic verification.

Host.Seed owns command selection and normal migration/seed lifecycle. Job projects reuse Catalog/Reference domain rules and infrastructure; Domain, Application and Host.Api do not reference these projects. Both jobs use the same Brevi PostgreSQL database. This separation changes code ownership, not storage or CLI flags.

