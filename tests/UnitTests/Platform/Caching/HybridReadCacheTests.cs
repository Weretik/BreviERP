using System.ComponentModel;
using System.Diagnostics;
using BuildingBlocks.Application.Caching;
using UnitTests.TestSupport;

namespace UnitTests.Platform.Caching;

/// <summary>The HybridCache adapter behind <c>IReadCache</c> (ADR-0021), as the modules register it.</summary>
public sealed class HybridReadCacheTests
{
    private static readonly ReadCacheEntry Entry = ReadCacheEntry.Create("test.read", "test:tag", TimeSpan.FromMinutes(1));
    private static readonly CancellationToken None = CancellationToken.None;

    [Test]
    public async Task Second_read_comes_from_memory_as_the_same_instance()
    {
        var cache = TestReadCache.Create();
        var reads = 0;

        var first = await cache.GetOrCreateAsync(Entry, _ => ValueTask.FromResult(new Answer(++reads)), None);
        var second = await cache.GetOrCreateAsync(Entry, _ => ValueTask.FromResult(new Answer(++reads)), None);

        Assert.Multiple(() =>
        {
            Assert.That(reads, Is.EqualTo(1));
            Assert.That(second, Is.SameAs(first));
        });
    }

    [Test]
    public async Task Dropped_tag_reads_again_and_another_tag_does_not()
    {
        var cache = TestReadCache.Create();
        var reads = 0;
        await cache.GetOrCreateAsync(Entry, _ => ValueTask.FromResult(new Answer(++reads)), None);

        await cache.InvalidateAsync("other:tag", None);
        await cache.GetOrCreateAsync(Entry, _ => ValueTask.FromResult(new Answer(++reads)), None);
        await cache.InvalidateAsync("test:tag", None);
        var after = await cache.GetOrCreateAsync(Entry, _ => ValueTask.FromResult(new Answer(++reads)), None);

        Assert.That(after.Value, Is.EqualTo(2));
    }

    [Test]
    public async Task Read_tags_the_request_span_with_hit_or_miss()
    {
        var cache = TestReadCache.Create();
        using var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
        };
        ActivitySource.AddActivityListener(listener);
        using var source = new ActivitySource("UnitTests.Caching");

        object? missTag, hitTag;
        using (var miss = source.StartActivity("miss"))
        {
            await cache.GetOrCreateAsync(Entry, _ => ValueTask.FromResult(new Answer(1)), None);
            missTag = miss!.GetTagItem("cache.test.read");
        }

        using (var hit = source.StartActivity("hit"))
        {
            await cache.GetOrCreateAsync(Entry, _ => ValueTask.FromResult(new Answer(2)), None);
            hitTag = hit!.GetTagItem("cache.test.read");
        }

        Assert.Multiple(() =>
        {
            Assert.That(missTag, Is.EqualTo("miss"));
            Assert.That(hitTag, Is.EqualTo("hit"));
        });
    }

    [ImmutableObject(true)]
    public sealed record Answer(int Value);
}
