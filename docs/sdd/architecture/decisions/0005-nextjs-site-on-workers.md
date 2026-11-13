# ADR-0005: Public site on Next.js + Cloudflare Workers, reading the ERP API

**Status:** accepted · **Date:** 2026-11-13 (recorded from the current code)

## Context

The old public site ran on OpenCart, separate from the ERP. Products, prices and texts were maintained twice. It also had SEO defects: no `hreflang`, no structured data, and wrong city names in titles. An earlier Angular rewrite was never finished. The site must be fast, indexed in Ukrainian and Russian, and edited by staff in the admin.

## Decision

- Build the site with Next.js 16 App Router (Server Components), Tailwind v4 and shadcn/ui, in the same Nx monorepo as the admin.
- Deploy it to Cloudflare Workers through OpenNext.
- The site reads a small anonymous public API from the ERP (Content module): catalogue, product pages, articles, company info, galleries and redirects. It never writes, except through the request form.
- Cache API responses in Workers KV with stale-while-revalidate: fresh for 5 minutes, served for up to a day if the API is down.
- SEO is part of every page: localized metadata, `hreflang`, JSON-LD, sitemaps, and 301 redirects from old OpenCart URLs.

## Why

- One source of truth: staff edit products and texts once, in the admin.
- Server rendering at the edge gives fast pages and full HTML for search engines.
- React is shared with the admin, and Kedr's site uses the same stack, so knowledge carries over.

## Trade-offs

- The site depends on the ERP API. The KV cache with long expiry softens API outages.
- Changes appear after up to 5 minutes, not instantly.
- OpenNext on Workers has platform limits (bundle size, Node APIs) that need care when adding libraries.
