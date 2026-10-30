# P5 PDF functional and responsibility audit — 2026-10-30

Scope: only the recently added ordinary paragraph grouping, seasonal header/capacity fixes, ready rejection, and frontend blocker presentation. Backend-code-audit and frontend-code-audit boundaries were applied locally. No API/schema, database, pricing, mail or generated-client changes were required.

## Finding and refactor

**AF-P5-PDF-004 — Low / verified refactor.** `Crm.Infrastructure/Documents/Rendering/PublicDocumentHtmlWriter.cs` mixed projection validation, image/content policy, print CSS and seasonal/ordinary markup. Owner backend; TS-010/TS-016. This made layout changes difficult to review independently from privacy constraints. Split within the existing rendering folder (no new business module):

- `PublicDocumentHtmlWriter.cs`: document framing, ordinary text-group composition, HTML escaping and inline-image markup.
- `PublicDocumentProjectionValidator.cs`: bounded input, group equality to allowlisted paragraphs, seasonal page/card/URL constraints and inline-image validation before writing.
- `PublicDocumentStyles.cs`: embedded-font print CSS and seasonal-only sizing overrides; CSS rule bytes preserved.
- `IntroductionDocumentHtmlWriter.cs`: escaped four-card seasonal page markup.

`IntroductionDocumentLayoutPolicy` remains an Application class that rejects unsupported fixed-layout content before ready/number issuance. `PublicDocumentTextGroup` remains a public-projection value, not a quote store or transport DTO. Chromium process lifecycle/private transient files remain in their existing adapter. The frontend blocker message is small and cohesive inside the document-action hook; extra modules are not warranted.

## Functional review

- Paragraph groups must flatten exactly to the approved public paragraph list; hidden notes/price bases cannot be introduced by grouping. The permitted102 groups cover100 item groups plus header and CTA.
- Seasonal URL/text/image limits and HTML escaping remain enforced. Styles and image bytes are embedded, CSP excludes scripts/network fetches, and the renderer keeps its existing bounded process/private-workspace rules.
- Ordinary quote grouping preserves the full item/CTA content. Large ordinary groups may span pages when physically larger than a page; no content is clipped.
- Fixed seasonal capacity is a conservative wrapping policy, not an arbitrary-text auto-fit guarantee. Both reproduced oversized inputs still fail before ready, number and issue. No truncation or tiny-font fallback was added.
- Generic template limits are unchanged; the compact seasonal document has its explicit stricter readiness policy. Frontend displays an actionable shortening instruction. Existing saved prices/artifacts and P6 deferral are unchanged.

No new functional/blocking defect was confirmed in this scope. The earlier three acceptance defects remain fixed. Live legal/branding approval, production deployment and the user-approved P6 new-selection deferral remain outside this audit.

## Fresh evidence

- Host.Api build passed before final extraction polish; final real harness builds and30/30 focused unit/contract/projection tests passed after it.
- Existing real renderer harnesses rerendered8 primary PDFs: ordinary1/7 pages and seasonal1/1/2 with and without logo. Ordinary item/CTA/privacy/text checks and all48 seasonal assertions passed.
- All16 freshly rasterized pages have exactly the same PNG SHA256 values as the visually accepted pages before refactoring: `E:/Development/p5-pdf-acceptance/audit-before-refactor.json` and comparison output `{pages:16,unchanged:16,changed:[]}`. PDF binary hashes can change on rerender because of metadata; this audit compared raster content, while artifact immutability is independently covered by the stored-journey tests.
- 1200-character opening and600+600 opening/CTA both still reject `introduction_public_layout_too_large`.
- Initial ordinary harness call omitted its output argument and failed before generation; corrected reproduction passed. Poppler produced its previously documented nonfatal Unicode-path lookup notices; raster generation and comparison succeeded.
- Existing NU1903 package advisory was reported by unit-test metadata; dependency changes were outside this slice. No new PostgreSQL run was needed because neither persistence nor ready/application code changed during this refactor.

Prior acceptance: [local P5 evidence](../evidence/p5-local-acceptance-2026-10-30.md). No commit or push performed.
