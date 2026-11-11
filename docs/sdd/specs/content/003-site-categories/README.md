# CNT-003 — «Контент → Розділи»: categories of the public site (Content module)

**Status:** draft, waits for the owner's choice of the tree source (asked 2026-11-11). **Date:** 2026-11-11. **Module:** `Content`, schema `content`. Continues [CNT-002](../002-product-pages/README.md).

## Why

The owner asked on 2026-11-11 for an Admin page where staff choose which categories and subcategories the site shows and edit their SEO, the same way as for products. The old `brevi.com.ua` has 13 categories and 52 subcategories; 58 of them have an SEO title, 24 an SEO description, and almost all a text in Ukrainian and Russian. «Типи і розділи» of the Admin has 16 flat sections with internal names («Одяг ЛІТО КМФ») and no subcategories, so the old SEO cannot be attached to it.

## Decision to confirm

The site gets its own category tree, owned by Content, with two levels as on the old site. This replaces the CNT-002 decision «the tree is built from Типи і розділи» (R-09). The alternative (keep the 16 sections and add a mark, SEO and text to each) loses the subcategories and most of the old SEO.

## Rules

- R-01: A site category has a parent or is a root; the tree has two levels. Fields: `ShowOnSite`, `Slug` (unique, same rules as a product page), `SortOrder`, and per language (`uk`, `ru`) `Name`, `SeoTitle` (≤ 200), `SeoDescription` (≤ 320), `Text` (blocks as in a product description).
- R-02: The site shows a category when it and its parent have `ShowOnSite = true`. A category without products on the site is still shown when marked (staff decide).
- R-03: A product page belongs to any number of site categories (the old site lists a product in several). A listing of a category shows the products of the category and of its subcategories, in the order of the category.
- R-04: Save replaces a category as a whole with a revision check; a taken slug is a 409. Deleting a category with subcategories is refused; deleting a category removes only its links to products.
- R-05: Access: page key `content-categories`, group «Контент», actions View, Create, Edit, Delete; defaults Director and Admin all, Manager view.
- R-06: Public API: the tree returns the shown categories with names in both languages; `products` takes `category` (slug); a new `GET /api/public/catalog/categories/{slug}?language=` returns the SEO and the text. `type` and `sectionId` of CNT-002 are removed from the public contract together with the site switch.
- R-07: Long texts are edited in the visual editor of the Admin; nothing is typed as markup.

## Scenarios

- SC-01: Staff open «Контент → Розділи», see the tree with the «на сайті» mark, the number of products, and what is missing (no Russian name, no SEO).
- SC-02: Staff edit a category in the side panel: mark, address, parent, order, names, SEO, text in both languages.
- SC-03: Staff add, move, and delete a category.
- SC-04: In «Товари сайту» staff choose the categories of a product.
- SC-05: A one-time transfer brings the 65 categories of the old site with SEO, texts, and the products of each (matched as in CNT-002 SC-05): preview first, then apply.
- SC-06: The site shows the menu, the category pages with SEO and text, and the breadcrumbs from this tree.

## Stages (one PR each)

1. Domain, schema, admin API of categories.
2. Admin page «Контент → Розділи» (brevi-web).
3. Categories of a product in «Товари сайту» and the old-site transfer.
4. Public API by category.
5. Site reads the new tree and category SEO (brevi-web).
