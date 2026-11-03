# CRM-002 — Clients from KeepinCRM

**Status:** backend implemented and verified with unit tests 2026-11-02. **Date:** 2026-11-02. **Module:** Crm, schema `crm`, no schema change.

Step 4 of moving the client's Calculator into Brevi. The admin gets «Загальні довідники → Контрагенти» with the tabs «Постачальники» ([REF-001](../../reference/001-suppliers-keepincrm/README.md)) and «Клієнти». Clients come from KeepinCRM, as they did in the Calculator, which read them straight from KeepinCRM and never kept them in the sheet. The product owner decided on 2026-11-02 that old Brevi clients not in KeepinCRM are deleted, except those that quotes use, which are hidden.

## Source

`GET https://api.keepincrm.com/v1/clients?page=N&q[lead_false]=true` with header `X-Auth-Token` (the same key as suppliers). The answer is `{items, pagination: {total_pages}}`. Fields used: `id`, `company`, `person`, `phones`, `email`, `contacts[].phones`, `contacts[].email`, `archived`.

## Rules

- **R-01:** A Brevi client is linked to KeepinCRM by the external identity `keepincrm / client / <id>`. Matching is by that ID only, never by name.
- **R-02:** KeepinCRM owns the name (`company`, or `person` for a private person), the kind (organization or person), the active flag (`archived` means not used) and the contact with position «KeepinCRM» (person, valid distinct phones and e-mails, at most 10). Legal name, tax ID and other contacts stay as set in Brevi. A phone or e-mail Brevi cannot accept is skipped.
- **R-03:** A new KeepinCRM client is created, owned by `KeepinCrm:ClientOwnerUserId` (default 1). A linked client missing from KeepinCRM is marked not used, never deleted. Repeating the same data changes nothing.
- **R-04:** A preview (`dryRun`, the API default) saves nothing. An error or an empty answer changes nothing.
- **R-05:** With `removeUnlinked` (Admin only, one-time transfer) Brevi clients without a KeepinCRM link are deleted with their contacts and identities; clients that a quote, a custom sewing draft or a merge refers to are hidden instead.
- **R-06:** The daily `brevierp-sync` job runs it as the step `keepin-clients` after `keepin-suppliers`, applied and without `removeUnlinked`.

## Scenarios

- **SC-01 / R-01–R-03:** Given a renamed linked client, a linked one missing from KeepinCRM, a manual one, a new private person and an archived company, then the first is renamed with its «KeepinCRM» contact, the missing one is not used, the manual one is untouched, and two are created (one not used).
- **SC-02 / R-03:** Repeating the same data leaves the revision unchanged.
- **SC-03 / R-04, R-05:** A transfer preview lists the unused unlinked client as deleted and the quoted one as hidden, and saves nothing; an empty KeepinCRM answer throws and saves nothing.

## Design

- Application: `Crm.Application/Customers/KeepinCrmSync/` (`KeepinCrmClientSync`, target mapping, identity, store and source ports, command).
- Infrastructure: `Crm.Infrastructure/KeepinCrm/` (options, parser, HTTP source), `Persistence/Customers/KeepinCrmSync/CustomerKeepinSyncStore.cs`, `DependencyInjection/CrmKeepinCrmExtensions.cs` (Host.Api and Host.Seed).
- API: `POST /api/crm/customers/keepin-sync` `{dryRun, removeUnlinked}`; the list summary gains `keepinCrmId`, `contactName`, `phones`. Contract [clients-quotes](../../../contracts/crm/clients-quotes.openapi.yaml) 1.1.0, additive.
- Known limit: a Manager sees only clients they own, so KeepinCRM clients are visible to Admins (and to the owner user) until owner mapping from `main_responsible` is decided.

## Tasks

- **TS-01 (done):** sync use case, endpoint, daily step. Tests: `tests/UnitTests/Crm/Customers/KeepinCrmClientSyncTests.cs`.
- **TS-02:** Admin «Контрагенти» with tabs «Постачальники» and «Клієнти» in brevi-web; «Клієнти» leaves «Менеджер сервісу».
- **TS-03:** one-time transfer: preview with `removeUnlinked`, then apply after the owner's confirmation.

## Verification 2026-11-02

`dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~Crm|FullyQualifiedName~OpenApi"`: 218 passed. `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj`: 7 passed.
