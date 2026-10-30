# P5 local rendered acceptance — 2026-10-30

The three remaining P5 acceptance packages have local evidence: [ordinary PDF](p5-standard-pdf-acceptance-2026-10-30.md), [seasonal PDF](p5-seasonal-pdf-acceptance-2026-10-30.md), and [persisted journey](p5-introduction-journey-acceptance-2026-10-30.md).

## Scope and outcomes

- Actual existing Chromium renderer, local Chrome and installed Arial: ordinary one-item PDF1 page, ordinary24-item PDF7 pages; seasonal summer/winter/general1/1/2 pages both without and with logo. Eight PDFs and16 raster pages were visually inspected across the two packages. Tests use clearly synthetic issuer/logo/public/private sentinels, not production data or a claim of current Catalog/brand approval.
- PDF text extraction verifies document number, visible public prices/metadata/Cyrillic and absence of internal notes, basis/formula/private source and hidden item/variant/tier sentinels. Seasonal48/48 assertions include exact product HTTPS links. Manifests and reproduction harnesses live under `E:/Development/p5-pdf-acceptance/standard` and `seasonal`; files and raster hashes are recorded there.
- Four isolated PostgreSQL tests verify stored selection→current check→real quote/basis stores→ready/number→queue/worker→artifact read/reopen for three layouts, plus oversized early rejection. New quote price275 replaces200 only for the new version; old saved public snapshot/projection and artifact ID/bytes/hash remain unchanged. The journey uses controlled Catalog ports/recording renderer; it is combined with the separate real Chromium proof, not represented as browser/deployed E2E.

## Defects found and fixed

1. Ordinary independent paragraphs split short item titles/variants from prices and separated CTA heading/text. `PublicDocumentTextGroup` carries explicit header/item/CTA groups; writer validates flattened group paragraphs exactly equal the allowlisted projection and uses keep-together pagination. No string-label heuristics or new private fields.
2. Large logo created an extra header-only seasonal page. Seasonal-only header/logo/card spacing now keeps all six representative PDFs at1/1/2. Ordinary layout is not compacted.
3. Valid generic template text could overflow the fixed seasonal layout or fail only in worker. `IntroductionDocumentLayoutPolicy` applies conservative wrapping budgets for title, intro/footer and cards before ready; no truncation. Reproduced1200 opening and600+600 intro/CTA now reject with `introduction_public_layout_too_large`. Prepare maps this to Invalid before number/issue. Ordinary documents retain multipage text capacity. Frontend explains how to shorten the content.

## Verification and boundary

Final scoped backend unit/contract/projection tests30/30; isolated PostgreSQL journey4/4, zero skipped; frontend document hook3/3 including actionable layout blocker. Build/type/diff checks accompany coordinator completion. Existing NU1903 dependency advisory is outside this slice.

P5 local technical acceptance is complete in the agreed scope. New user-created selection templates remain blocked until P6 by the explicit user decision; saved references are preserved. Email/outbox/send and production package are P6. No live email, production deployment, production database operation or Git commit/push was performed by acceptance. This evidence does not replace operator/business approval of live legal/branding/source data or a deployed browser E2E.
