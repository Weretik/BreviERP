using Ardalis.Result;
using BuildingBlocks.Application.Behaviors;
using BuildingBlocks.Application.Caching;
using Mediator;

namespace UnitTests.Platform.Caching;

public sealed class ReadCacheInvalidationBehaviorTests
{
    [Test]
    public async Task Successful_command_drops_its_tags()
    {
        var cache = new RecordingReadCache();

        await new ReadCacheInvalidationBehavior<Save, Result>(cache)
            .Handle(new Save(), (_, _) => ValueTask.FromResult(Result.Success()), CancellationToken.None);

        Assert.That(cache.Invalidated, Is.EqualTo(new[] { "a", "b" }));
    }

    [Test]
    public async Task Failed_command_keeps_the_cache()
    {
        var cache = new RecordingReadCache();

        await new ReadCacheInvalidationBehavior<Save, Result>(cache)
            .Handle(new Save(), (_, _) => ValueTask.FromResult(Result.Conflict("changed")), CancellationToken.None);

        Assert.That(cache.Invalidated, Is.Empty);
    }

    [Test]
    public async Task Command_without_tags_keeps_the_cache()
    {
        var cache = new RecordingReadCache();

        await new ReadCacheInvalidationBehavior<Plain, Result>(cache)
            .Handle(new Plain(), (_, _) => ValueTask.FromResult(Result.Success()), CancellationToken.None);

        Assert.That(cache.Invalidated, Is.Empty);
    }

    [Test]
    public void Entry_key_carries_the_variant_and_needs_a_positive_lifetime()
    {
        Assert.Multiple(() =>
        {
            Assert.That(ReadCacheEntry.Create("x.read", "x", TimeSpan.FromSeconds(1), "ru").Key, Is.EqualTo("x.read:ru"));
            Assert.That(ReadCacheEntry.Create("x.read", "x", TimeSpan.FromSeconds(1)).Key, Is.EqualTo("x.read"));
            Assert.That(() => ReadCacheEntry.Create("x.read", "x", TimeSpan.Zero), Throws.InstanceOf<ArgumentOutOfRangeException>());
        });
    }

    public sealed record Save : ICommand<Result>, IInvalidatesReadCache
    {
        public IReadOnlyCollection<string> InvalidatedCacheTags => ["a", "b"];
    }

    public sealed record Plain : ICommand<Result>;

    private sealed class RecordingReadCache : IReadCache
    {
        public List<string> Invalidated { get; } = [];

        public ValueTask<T> GetOrCreateAsync<T>(ReadCacheEntry entry, Func<CancellationToken, ValueTask<T>> read,
            CancellationToken cancellationToken) => read(cancellationToken);

        public ValueTask InvalidateAsync(string tag, CancellationToken cancellationToken)
        {
            Invalidated.Add(tag);
            return ValueTask.CompletedTask;
        }
    }
}
