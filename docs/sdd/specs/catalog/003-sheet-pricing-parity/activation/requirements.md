# Readiness rules and acceptance

- R-A01: enumerate every imported `Прайс на сайт` row with type `Швейна продукція` or `ЗІЗ`. Preserve SKU/source cell, workbook/hash/capture date and report/check instant. Missing current identity must appear as a blocker; it must not silently disappear. Other types are reported as unsupported, not skipped.
- R-A02: use existing strict Application evaluation and current read ports; never override approval or copy price formulas. Include current rule/offer/settings/reference IDs and approval flags where applicable. Strict readiness requires a price object and no strict blockers.
- R-A03: source numeric outputs absent/invalid are separate issues with cell addresses, not proof of formula error and not zero. A missing supplier cost is not globally fatal when approved external inputs supply every tier; strict Domain evaluation remains authoritative.
- R-A04: report current Sewing publication independently from current calculation readiness. An earlier publication does not prove a new calculation is ready, nor does a ready calculation imply publication. PPE uses current calculated prices and has no fabricated publication.
- R-A05: explicit local-only `pricing-readiness` mode requires a report path, is mutually exclusive with every other seed mode, and does not fall through to normal migrations/seeding. It writes only the report file. No approval/save/publication writer is invoked.
- R-A06: report is point-in-time advisory evidence, not authorization. Actual edits use existing expected-version/idempotency/authenticated manager workflows. Re-evaluate after edits and before publishing; preserve historical versions.

## Acceptance scenarios

| ID | Observable result | Acceptance |
| --- | --- | --- |
| SC-A01 | Full imported inventory | One item per source row, exact aggregate counts, missing identities retained with named blockers. |
| SC-A02 | Drafts and missing inputs | Unapproved/unresolved inputs remain blocked with IDs/flags; matching old diagnostic values cannot make them ready. |
| SC-A03 | Independent source issues | Empty/invalid source outputs identify exact cells without inventing a price or hiding other rows. |
| SC-A04 | Ready versus published | Ready Sewing without publication requests save/publish; existing publication is recorded separately; PPE does not receive a publication. |
| SC-A05 | No business-data mutation | On an isolated fixture report execution leaves input, calculation and publication rows unchanged; nonlocal target is rejected before access. |
| SC-A06 | Controlled activation | Each approved item has reviewed evidence, expected current version and actor; post-change strict calculation and final consumer checks pass. Gated follow-up, not claimed by readiness delivery. |
