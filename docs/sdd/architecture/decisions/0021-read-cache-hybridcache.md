# ADR-0021: Cached reads go through one port over HybridCache, dropped by tag after a command

- Status: accepted
- Date: 2026-11-18 (owner's request: «добавь кэш, только архитектурно верно и там, где реально полезно»)
- Owner: Виталий (project owner)

## Context

Before this decision eight reads already kept their answer in `IMemoryCache`, each in its own way: the seven public site sources of Content (catalog snapshot, articles, redirects, fixed pages, SEO settings, company facts, galleries; 60 seconds) and the saved access rights of Identity (read on every admin request; 30 seconds). Each source had its own key, lifetime and `TryGetValue`/`Set` code; only the access rights were dropped on save, so an Admin change in Content reached the site only after the minute; many callers of a missing key rebuilt it at the same time; nothing showed in traces whether a read came from memory.

The API runs on Cloud Run, which may start several instances; `Host.Seed` jobs run in another process. Redis (Memorystore) would cost money every month for a small load.

## Alternatives considered

- Keep `IMemoryCache` in each source: rejected, for the reasons above.
- Redis as a shared cache now: rejected for cost and an extra outage point; kept possible as an L2.
- Output caching of HTTP responses: rejected; it lives in the API layer, caches per URL, and commands in Application could not drop it without HTTP concerns.

## Decision

- One port `IReadCache` in `BuildingBlocks.Application/Caching` (`GetOrCreateAsync(ReadCacheEntry, read)`, `InvalidateAsync(tag)`). `ReadCacheEntry` (`readonly record struct`, built by `Create`) carries a stable name, the key, one tag and the lifetime.
- One adapter `HybridReadCache` in `BuildingBlocks.Infrastructure/Caching` over .NET `HybridCache` (`Microsoft.Extensions.Caching.Hybrid`): one read per missing key (stampede protection), tags, and the tag `cache.<name> = hit | miss` on the request span in Cloud Trace. `AddReadCache()` registers it; Content and Identity Infrastructure call it (it is safe to call twice).
- **L1 only.** No `IDistributedCache` is registered. Adding an L2 later is one host registration (for example `AddStackExchangeRedisCache`); no query or command changes.
- **Queries cache, commands drop.** A cached value is a sealed, immutable read snapshot marked `[ImmutableObject(true)]`, so HybridCache hands out the same instance; it holds read models only, never a domain entity (CQRS). A command whose data a cached read shows implements `IInvalidatesReadCache`; `ReadCacheInvalidationBehavior` drops its tags after success. A write outside Mediator (`AccessRightsService`) calls `InvalidateAsync`.
- **Content:** all seven public sources share the tag `content:public-site` and 60 seconds (`PublicSiteCache`); every Content command drops that tag (an architecture test checks each one).
- **Identity:** saved access rights, tag `identity:role-access`, 30 seconds, dropped by a save or a role delete.
- Only `BuildingBlocks.Infrastructure/Caching` may use `HybridCache`, `IMemoryCache` or `IDistributedCache` (`ReadCacheLibraryTests`).

## Consequences

- Invalidation is local: the instance that ran the command drops the tag at once; other instances, `Host.Seed` imports, and Catalog changes behind the public catalog show after the lifetime. Lifetimes stay short; money, stock, orders and pricing inputs are never cached.
- A cached value never holds a domain entity: the Content catalog and article snapshots carry read models (`PublicProductPageView`, `PublicSiteCategoryView`, `PublicArticleView`), so an L2 can serialize them; `CachedReadModelTests` fails when an entity or aggregate is reachable from any `[ImmutableObject(true)]` type.
- A new cached read records in its spec why it is worth caching, its lifetime, its tag and the commands that drop it.

## Sources and checks

- `src/BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application/Caching/`, `.../Behaviors/ReadCacheInvalidationBehavior.cs`, `src/BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure/Caching/`.
- Tests: `tests/UnitTests/Platform/Caching/`, `tests/ArchitectureTests/Caching/`, the Content public source tests.
