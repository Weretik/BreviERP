# Ordinary P5 PDF acceptance — 2026-10-30

**Result:** real Chromium rendering, extracted customer fields/privacy and visual ordinary one/multipage acceptance passed after the page-group fix. This is a deterministic synthetic fixture proof through the real production projection and renderer, not a production seller-data approval or deployed browser journey.

## Reproducible local fixture

Harness: `E:/Development/p5-pdf-acceptance/standard/runner/StandardProof.csproj` and `Program.cs`. It references existing `Crm.Infrastructure`, constructs `QuoteVersion`, `IssuerSettingsVersion`, `DocumentTemplateVersion`, calls `QuotePublicDocumentProjectionBuilder.Build`, then `ChromiumPrivateDocumentRenderer.RenderAsync`. It does not use a duplicate HTML writer or pricing formula and does not access any database.

Runtime: `C:/Program Files/Google/Chrome/Application/chrome.exe`; font `C:/Windows/Fonts/arial.ttf`. All sample values and logo are explicitly synthetic. The logo is a local PNG fixture with BREVI text, not a certification of the provisioned production brand asset. Issuer text is marked “тестовий документ”. The main Arial text font is embedded. Cyrillic includes Ї/Є/І/Ґ, literal HTML-like public text, punctuation, long descriptions and 24 repeated complete quote positions for the multipage case.

The PDF skill operation marker ran once before generation for two PDF outputs. Command:

```powershell
dotnet run --project E:/Development/p5-pdf-acceptance/standard/runner/StandardProof.csproj --no-restore -- E:/Development/p5-pdf-acceptance/standard
```

## Discovered and fixed pagination defect

The first multipage proof flattened each item/variant/price and CTA into unrelated paragraphs. At a page transition, “Варіант 2” ended page 1 and its price started page 2; CTA heading ended page 6 while CTA text/manager alone occupied page 7. This was a confirmed contextual pagination defect, despite complete text and no clipping. Machine results are retained in `checks-before-fix.json`, and the original visual contact sheet in `ordinary-multi-before-fix.png`.

The coordinator fixed the production projection with typed public text groups for issuer/header, each ordinary quote item, and CTA. The writer validates flattening equivalence and keeps each short group together; it does not infer structure from customer text. After that fix the same harness was rebuilt and regenerated. The later seasonal CSS change is conditional on `document.Pages != null`; the coordinator confirmed ordinary rendering is unchanged.

## Final artifacts and checks

All files below are under `E:/Development/p5-pdf-acceptance/standard/`.

| Artifact | Pages / positions | SHA-256 |
| --- | --- | --- |
| `ordinary-one.pdf` | 1 / 1 | `E2614C341F63667FE45667D0BBFAD8CD25CC24B7C0F132BEDBE7399897DCA40F` |
| `ordinary-multi.pdf` | 7 / 24 | `E2B35D3780ABF6CF1E931600F400EA9D13084238AE749A37C6B3BB4A4776F9B9` |

`manifest.json` records output bytes/hash; `audit_pdf.py` extracts actual PDF text using pypdf and rasterizes every page with bundled Poppler `pdftoppm`; `checks.json` stores the final counts. The actual PDF output, rather than only source projection text, contains the fixed number `10/10/2026`, visible `1234.56 грн`, all requested position titles and manager Олена. It excludes internal notes, calculation-basis sentinels, hidden variants/items, and hidden tier prices `987.65`, `876.54`, `999.99`.

Both final artifacts have `short_item_groups_intact=true` and `cta_group_intact=true`: each page contains equal item-title and retail-price counts and the complete CTA is on one page. The final multipage last page contains positions 22–24 plus the whole CTA, not an orphan footer. These assertions are specific to the short complete-item fixture; oversized customer text may legitimately span pages.

Visual review inspected `ordinary-one-1.png` and the seven-page `ordinary-multi-contact-sheet.png`; all 8 rendered pages are present. Logo and title are legible, Cyrillic is displayed correctly, literal script markup appears as text, text/price groups stay together, and no overlap or clipping was observed. Independent pdfplumber character-bounds checks found zero characters outside the printable margin on all 8 pages (`bounds.json`). Full-page PNGs and extracted `*.text.txt` are retained.

Poppler reported nonfatal nameToUnicode path warnings for its Bulgarian/Greek/Thai resource filenames under the Unicode Windows profile, but returned success and produced all page images. pdfplumber reported missing FontBBox on an auxiliary font; the main Arial font is embedded and the visual/text/bounds checks passed. Neither warning was suppressed or interpreted as proof of layout correctness.

## Acceptance boundary

No production records, quotes, email, seed or migrations were touched. Fixture number/tax/seller values demonstrate rendering and pinning, not approval of changed business data. Actual saved-template → ready → artifact replay and historical hash behavior are covered by the coordinator's separate PostgreSQL journey evidence. No commit or push was made by this package.
