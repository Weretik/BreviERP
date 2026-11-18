using BuildingBlocks.Application.Caching;
using BuildingBlocks.Infrastructure.Caching;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTests.TestSupport;

/// <summary>The production read cache (HybridCache in memory, ADR-0021), fresh for each test.</summary>
internal static class TestReadCache
{
    public static IReadCache Create()
        => new ServiceCollection().AddLogging().AddReadCache().BuildServiceProvider().GetRequiredService<IReadCache>();
}
