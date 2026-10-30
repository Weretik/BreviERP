# P5 seasonal PDF acceptance — 2026-10-30

## Scope and source

Actual PDF files were rendered through the production `IntroductionPublicDocumentProjectionBuilder.Freeze/Build` and `ChromiumPrivateDocumentRenderer`, using local Chrome (`C:/Program Files/Google/Chrome/Application/chrome.exe`) and local Arial TTF. No browser-side PDF substitute, mail send, database write or pricing formula copy was used. The fixture supplies synthetic selections and a pinned current-price snapshot, Ukrainian public metadata and private price-source/formula sentinels. Product names intentionally test text layout; they are not a claim that these synthetic IDs/names are approved Catalog seasonal combinations.

The separate acceptance harness is at `E:/Development/p5-pdf-acceptance/seasonal/runner/SeasonalProof.csproj`. It references the real CRM project, not copied renderer source. Python `verify.py` checks produced PDFs with pypdf; `render-pages.py` uses bundled Poppler to rasterize the PDFs at 90 dpi. The PDF skill artifact-start marker ran successfully before the first PDF generation.

## Final artifact checks

All final files are under `E:/Development/p5-pdf-acceptance/seasonal/`:

| File | Actual pages | Product cards / links | Result |
| --- | --- | --- | --- |
| summer.pdf | 1 | 4 / 4 | passed |
| winter.pdf | 1 | 4 / 4 | passed |
| general.pdf | 2 | 8 / 8 | passed |
| summer-logo.pdf | 1 | 4 / 4 | passed |
| winter-logo.pdf | 1 | 4 / 4 | passed |
| general-logo.pdf | 2 | 8 / 8 | passed |

For each file, eight assertions passed: expected page count, exact four/eight `Великий гурт` entries, document number `10/10/2026`, fabric/composition/density, Ukrainian Ґ/Є/І/Ї glyph text, exact Brevi HTTPS product links, absence of `PRIVATE_FORMULA_SENTINEL`/`PRIVATE_SOURCE_SENTINEL`, and all offered prices. Total: **48/48 assertions across six actual PDFs**. Final file SHA-256 values and all link URIs are recorded in `verification.json`; initial render bytes are also recorded in `render-results.json`. Extracted text is saved beside each PDF.

All **eight page PNGs** were visually inspected: long model names and composition wrap inside their cards, four cards fit per season, no text overlap/clipping was observed, footer and price lines are legible, the summer/winter boundary starts on the correct page, and the optional square test logo does not create a header-only page after the fix. The logo is a synthetic geometric test asset, not acceptance of the production brand asset provisioning. Final rerasterization produced exactly the same eight PNG hashes as the visually inspected pages (`visual-rerender-check.json`).

## Confirmed defects and final regression

1. Before the shared layout fix, the allowed square logo used 38 mm height and moved the whole first seasonal section after the title onto a new page: general output had **three pages**, with a nearly empty logo/title-only first page. The root coordinator fixed the seasonal-only header/card CSS; the six final artifacts above verify the correction. The original failing PDF/first-page image and check output remain under `before-layout-fix/`.
2. The initial renderer paragraph limit differed from allowed document-template text length. A valid 1200-character opening could fail after public projection creation. The final application layout policy now rejects this before ready/render: `InvalidOperationException: introduction_public_layout_too_large`.
3. With the compact logo CSS, a legal 600-character opening plus 600-character CTA still produced **three pages** before the capacity policy. `before-layout-fix/general-medium.pdf` preserves that reproduction. The coordinator added a conservative public layout budget. Final harness checks confirm that both the 1200 opening and the 600+600 combined case are rejected by Build with `introduction_public_layout_too_large`, without silent truncation. The original representative long-name/composition fixture was not shortened and remains accepted. Final outcomes are recorded in `stress-results.json`.

The shared policy estimates title, intro/footer and weighted card line budgets before issuance. This is a fail-closed bound, not a promise to fit arbitrary 500-character fields into the fixed seasonal layout. Unusually long public content needs correction before ready. Root owns the policy unit and PostgreSQL readiness regression evidence.

## Reproduction

```powershell
dotnet run --project E:/Development/p5-pdf-acceptance/seasonal/runner/SeasonalProof.csproj
dotnet run --project E:/Development/p5-pdf-acceptance/seasonal/runner/SeasonalProof.csproj -- --stress
# bundled Python:
python E:/Development/p5-pdf-acceptance/seasonal/verify.py
python E:/Development/p5-pdf-acceptance/seasonal/render-pages.py
```

All final commands completed successfully. Poppler emitted nonfatal `nameToUnicode` lookup notices for Bulgarian/Greek/Thai due to the Unicode installation path; it returned exit 0 and produced all expected raster pages. Notices are captured in `poppler-stderr.log`. Cyrillic text extraction and visual glyph review passed independently.

This proves actual seasonal PDF layout/public content for the representative accepted fixtures and early blocking of the reproduced oversized cases. It does not replace the coordinator's saved-selection/ready/artifact/reopen journey, verify live Catalog data, or authorize P6 email/new-selection creation. No commit or push was performed.

## Final long-word regression

After the coordinator added overflow-wrap:anywhere for document titles and introduction card headings, all six primary PDFs were rendered and verified again: 48/48 assertions passed; all eight freshly rasterized PNG hashes remained identical to the visually reviewed pages. Additional summer-word.pdf contains one unbroken 100-character Cyrillic model name accepted by the capacity policy. It stays on one page, wraps inside its card without clipping/overlap, and all 100 glyphs are present. pdfplumber bounding checks found x=66.25..282.27 pt, inside the first card; word-verification.json records this check. summer-word-1.png was visually inspected. The long-word case is a seventh artifact beyond the six primary layout cases.
