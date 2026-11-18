namespace ArchitectureTests.Caching;

/// <summary>
/// The public site reads of Content are cached under one tag (<c>PublicSiteCache.Tag</c>, ADR-0021). Every Content
/// command changes something the site shows, so every one must drop that tag; a new command that forgets it would leave
/// the site stale for the cache lifetime.
/// </summary>
public sealed class ContentCacheInvalidationTests
{
    [Test]
    public void Every_content_command_drops_the_public_site_cache()
    {
        var root = CachingRoot.Find();
        var features = Path.Combine(root, "src", "Modules", "Content", "Content.Application", "Content.Application", "Features");
        var commands = Directory.EnumerateFiles(features, "*Command.cs", SearchOption.AllDirectories)
            .Where(path => path.Split(Path.DirectorySeparatorChar).Contains("Commands"))
            .ToArray();
        var offending = commands
            .Where(path => !File.ReadAllText(path).Contains("IInvalidatesReadCache", StringComparison.Ordinal)
                           || !File.ReadAllText(path).Contains("PublicSiteCache.Tag", StringComparison.Ordinal))
            .Select(path => Path.GetRelativePath(root, path))
            .Order()
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(commands, Is.Not.Empty);
            Assert.That(offending, Is.Empty);
        });
    }
}

internal static class CachingRoot
{
    public static string Find()
    {
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root != null && !File.Exists(Path.Combine(root.FullName, "Directory.Packages.props"))) root = root.Parent;
        Assert.That(root, Is.Not.Null);
        return root!.FullName;
    }
}
