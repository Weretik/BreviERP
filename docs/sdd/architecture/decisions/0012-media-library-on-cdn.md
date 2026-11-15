# ADR-0012: Photos live on the CDN; the Media module keeps a library of them by id

**Status:** accepted · **Date:** 2026-11-15 (recorded from the code; [MED-001](../../specs/media/001-media-library/README.md)) · **Owner:** Виталий (project owner)

## Context

Product photos, article pictures, staff avatars and photos from the old site must load fast on the site and in КП PDFs. The CDN `brevi-media.cdn.express` (adm.tools storage) is already paid for. Photos used to be rows inside Catalog, so other modules could not reuse them and nobody knew which files were in use.

## Options

- Store photos in PostgreSQL or Cloud Storage: rejected; the CDN exists and is cheaper for traffic.
- Keep photos inside Catalog: rejected; Content, Crm and Identity need them too.
- A separate Media module with a library by id: chosen.

## Decision

- The Media module owns folders and photo records in schema `media`; originals go to the CDN under `/library/original` through the adm.tools adapter in Infrastructure, previews are made by a background task.
- Other modules store only a photo id and read URLs through `Media.Contracts`; Catalog answers which products use a photo through `Catalog.Contracts`.

## Consequences

- One inventory of photos with stable URLs; modules do not know the CDN.
- Without CDN credentials uploads answer 503.

## Sources

[Photo library](../../features/media/library.md), [configuration](../../operations/configuration/configuration-and-secrets.md).
