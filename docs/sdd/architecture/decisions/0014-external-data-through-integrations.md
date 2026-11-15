# ADR-0014: Data from outside systems comes in through scheduled imports behind module ports

**Status:** accepted · **Date:** 2026-11-15 (recorded from the code) · **Owner:** Виталий (project owner)

## Context

Brevi still depends on data kept elsewhere: suppliers and clients in KeepinCRM, purchase invoices and leads in Google Sheets filled by people and a Telegram bot, supplier price lists on supplier sites, staff mail in Gmail, invoice recognition by OpenAI. These systems are slow, sometimes down, and change formats. The business owner's rule: only the client's current Google Sheet data is valid, and the existing Telegram bot and group are reused, never a new bot.

## Options

- Read the outside system live on every page: rejected; pages would fail when the system is down.
- A separate integration service: rejected; too much for one developer.
- Each owning module imports into its own tables on a schedule, through a port in Application and an adapter in `Infrastructure/Integrations/<System>/`: chosen.

## Decision

- The module that owns the data owns the import, for example: Crm (KeepinCRM clients, leads sheet), Reference (KeepinCRM suppliers), Purchasing (sheet «финансы»), Catalog (supplier price lists), Media (previews).
- Imports run as background tasks ([ADR-0010](0010-scheduler-tick-for-background-tasks.md)); a failure is recorded with its reason and never blocks the API.
- Google Sheets are read with the runtime service account (the sheet is shared with it), not with a stored key.
- AI is called only through `Ai.Contracts` (`IStructuredDataExtractor`); the OpenAI key is stored encrypted with ASP.NET Data Protection, with a monthly limit set in the admin.
- Telegram goes through the existing bot and group; Gmail is send-only (`gmail.send`) with an encrypted refresh token per staff member.

## Consequences

- Pages read local tables and keep working when an outside system is down; data is as fresh as the last import.
- Each outside format change touches one adapter.

## Sources

[Sync job runbook](../../operations/jobs/sync-job.md), [Gmail for КП](../../operations/configuration/crm-gmail.md), [module map](../overview/modules.md).
