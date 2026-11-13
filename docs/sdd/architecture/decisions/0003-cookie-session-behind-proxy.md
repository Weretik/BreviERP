# ADR-0003: Cookie session + CSRF behind a Cloudflare proxy

**Status:** accepted · **Date:** 2026-11-13 (recorded from the current code)

## Context

An early version briefly allowed anonymous access to the API while the admin was being moved over. The admin is a browser SPA on another subdomain (`office.brevi.com.ua`). The API (`api.brevi.com.ua`) runs on Cloud Run. Staff work from office and home networks. Access must be per page and per action, and the owner can create custom roles.

## Decision

- Use ASP.NET Core Identity with an HttpOnly, `Secure`, `__Host-` prefixed session cookie (sliding lifetime, security-stamp check every 5 minutes). Do not keep tokens in browser storage.
- Use double-submit CSRF protection: a readable CSRF cookie on the parent domain plus an `X-CSRF-Token` header on every unsafe request.
- Use a fallback authorization policy that requires a signed-in user. Only the endpoints that must be public allow anonymous access: public site API, login, Telegram webhook, and Gmail OAuth callback.
- Access rights come from one `AccessCatalog`. It defines pages and actions (view/create/edit/delete), and each page has its own `access:<page>` policy. Built-in and custom roles get defaults that the owner can change in the admin.
- Login is rate-limited. Five wrong passwords lock the account for 15 minutes.
- A Cloudflare Worker serves `api.brevi.com.ua` and forwards the real client IP with a shared secret. The API trusts that IP only when the secret matches.

## Why

- An HttpOnly cookie cannot be read by injected scripts, which is safer than a bearer token in `localStorage` for a browser-only admin.
- One catalogue of pages and rights keeps the UI and the API in sync. The admin hides what the API would deny anyway.
- Cloudflare in front adds TLS, caching for public endpoints, and a stable domain.

## Trade-offs

- Cookies plus CORS plus CSRF need careful configuration: an explicit list of allowed origins, and a CSRF cookie domain set per environment.
- The Cloud Run origin URL still accepts direct requests. They skip Cloudflare but not authentication. Planned fix: reject requests without the proxy secret, or limit ingress.
- A future mobile client would need a token flow added.
