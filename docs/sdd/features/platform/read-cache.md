# Read cache (HybridCache)

**Static code check:** 2026-11-18. **Owner:** platform ([`BuildingBlocks.Application/Caching`](../../../../src/BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application/Caching/), [`BuildingBlocks.Infrastructure/Caching`](../../../../src/BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure/Caching/)).

Reads that every visitor or every admin request needs are kept in the memory of the API instance for a short time: the seven public site reads of Content (60 seconds) and the saved access rights (30 seconds). A change in the Admin drops them at once on the instance that saved it; other instances see it after the lifetime. No Redis: a shared L2 can be added later by registering an `IDistributedCache`.

## How it works

1. **Registration.** [`AddReadCache()`](../../../../src/BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure/Caching/ReadCacheRegistration.cs) registers .NET `HybridCache` and `IReadCache` → [`HybridReadCache`](../../../../src/BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure/Caching/HybridReadCache.cs); Content and Identity Infrastructure call it. [`MediatorRegistrationExtensions`](../../../../src/Bootstrapper/Host.Api/DependencyInjection/Modules/MediatorRegistrationExtensions.cs) adds [`ReadCacheInvalidationBehavior`](../../../../src/BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application/Behaviors/ReadCacheInvalidationBehavior.cs).
2. **Read.** A source asks `IReadCache.GetOrCreateAsync(entry, read)`; [`ReadCacheEntry`](../../../../src/BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application/Caching/ReadCacheEntry.cs) gives name, key, tag, lifetime. On a miss the read runs once and the immutable read model is kept; it never holds a domain entity (`ArchitectureTests/Caching/CachedReadModelTests`).
3. **Invalidation.** A command implementing [`IInvalidatesReadCache`](../../../../src/BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application/Caching/IInvalidatesReadCache.cs) lists tags; after success the behavior drops them. `AccessRightsService` drops its tag itself.
4. **Tracing.** Each read sets `cache.<name>` = `hit` or `miss` on the request span.

## Cached reads

| Read | Entry | Lifetime | Dropped by |
| --- | --- | --- | --- |
| Public catalog, articles, redirects, pages, SEO settings, company, galleries ([`PublicSiteCache`](../../../../src/Modules/Content/Content.Application/Content.Application/PublicSiteCache.cs)) | `content.public-*`, tag `content:public-site` | 60 s | every Content command |
| Saved access rights ([`AccessGrantSource`](../../../../src/Modules/Identity/Identity.Infrastructure/Identity.Infrastructure/Integrations/AspNetIdentity/Access/AccessGrantSource.cs)) | `identity.role-access`, tag `identity:role-access` | 30 s | rights save, role delete |

## Code structure

```text
src/BuildingBlocks/
├── BuildingBlocks.Application/BuildingBlocks.Application/
│   ├── Caching/{IReadCache, ReadCacheEntry, IInvalidatesReadCache}.cs
│   └── Behaviors/ReadCacheInvalidationBehavior.cs
└── BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure/Caching/
    ├── HybridReadCache.cs          # HybridCache adapter, span tag
    └── ReadCacheRegistration.cs    # AddReadCache()
```

## Tests

- `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~Platform.Caching|FullyQualifiedName~UnitTests.Content"`
- `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj` (`Caching/`: libraries only in the adapter, every Content command drops the tag).

## Related sources

- [PLT-005](../../specs/platform/005-read-cache/README.md), [ADR-0021](../../architecture/decisions/0021-read-cache-hybridcache.md), [module rules: caching reads](../../standards/module-rules.md#caching-reads)
