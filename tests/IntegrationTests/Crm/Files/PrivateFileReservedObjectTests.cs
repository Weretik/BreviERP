using Ardalis.Result;
using Crm.Infrastructure.Files.Storage;
using IntegrationTests.Crm.Files.Support;

namespace IntegrationTests.Crm.Files;

public sealed class PrivateFileReservedObjectTests
{
    [Test]
    public async Task Expiry_between_staging_and_rename_prevents_a_late_object_commit()
    {
        var workspace = Path.Combine(Path.GetTempPath(), "brevi-ts007-expiry-" + Guid.NewGuid().ToString("N"));
        try
        {
            var clock = new ExpiringClock();
            var store = new FileSystemPrivateFileStore(new PrivateFileStorageOptions
            {
                RootPath = Path.Combine(workspace, "private"), PublicWebRootPath = Path.Combine(workspace, "wwwroot")
            }, clock);
            var root = Guid.NewGuid();
            var file = Guid.NewGuid();
            var result = await store.WriteReservedAsync(root, file, new MemoryStream(FileLifecycleTestData.Pdf),
                "application/pdf", clock.Initial.AddMinutes(1), default);
            Assert.That(result.Status, Is.EqualTo(ResultStatus.Conflict));
            var scope = Path.Combine(workspace, "private", root.ToString("N"));
            Assert.That(Directory.Exists(Path.Combine(scope, file.ToString("N"))), Is.False);
            Assert.That(Directory.Exists(Path.Combine(scope, ".pending-" + file.ToString("N"))), Is.False);
        }
        finally { if (Directory.Exists(workspace)) Directory.Delete(workspace, true); }
    }

    [Test]
    public async Task Cleanup_removes_only_known_reserved_final_and_pending_paths()
    {
        var workspace = Path.Combine(Path.GetTempPath(), "brevi-ts007-pending-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new FileSystemPrivateFileStore(new PrivateFileStorageOptions
            {
                RootPath = Path.Combine(workspace, "private"), PublicWebRootPath = Path.Combine(workspace, "wwwroot")
            });
            var root = Guid.NewGuid();
            var file = Guid.NewGuid();
            Assert.That((await store.WriteReservedAsync(root, file, new MemoryStream(FileLifecycleTestData.Pdf),
                "application/pdf", DateTimeOffset.UtcNow.AddHours(1), default)).IsSuccess, Is.True);
            var scope = Path.Combine(workspace, "private", root.ToString("N"));
            var pending = Path.Combine(scope, ".pending-" + file.ToString("N"));
            var unrelated = Path.Combine(scope, ".pending-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(pending);
            Directory.CreateDirectory(unrelated);
            await File.WriteAllTextAsync(Path.Combine(pending, "content"), "Interrupted reserved write");
            Assert.That((await store.DeleteTemporaryAsync(root, file, default)).IsSuccess, Is.True);
            Assert.That(Directory.Exists(pending), Is.False);
            Assert.That(Directory.Exists(Path.Combine(scope, file.ToString("N"))), Is.False);
            Assert.That(Directory.Exists(unrelated), Is.True);
        }
        finally { if (Directory.Exists(workspace)) Directory.Delete(workspace, true); }
    }

    [Test]
    public void Lazy_unconfigured_storage_does_not_swallow_cancellation()
    {
        var store = new ConfiguredPrivateFileStore(new PrivateFileStorageOptions());
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Assert.CatchAsync<OperationCanceledException>(async () => await store.PrepareAsync(cancellation.Token));
        Assert.CatchAsync<OperationCanceledException>(async () => await store.ReadAsync(Guid.NewGuid(), Guid.NewGuid(), cancellation.Token));
    }

    private sealed class ExpiringClock : TimeProvider
    {
        internal DateTimeOffset Initial { get; } = DateTimeOffset.UtcNow;
        private int calls;
        public override DateTimeOffset GetUtcNow() => Interlocked.Increment(ref calls) == 1 ? Initial : Initial.AddHours(1);
    }
}
