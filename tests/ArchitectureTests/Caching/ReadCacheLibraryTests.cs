using System.Text.RegularExpressions;

namespace ArchitectureTests.Caching;

/// <summary>
/// Caching goes through one port (ADR-0021): modules and hosts use <c>IReadCache</c>; only
/// <c>BuildingBlocks.Infrastructure/Caching</c> touches <c>HybridCache</c>, <c>IMemoryCache</c> or <c>IDistributedCache</c>.
/// One way to name, expire, invalidate and trace a cached read, and one place to add an L2.
/// </summary>
public sealed partial class ReadCacheLibraryTests
{
    private static readonly string Adapter = Path.Combine(
        "src", "BuildingBlocks", "BuildingBlocks.Infrastructure", "BuildingBlocks.Infrastructure", "Caching") + Path.DirectorySeparatorChar;

    [Test]
    public void Only_the_caching_adapter_uses_cache_libraries()
    {
        var root = CachingRoot.Find();
        var offending = Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(root, path))
            .Where(path => !path.Split(Path.DirectorySeparatorChar).Any(part => part is "bin" or "obj"))
            .Where(path => !path.StartsWith(Adapter, StringComparison.Ordinal))
            .Where(path => File.ReadLines(Path.Combine(root, path))
                .Where(line => !line.TrimStart().StartsWith("//", StringComparison.Ordinal))
                .Any(line => CacheLibrary().IsMatch(line)))
            .Order()
            .ToArray();

        Assert.That(offending, Is.Empty);
    }

    [GeneratedRegex(@"\b(IMemoryCache|IDistributedCache|HybridCache|AddMemoryCache|AddDistributedMemoryCache)\b|Microsoft\.Extensions\.Caching")]
    private static partial Regex CacheLibrary();
}
