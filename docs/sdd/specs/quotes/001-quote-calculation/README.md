# QTE-001 — «Розрахунок КП»: quote calculation module (plan)

**Status:** draft, planned only; not implemented. **Date:** 2026-11-03. **Module:** new module `Quotes`, own schema `quotes`, own migrations history table (see [module rules](../../../standards/module-rules.md)).

This document keeps the idea for a later AI or developer. Decided by the product owner on 2026-11-03 while moving «Товари» from the Calculator sheet ([CAT-006](../../catalog/006-products-page/README.md)).

## Idea

A commercial proposal (КП) is calculated from Brevi's own references and products, not from the Google Sheet or the old quote cabinet. The calculation is a separate module with its own schema; it only reads other modules.

## Inputs (read, never written)

- Catalog: products of both tables, code, name, section, season, active, checked, the short description for a quote (`ProductCatalogProfiles.ShortDescription`), current prices (sewing publications, PPE prices), and the temporary link to the current site page for photos (`ProductCatalogProfiles.WebsiteUrl`).
- Reference: fabrics, accessories, operations, pricing parameters, PPE markup rules, product sections.
- Crm: clients (from KeepinCRM).
- Later, Content: photos from our CDN instead of the current site links.

## Current state to reuse or replace

- The old flow: the sheet script «Синхронизаций калькулятора» sends the active catalogue to an external quote cabinet. It is not moved; the module reads Brevi directly ([script analysis](https://claude.ai/artifact/FEvYFJ2pCgcZUkJVSQhBd4), 2026-11-03).
- Crm already has versioned quotes, quote items and tiers, templates, issuer settings, PDF and send history, built during the earlier calculator port ([clients and quotes](../../../features/crm/clients-and-quotes.md)). The specification decides which parts move into `Quotes` and which stay in Crm; do not build a second copy.

## Open questions for the specification

- Which quote variants the business needs (sewing to order, PPE resale, mixed) and their price tiers (1–9, 10–40, from 41 pcs).
- Discounts and rounding rules, VAT display.
- PDF layout and who sends it (Crm outbox?).

## What to implement (when the owner starts it)

1. `$backend-feature-spec`: aggregates (quote, version, line), calculation rules from the Calculator sheet, API, contract.
2. Read ports to Catalog, Reference and Crm; no cross-module foreign keys.
3. Admin pages in brevi-web («Розрахунок КП»).
4. Migration plan for existing Crm quotes.
