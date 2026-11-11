# CNT-010 — Main texts and frequent questions of the fixed site pages (Content module)

**Status:** accepted by the owner 2026-11-11 (stage 6 of the site SEO plan: page texts and FAQ in the Admin); stage 1 (this repository) implemented 2026-11-11. **Date:** 2026-11-11. **Module:** `Content`, schema `content`. Extends [CNT-007](../007-site-pages/README.md).

## Why

The SEO audit of 2026-11-11 asks for a unique text of 300–600 words and 3–5 frequent questions on every main section, in Ukrainian and Russian, edited in the Admin. Today the home text and FAQ and the industries FAQ live in the site code; other pages have none.

## Decision

The same row of `content.SitePages` that holds the search texts also holds, per language, the main text (blocks of the visual editor, the same shape as category texts) and the list of frequent questions. «Сторінки сайту» edits them in the same side panel. The site shows the text and the questions at the bottom of the page and gives the questions to search engines as `FAQPage`; an empty text or list keeps the site's own (or nothing on a page that had none).

## Rules

- R-01: Per page and language: text ≤ 200 blocks and ≤ 20000 characters (paragraph, heading, list; bold and italic inside); questions ≤ 12, question ≤ 200 and answer ≤ 1500 characters, each one line of plain text.
- R-02: Blocks and questions are trimmed; an empty block, a question without an answer and an answer without a question are dropped.
- R-03: Save replaces all texts of the page in both languages with the revision check of CNT-007 R-03; a save without text or questions clears them. The public endpoint returns them with the search texts, always as lists.
- R-04: The migration `AddSitePageTextAndFaq` fills the home text and questions and the industries questions with what the site shows today, so staff edit instead of typing again.

## Scenarios

- SC-01: Staff open a page in «Сторінки сайту», change the main text in the visual editor and the questions, save; too long or too many is refused with a message.
- SC-02: Within minutes the site shows the new text and questions at the bottom of that page, and its `FAQPage` lists the same questions.

## Design

The [admin contract](../../../contracts/content/content-site-pages.openapi.yaml) adds `text` and `faq` to `SitePageText` and the schema `SiteFaqItem`; the [public contract](../../../contracts/content/content-public-site-pages.openapi.yaml) returns them. Block shape: `ProductTextBlock` of the [products contract](../../../contracts/content/content-products.openapi.yaml).

| Path | Responsibility | State |
| --- | --- | --- |
| `Content.Domain/SitePages/SitePage.cs` | texts, questions, limits, `Replace` of both languages | changed |
| `Content.Domain/SitePages/SiteFaqItem.cs` | question with answer and its cleaning; `SitePageTextInput` | new |
| `Content.Application/Contracts/SitePageResponses.cs` | `SitePageText` with `text` and `faq` | changed |
| `Content.Application/SitePages/Save`, `Shared` | validator per language, messages, mapping | changed |
| `Content.Application/PublicSitePages` | public read with text and questions | changed |
| `Content.Infrastructure/.../SitePageConfiguration.cs`, migration `AddSitePageTextAndFaq` | four jsonb columns, seed of the current texts | changed / new |

## Stages (one PR each)

1. Columns, admin and public API (this repository).
2. Admin side panel with the visual editor and the questions; the site shows them (brevi-web).

## Tests

`tests/UnitTests/Content/SitePagesTests.cs`, `SitePagesContractTests.cs`.
