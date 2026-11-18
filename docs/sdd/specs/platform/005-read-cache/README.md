# PLT-005 — Read cache (HybridCache, in memory)

**Status:** accepted 2026-11-18, implemented the same day (owner: «добавь где реально было бы полезно … архитектурно верно … зафиксируй в доках»). **Owner:** platform (port and adapter in BuildingBlocks; the cached reads in Content and Identity). **Decision:** [ADR-0021](../../../architecture/decisions/0021-read-cache-hybridcache.md).

## Goal

The reads that are worth caching keep doing so through one port over .NET HybridCache, are dropped at once when an Admin changes their data, and show in traces whether they came from memory.

## Where a cache is useful (checked 2026-11-18)

| Read | Why | Lifetime, tag |
| --- | --- | --- |
| Public catalog snapshot (Content) | reads every Catalog product and all site pages; anonymous; every catalog request | 60 s, `content:public-site` |
| Public articles, redirects, fixed pages, SEO settings, company facts, galleries (Content) | anonymous, every page of the site; same for every visitor | 60 s, `content:public-site` |
| Saved access rights (Identity) | read on every admin request by `AccessRequirementHandler` | 30 s, `identity:role-access` |
| Reference books, pricing inputs, prices, stock, money, orders | correctness first; prices come with versions | never cached |

All eight were already cached with `IMemoryCache`; this spec moves them onto the port, adds invalidation for Content and tracing.

## Rules

- R-01 Caching only through `IReadCache`; only `BuildingBlocks.Infrastructure/Caching` uses `HybridCache`, `IMemoryCache` or `IDistributedCache`.
- R-02 A cached value is a sealed read model with `[ImmutableObject(true)]`, never tracked, never handed to a command, and never holds a domain entity or aggregate; only immutable domain values (records, enums, typed ids) may appear in it, as in responses (`ArchitectureTests/Caching/CachedReadModelTests`).
- R-03 Every cached read has a `ReadCacheEntry` (name, key, tag, lifetime).
- R-04 Every Content command implements `IInvalidatesReadCache` with `PublicSiteCache.Tag`; the behavior drops it only after success.
- R-05 Saving or deleting access rights drops `identity:role-access`.
- R-06 Invalidation reaches only the instance that ran the change; others, `Host.Seed` imports and Catalog changes show after the lifetime.
- R-07 Each read tags the request span with `cache.<name> = hit | miss`.
- R-08 L2 is off; adding it is one host registration.

## Scenarios

- SC-01 Two site requests within a minute: the second answers from memory (`cache.content.public-catalog = hit`).
- SC-02 An Admin saves an article: the next site request on that instance reads the new data.
- SC-03 A failed save (stale revision, validation): the cache stays.
- SC-04 A director changes a role's rights: the next admin request on that instance uses them; other instances within 30 seconds.
- SC-05 Many visitors hit a missing snapshot at once: one rebuild.

## File-role map

| File | Role |
| --- | --- |
| `BuildingBlocks.Application/Caching/IReadCache.cs`, `ReadCacheEntry.cs`, `IInvalidatesReadCache.cs` | port, entry, command marker |
| `BuildingBlocks.Application/Behaviors/ReadCacheInvalidationBehavior.cs` | drops tags after a successful command |
| `BuildingBlocks.Infrastructure/Caching/HybridReadCache.cs`, `ReadCacheRegistration.cs` | adapter, `AddReadCache()` |
| `Content.Application/PublicSiteCache.cs` | tag and lifetime of the public site reads |
| `Content.Application/Features/*/Shared/**/Public*Source.cs` | the seven cached reads |
| `Content.Application/Features/*/Commands/*/*Command.cs` | 21 commands that drop the tag |
| `Identity.Infrastructure/.../Access/AccessGrantSource.cs`, `AccessRightsService.cs` | cached rights and their invalidation |
| `Host.Api/DependencyInjection/Modules/MediatorRegistrationExtensions.cs` | the behavior in the pipeline |

## Tests and evidence

- Unit: [HybridReadCacheTests](../../../../../tests/UnitTests/Platform/Caching/HybridReadCacheTests.cs) (SC-01, R-07), [ReadCacheInvalidationBehaviorTests](../../../../../tests/UnitTests/Platform/Caching/ReadCacheInvalidationBehaviorTests.cs) (SC-03), Content public source tests (`UnitTests.Content`, SC-02).
- Architecture: [ReadCacheLibraryTests](../../../../../tests/ArchitectureTests/Caching/ReadCacheLibraryTests.cs) (R-01), [ContentCacheInvalidationTests](../../../../../tests/ArchitectureTests/Caching/ContentCacheInvalidationTests.cs) (R-04).
- 2026-11-18: `UnitTests` (Content, Identity, Platform.Caching: 205 passed), `ArchitectureTests` (132 passed, with `CachedReadModelTests`), `HostApiTests` Access and endpoint inventory (27 passed); the OpenAPI contract is unchanged.

## Open items

- None. The catalog and article snapshots held untracked Content entities (`ProductPage`, `SiteCategory`, `Article`) before this spec; on owner review the same day (2026-11-18) they became read models (`PublicProductPageView`, `PublicSiteCategoryView`, `PublicArticleView` in the `Snapshot/` folders), and `CachedReadModelTests` keeps entities out of every cached type.
